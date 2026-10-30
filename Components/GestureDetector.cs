using System;
using Godot;

/// <summary>
/// Turns presses on its parent control into gestures: tap, long press, swipe and drag.
/// Add it as a child of the control to watch. It has no area of its own (it listens to the parent's input),
/// so it never covers other controls. Distances are in screen pixels, whatever the parent's scale.
/// A move that isn't a swipe or a drag scrolls the nearest ScrollContainer, like mobile lists. Inside a
/// <see cref="DragScrollContainer"/> the container scrolls itself, and a press it scrolled with isn't a tap.
/// </summary>
[GlobalClass]
public partial class GestureDetector : Node
{
    public enum SwipeDirection { Up, Down, Left, Right }

    enum State { Idle, Pressed, LongPressed, SwipeCandidate, Scrolling, Dragging }

    /// <summary>How far the pointer moves before a press stops being a tap.</summary>
    [Export]
    public float MoveTolerance { get; set; } = 12;

    /// <summary>Seconds a still press is held before it's a long press.</summary>
    [Export]
    public float LongPressTime { get; set; } = 0.5f;

    /// <summary>Whether fast flicks are reported as <see cref="Swiped"/> instead of scrolling.</summary>
    [Export]
    public bool Swipes { get; set; }

    [Export]
    public float SwipeDistance { get; set; } = 60;

    [Export]
    public float SwipeMaxTime { get; set; } = 0.35f;

    /// <summary>Whether sideways flicks are swipes too; otherwise only up and down are, and sideways moves scroll.</summary>
    [Export]
    public bool HorizontalSwipes { get; set; }

    /// <summary>
    /// Whether moves are reported as drags (<see cref="DragStarted"/>...). A move along the axis the nearest
    /// scroll list scrolls on still scrolls it, so a list of draggable cards stays scrollable.
    /// </summary>
    [Export]
    public bool Drags { get; set; }

    /// <summary>
    /// Whether moves that aren't swipes or drags scroll the nearest ScrollContainer. Off for things dragged
    /// out of a list in any direction (they drag even along the list's scroll axis).
    /// </summary>
    [Export]
    public bool ScrollsParent { get; set; } = true;

    Control? target;
    ScrollContainer? scroll;
    State state = State.Idle;
    Vector2 pressPosition;
    Vector2 pendingScroll;
    double heldTime;

    public event Action? Tapped;
    public event Action? LongPressed;
    public event Action<SwipeDirection>? Swiped;
    /// <summary>Global position where the press started.</summary>
    public event Action<Vector2>? DragStarted;
    /// <summary>Current global pointer position.</summary>
    public event Action<Vector2>? DragMoved;
    /// <summary>Global position of the release.</summary>
    public event Action<Vector2>? DragEnded;
    /// <summary>The drag stopped without a release (<see cref="Cancel"/>, or the detector left the tree).</summary>
    public event Action? DragCancelled;

    public bool IsDragging => state == State.Dragging;

    public override void _EnterTree()
    {
        target = GetParent() as Control;
        if (target is not null)
        {
            target.GuiInput += OnGuiInput;
        }
        SetProcess(false);
    }

    public override void _ExitTree()
    {
        Cancel();
        if (target is not null)
        {
            target.GuiInput -= OnGuiInput;
        }
        target = null;
    }

    /// <summary>Forgets the current press without reporting it, e.g. when a dialog opens mid-press.</summary>
    public void Cancel()
    {
        var wasDragging = state == State.Dragging;
        Reset();
        if (wasDragging)
        {
            DragCancelled?.Invoke();
        }
    }

    public override void _Process(double delta)
    {
        heldTime += delta;

        if (state == State.Pressed && heldTime >= LongPressTime)
        {
            state = State.LongPressed;
            LongPressed?.Invoke();
        }
        else if (state == State.SwipeCandidate && heldTime > SwipeMaxTime)
        {
            // Too slow for a flick: it's a scroll after all
            StartScrolling();
        }
    }

