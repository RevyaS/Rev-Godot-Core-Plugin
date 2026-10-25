using System;
using System.Collections.Generic;
using static SceneHelper;

public partial class RevGodotCoreSceneFactory
{
    public static LabelValueContainer CreateLabelValueContainer(string name, string value)
    {
        var scene = GetScene<LabelValueContainer>("res://addons/rev_godot_core/Scenes/LabelValueContainer.tscn");
        scene.Configure(name, value);
        return scene;
    }

    public static SlotSelectorDialog CreateSlotSelectorDialog(IReadOnlyList<SaveSlot> slots, SlotSelectorMode mode, Func<SaveSlot, string>? describe = null)
    {
        var scene = GetScene<SlotSelectorDialog>("res://addons/rev_godot_core/Scenes/SlotSelectorDialog.tscn");
        scene.Configure(slots, mode, describe);
        return scene;
    }
}
