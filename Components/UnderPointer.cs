using System.Collections.Generic;
using Godot;

/// <summary>
/// Finds the control under a pointer by geometry instead of by which control receives input, so the
/// targets of a drag (e.g. from a <see cref="GestureDetector"/>) never have to catch the mouse and can't
/// block anything. Works with rotated and scaled controls.
/// </summary>
public static class UnderPointer
{
    public static bool Contains(Control control, Vector2 globalPoint)
    {
        if (!control.IsVisibleInTree()) return false;
        var local = control.GetGlobalTransform().AffineInverse() * globalPoint;
        return new Rect2(Vector2.Zero, control.Size).HasPoint(local);
    }

    /// <summary>The control under <paramref name="globalPoint"/>, the later one when they overlap.</summary>
    public static T? Find<T>(IEnumerable<T> controls, Vector2 globalPoint) where T : Control
    {
        T? found = null;
        foreach (var control in controls)
        {
            if (Contains(control, globalPoint))
            {
                found = control;
            }
        }
        return found;
    }
}