    private void OnGuiInput(InputEvent e)
    {
        switch (e)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } press:
                Begin(press.GlobalPosition);
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false } release:
                End(release.GlobalPosition);
                break;
            case InputEventMouseMotion motion when state != State.Idle:
                Move(motion.GlobalPosition, motion.Relative);
                break;
        }
    }

    private void Begin(Vector2 position)
    {
        state = State.Pressed;
        pressPosition = position;
        pendingScroll = Vector2.Zero;
        heldTime = 0;
        scroll = FindScroll();
        SetProcess(true);
    }

    private void Move(Vector2 position, Vector2 relative)
    {
        switch (state)
        {
            case State.Scrolling:
                ScrollBy(relative);
                return;
            case State.Dragging:
                DragMoved?.Invoke(position);
                return;
            case State.SwipeCandidate:
                pendingScroll += relative;
                return;
            case State.Pressed:
                pendingScroll += relative;
                Classify(position);
                return;
        }
    }

    // Decides what a press that moved past the tolerance is
    private void Classify(Vector2 position)
    {
        var moved = position - pressPosition;
        if (moved.Length() < MoveTolerance) return;

        bool vertical = Mathf.Abs(moved.Y) > Mathf.Abs(moved.X);
        if (Swipes && heldTime <= SwipeMaxTime && (vertical || HorizontalSwipes))
        {
            // Wait for the release (a flick) or for the time to run out (a scroll)
            state = State.SwipeCandidate;
            return;
        }

        if (Drags && !ScrollsAlong(vertical))
        {
            state = State.Dragging;
            // The list mustn't scroll under a dragged item
            (scroll as DragScrollContainer)?.Release();
            DragStarted?.Invoke(pressPosition);
            DragMoved?.Invoke(position);
            return;
        }

        StartScrolling();
    }

    private void End(Vector2 position)
    {
        var moved = position - pressPosition;
        switch (state)
        {
            case State.Pressed when !ScrolledByContainer():
                Tapped?.Invoke();
                break;
            case State.SwipeCandidate when IsSwipe(moved):
                Swiped?.Invoke(DirectionOf(moved));
                break;
            case State.Dragging:
                state = State.Idle;
                DragEnded?.Invoke(position);
                break;
        }
        Reset();
    }

    private bool IsSwipe(Vector2 moved)
    {
        if (heldTime > SwipeMaxTime) return false;
        var along = Mathf.Max(Mathf.Abs(moved.X), Mathf.Abs(moved.Y));
        var across = Mathf.Min(Mathf.Abs(moved.X), Mathf.Abs(moved.Y));
        bool vertical = Mathf.Abs(moved.Y) >= Mathf.Abs(moved.X);
        // Clearly along one axis, far enough, and an axis that swipes
        return along >= SwipeDistance && along > across * 1.5f && (vertical || HorizontalSwipes);
    }

    private static SwipeDirection DirectionOf(Vector2 moved)
    {
        if (Mathf.Abs(moved.Y) >= Mathf.Abs(moved.X))
        {
            return moved.Y < 0 ? SwipeDirection.Up : SwipeDirection.Down;
        }
        return moved.X < 0 ? SwipeDirection.Left : SwipeDirection.Right;
    }

    private void StartScrolling()
    {
        state = State.Scrolling;
        ScrollBy(pendingScroll);
        pendingScroll = Vector2.Zero;
    }

    private void ScrollBy(Vector2 relative)
    {
        // A DragScrollContainer follows the pointer itself
        if (scroll is null || scroll is DragScrollContainer) return;
        scroll.ScrollHorizontal -= (int)relative.X;
        scroll.ScrollVertical -= (int)relative.Y;
    }

    private bool ScrollsAlong(bool vertical)
    {
        if (scroll is null) return false;
        return vertical
            ? scroll.VerticalScrollMode != ScrollContainer.ScrollMode.Disabled
            : scroll.HorizontalScrollMode != ScrollContainer.ScrollMode.Disabled;
    }

    private bool ScrolledByContainer()
    {
        return scroll is DragScrollContainer container && container.LastGestureWasDrag;
    }

    private ScrollContainer? FindScroll()
    {
        if (!ScrollsParent) return null;
        for (Node? node = target?.GetParent(); node is not null; node = node.GetParent())
        {
            if (node is ScrollContainer found) return found;
        }
        return null;
    }

    private void Reset()
    {
        state = State.Idle;
        scroll = null;
        SetProcess(false);
    }
}
