using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security.Principal;
using Godot;
using Color = Godot.Color;

namespace InventoryConcept.cs;

[GlobalClass]
public partial class Inventory : Control
{
    //[Export] public int Width = 6;
    //[Export] public int Height = 6;
    [Export] public GridShape GridShape = new GridShape();
    [Export] public int CellWidth = 32;
    [Export] public int CellHeight = 32;
    [Export] public Color CellColor = Colors.LightSlateGray;
    [Export] public Color DisabledCellColor = Colors.DarkSlateGray;
    
    [Export] public StyleBox SlotEmptyStyle;
    [Export] public StyleBox SlotFullStyle;
    [Export] public StyleBox SlotHoveredStylePositive;
    [Export] public StyleBox SlotHoveredStyleNeutral;
    [Export] public StyleBox SlotHoveredStyleNegative;
    
    //[Export] public Color SlotHoverColorPositive = Colors.SeaGreen; 780883967U
    //[Export] public Color SlotHoverColorNeutral = Colors.Chocolate; 3530104575U
    //[Export] public Color SlotHoverColorNegative = Colors.Firebrick; 2988581631U

    [Export] public Label Pos1;
    [Export] public Label Pos2;
    [Export] public Label Pos3;
    
    private Grid<InventorySlot> _grid;
    private Vector2I _maxBound;
    private Rect2 _bounds;
    private InventorySlot? _hoveredSlot = null;

    private bool _initialized = false;
    
    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        
        var gridShape = GridShape.ToGrid();
        var rotatedGridShape = gridShape.GetRotatedGrid();
        _grid = new Grid<InventorySlot>(rotatedGridShape.Width, rotatedGridShape.Height,
            (int idx, int x, int y, out InventorySlot cell) =>
            {
                cell = new InventorySlot(false, x, y);
                return rotatedGridShape.TryGetCell(x, y, out bool val) && val;
            });
        
        _maxBound = new Vector2I(rotatedGridShape.Width * CellWidth, rotatedGridShape.Height * CellHeight);
        _bounds = new Rect2(GetGlobalRect().Position, _maxBound);
        
        Pos1.Text = _bounds.ToString() + GetGlobalRect().Position;
        _initialized = true;
        QueueRedraw();
    }
    
    public override void _Process(double delta)
    {   
        
    }

    public bool IsHovered => _hoveredSlot != null;

    private InventorySlot? HoveredSlot
    {
        get => _hoveredSlot;
        set
        {
            _hoveredSlot = value;
            QueueRedraw();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            var mousePos = eventMouseMotion.Position;
            Pos2.Text = mousePos.ToString();
            if (GetSlotAtPosition(mousePos, out InventorySlot foundSlot))
            {
                if (HoveredSlot == null)
                {
                    foundSlot.IsHovered = true;
                    HoveredSlot = foundSlot;
                } else if (foundSlot.GridPosition != HoveredSlot.Value.GridPosition)
                {
                    var oldSlot = HoveredSlot.Value;
                    oldSlot.IsHovered = false;
                    HoveredSlot = foundSlot;
                }
                Pos3.Text = "Hovered Cell: " + _hoveredSlot.Value.GridPosition.ToString();
            }
            else
            {
                if (HoveredSlot != null)
                {
                    var oldSlot = HoveredSlot.Value;
                    oldSlot.IsHovered = false;
                    HoveredSlot = null;
                    Pos3.Text = "Hovered Cell: ";
                }
                
            }
        }
    }

    private bool GetSlotAtPosition(Vector2 position, [MaybeNullWhen(false)] out InventorySlot slot)
    {
        if (_bounds.HasPoint(position))
        {
            var offsetPosition = position - _bounds.Position;
            if (_grid.TryGetCell((int)offsetPosition.X / CellWidth, (int)offsetPosition.Y / CellHeight,
                    out InventorySlot foundSlot))
            {
                slot = foundSlot;
                return true;
            }
        }
        slot = default!;
        return false;
    }
    
    public override void _Draw()
    {
        DrawCircle(Vector2.Zero, 3f, Colors.Crimson, true);
        if (!_initialized) return;
        foreach (var slot in _grid.GetAllCells())
        {
            var gridPos = slot.GridPosition;
            DrawStyleBox(slot.IsFilled ? SlotFullStyle : SlotEmptyStyle, new Rect2(gridPos.X * CellWidth+2, gridPos.Y * CellHeight+2, CellWidth-4, CellHeight-4));

        }

        if (HoveredSlot != null) // Draw infill if slot is hovered
        {
            var gridPos = HoveredSlot.Value.GridPosition;
            DrawStyleBox(SlotHoveredStylePositive, new Rect2(gridPos.X * CellWidth+5, gridPos.Y * CellHeight+5, CellWidth-10, CellHeight-10));
        }
            
    }
    
    private RandomNumberGenerator _rng = new();
    
    private bool Populator(int idx, int x, int y, out InventorySlot cell)
    {
        if (_rng.Randf() > 0.75)
        {
            cell = new InventorySlot(true, x, y);;
            return false;
        }
        cell = new InventorySlot(false, x, y);
        return true;
    }
}


