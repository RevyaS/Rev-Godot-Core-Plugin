#if TOOLS
using Godot;
using System;

[Tool]
public partial class RevGodotCore : EditorPlugin
{
	Button pivotButton = null!;

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

		pivotButton = new Button
        {
            Text = "Center Pivot",
            Visible = false, // Hidden until a Control is selected
            TooltipText = "Center Pivot Offset for this Control"
        };

        pivotButton.Pressed += OnPivotButtonPressed;
		AddControlToContainer(CustomControlContainer.CanvasEditorMenu, pivotButton);

		EditorInterface.Singleton.GetSelection().SelectionChanged += OnSelectionChanged;
	}

    private void OnSelectionChanged()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        if (selected.Count == 0)
        {
            pivotButton.Visible = false;
            return;
        }
        Node node = selected[0];
        // Only show button for Control nodes
        pivotButton.Visible = node is Control;
    }

    private void OnPivotButtonPressed()
    {
        var selected = EditorInterface.Singleton.GetSelection().GetSelectedNodes();
        if (selected.Count == 0) return;

        foreach (Node node in selected)
        {
            if (node is Control control)
            {
                control.PivotOffset = control.Size / 2;
            }
        }
    }

    public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveControlFromContainer(CustomControlContainer.CanvasEditorMenu, pivotButton);
        pivotButton.QueueFree();
	}
}
#endif
