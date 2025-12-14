using Godot;

public partial class LabelValueContainer : HBoxContainer
{
	string labelText, valueText;
	Label label, labelValue;
	
	public override void _Ready()
	{
		label = GetNode<Label>("%Label");
		labelValue = GetNode<Label>("%LabelValue");

		Render();
	}

	public void Render() {
		if(!IsInsideTree()) return;
		labelValue.Text = valueText;
		label.Text = labelText;
	}

	public void Configure(string label, string value) {
		labelText = label;
		valueText = value;
		Render();
	}
}