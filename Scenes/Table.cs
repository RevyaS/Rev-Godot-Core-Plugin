using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>
/// A grid whose columns line up across rows: an optional header row, then rows of controls added with <see cref="AddRow"/>.
/// Setting <see cref="Headers"/> also sets the column count. Columns in <see cref="ExpandedColumns"/> take the remaining width.
/// </summary>
[Tool]
public partial class Table : GridContainer
{
    string[] headers = [];
    int headerFontSize = 0;
    int[] expandedColumns = [];
    readonly List<Label> headerLabels = [];
    readonly List<Control[]> rows = [];

    [Export]
    public string[] Headers
    {
        get => headers;
        set
        {
            headers = value;
            if (headers.Length > 0) Columns = headers.Length;
            Render();
        }
    }

    /// <summary>0 keeps the theme's font size.</summary>
    [Export]
    public int HeaderFontSize
    {
        get => headerFontSize;
        set
        {
            headerFontSize = value;
            Render();
        }
    }

    [Export]
    public int[] ExpandedColumns
    {
        get => expandedColumns;
        set
        {
            expandedColumns = value;
            Render();
        }
    }

    public IReadOnlyList<Control[]> Rows => rows;

    public override void _Ready()
    {
        Render();
    }

    public void AddRow(params Control[] cells)
    {
        if (cells.Length != Columns) throw new ArgumentException($"A row needs {Columns} cells, got {cells.Length}", nameof(cells));

        rows.Add(cells);
        for (int column = 0; column < cells.Length; column++)
        {
            ApplyColumn(column, cells[column]);
            AddChild(cells[column]);
        }
    }

    public void ClearRows()
    {
        foreach (var cell in rows.SelectMany(row => row))
        {
            RemoveChild(cell);
            cell.QueueFree();
        }
        rows.Clear();
    }

    private void Render()
    {
        if (!IsNodeReady()) return;

        foreach (var label in headerLabels)
        {
            RemoveChild(label);
            label.QueueFree();
        }
        headerLabels.Clear();

        for (int column = 0; column < headers.Length; column++)
        {
            var label = new Label { Text = headers[column] };
            if (headerFontSize > 0) label.AddThemeFontSizeOverride("font_size", headerFontSize);
            ApplyColumn(column, label);
            AddChild(label);
            MoveChild(label, column);
            headerLabels.Add(label);
        }

        foreach (var row in rows)
        {
            for (int column = 0; column < row.Length; column++) ApplyColumn(column, row[column]);
        }
    }

    private void ApplyColumn(int column, Control cell)
    {
        if (Array.IndexOf(expandedColumns, column) >= 0)
        {
            cell.SizeFlagsHorizontal |= SizeFlags.Expand;
        }
    }
}
