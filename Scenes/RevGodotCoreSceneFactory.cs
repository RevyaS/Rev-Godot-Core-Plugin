using static SceneHelper;

public partial class RevGodotCoreSceneFactory
{
    public static LabelValueContainer CreateLabelValueContainer(string name, string value)
    {
        var scene = GetScene<LabelValueContainer>("res://addons/rev_godot_core/Scenes/LabelValueContainer.tscn");
        scene.Configure(name, value);
        return scene;
    }
}