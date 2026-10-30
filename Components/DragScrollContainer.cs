using Godot;

/// <summary>
/// A ScrollContainer that also scrolls by dragging anywhere on it, buttons included, like mobile lists.
/// It doesn't take the input away from its children, so a button still gets its release after a drag:
/// check <see cref="LastGestureWasDrag"/> in the button's Pressed handler to ignore those.
/// Any press inside its rect starts tracking, even one on a control drawn over it.
/// </summary>
[Tool]
public partial class DragScrollContainer : ScrollContainer
{
    bool tracking, dragging;
    Vector2 pressPosition;

    /// <summary>How far the pointer moves before a press becomes a drag.</summary>
    [Export]
    public float DragThreshold { get; set; } = 16;

    /// <summary>True from the moment the current press turned into a drag until the next press.</summary>
    public bool LastGestureWasDrag { get; private set; }

    /// <summary>
    /// Stops following the current press, e.g. when a child starts dragging an item out of the list.
    /// The press still counts as a drag, so buttons ignore its release.
    /// </summary>
    public void Release()
    {
        if (tracking)
        {
            LastGestureWasDrag = true;
        }
        tracking = false;
        dragging = false;
    }

    public override void _Input(InputEvent e)
    {
        if (Engine.IsEditorHint() || !IsVisibleInTree()) return;

        switch (e)
        {
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true } press when GetGlobalRect().HasPoint(press.GlobalPosition):
                tracking = true;
                dragging = false;
                LastGestureWasDrag = false;
                pressPosition = press.GlobalPosition;
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false }:
                tracking = false;
                dragging = false;
                break;
            case InputEventMouseMotion motion when tracking:
                if (!dragging && motion.GlobalPosition.DistanceTo(pressPosition) > DragThreshold)
                {
                    dragging = true;
                    LastGestureWasDrag = true;
                }
                if (dragging)
                {
                    ScrollHorizontal -= (int)motion.Relative.X;
                    ScrollVertical -= (int)motion.Relative.Y;
                }
                break;
        }
    }
}
