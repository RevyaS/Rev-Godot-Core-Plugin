using Godot;

[Tool]
public partial class LabelValueContainer : HBoxContainer
{
	string labelText = null!, valueText = null!;
	Label label = null!, labelValue = null!;
	int fontSize = 16;

	[Export]
	public int FontSize
    {
        get => fontSize;
		set
        {
            fontSize = value;
			Render();
        }
    }

	[Export]
	public string Label
    {
        get => labelText;
		set
        {
            labelText = value;
			Render();
        }
    }

	[Export]
	public string Value
    {
        get => valueText;
		set
        {
            valueText = value;
			Render();
        }
    }
	
	public override void _Ready()
	{
		label = GetNode<Label>("%Label");
		labelValue = GetNode<Label>("%LabelValue");

		Render();
	}

	public void Render() {
		if(!IsInsideTree()) return;

		label.RemoveThemeFontSizeOverride("font_size");
		label.AddThemeFontSizeOverride("font_size", FontSize);

		labelValue.RemoveThemeFontSizeOverride("font_size");
		labelValue.AddThemeFontSizeOverride("font_size", FontSize);

		labelValue.Text = valueText;
		label.Text = labelText;
	}

	public void Configure(string label, string value) {
		labelText = label;
		valueText = value;
		Render();
	}
}