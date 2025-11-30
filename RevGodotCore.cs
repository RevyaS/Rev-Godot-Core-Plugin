#if TOOLS
using Godot;
using System;

[Tool]
public partial class RevGodotCore : EditorPlugin
{
	public override void _EnterTree()
	{
		var nodeContainerScript = GD.Load<Script>("res:///addons/rev_godot_core/Components/NodeContainer.cs");
		var containerIcon = GD.Load<Texture2D>("res://addons/rev_godot_core/Assets/Icons/Container.svg");
		AddCustomType(nameof(NodeContainer), nameof(Control), nodeContainerScript, containerIcon);

		var vboxNodeContainerScript = GD.Load<Script>("res:///addons/rev_godot_core/Components/VBoxNodeContainer.cs");
		var vboxNodeContainerIcon = GD.Load<Texture2D>("res://addons/rev_godot_core/Assets/Icons/VBoxContainer.svg");
		AddCustomType(nameof(VBoxNodeContainer), nameof(VBoxContainer), vboxNodeContainerScript, vboxNodeContainerIcon);

		var hboxNodeContainerScript = GD.Load<Script>("res:///addons/rev_godot_core/Components/HBoxNodeContainer.cs");
		var hboxNodeContainerIcon = GD.Load<Texture2D>("res://addons/rev_godot_core/Assets/Icons/HBoxContainer.svg");
		AddCustomType(nameof(HBoxNodeContainer), nameof(HBoxContainer), hboxNodeContainerScript, hboxNodeContainerIcon);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
	}
}
#endif
