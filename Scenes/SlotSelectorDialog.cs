using System;
using System.Collections.Generic;
using Godot;

public enum SlotSelectorMode
{
    SAVE,
    LOAD
}

/// <summary>
/// Lists save slots to pick one. In <see cref="SlotSelectorMode.LOAD"/> only occupied slots can be picked,
/// in <see cref="SlotSelectorMode.SAVE"/> any slot can, and an occupied one turns the OK button into "Overwrite".
/// </summary>
public partial class SlotSelectorDialog : ConfirmationDialog
{
    public event Action<SaveSlot>? SlotSelected;

    VBoxContainer slotList = null!;
    ButtonGroup slotGroup = new();
    IReadOnlyList<SaveSlot> slots = [];
    SlotSelectorMode mode;
    Func<SaveSlot, string> describe = DefaultDescription;
    SaveSlot? selected;

    public override void _Ready()
    {
        slotList = GetNode<VBoxContainer>("%SlotList");
        Confirmed += OnConfirmed;
        Render();
    }

    public void Configure(IReadOnlyList<SaveSlot> slots, SlotSelectorMode mode, Func<SaveSlot, string>? describe = null)
    {
        this.slots = slots;
        this.mode = mode;
        this.describe = describe ?? DefaultDescription;
        selected = null;
        Render();
    }

    public static string DefaultDescription(SaveSlot slot) =>
        slot.IsOccupied ? $"Slot {slot.Number} - {slot.LastModified:g}" : $"Slot {slot.Number} - Empty";

    private void Render()
    {
        if (!IsNodeReady()) return;

        Title = mode == SlotSelectorMode.SAVE ? "Save to Slot" : "Load Slot";

        foreach (var child in slotList.GetChildren())
        {
            slotList.RemoveChild(child);
            child.QueueFree();
        }

        slotGroup = new();
        foreach (var slot in slots)
        {
            var button = new Button
            {
                Text = describe(slot),
                ToggleMode = true,
                ButtonGroup = slotGroup,
                Alignment = HorizontalAlignment.Left,
                Disabled = mode == SlotSelectorMode.LOAD && !slot.IsOccupied
            };
            button.Pressed += () => SelectSlot(slot);
            slotList.AddChild(button);
        }

        RenderOkButton();
    }

    private void SelectSlot(SaveSlot slot)
    {
        selected = slot;
        RenderOkButton();
    }

    private void RenderOkButton()
    {
        var okButton = GetOkButton();
        okButton.Disabled = selected is null;
        okButton.Text = mode switch
        {
            SlotSelectorMode.SAVE => selected?.IsOccupied == true ? "Overwrite" : "Save",
            SlotSelectorMode.LOAD => "Load",
            _ => throw new NotSupportedException($"Mode Not Supported {mode}")
        };
    }

    private void OnConfirmed()
    {
        if (selected is not null) SlotSelected?.Invoke(selected);
    }
}
