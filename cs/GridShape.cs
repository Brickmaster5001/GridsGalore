using System;
using Godot;
using Godot.Collections;
using InventoryConcept.cs;

[GlobalClass]
[Tool]
public partial class GridShape : Resource
{
    [Export]
    public int[] GridShapeData { get; set; }
    
    public GridShape() : this(null) {}
    
    public GridShape(int[] gridArr)
    {
        GridShapeData = gridArr ?? [1, 1, 1];
    }

    public Grid<bool> ToGrid()
    {
        return UnpackGridShapeFromInt32Array(GridShapeData);
    }
    
    private Grid<bool> UnpackGridShapeFromInt32Array(int[] gridArr)
    {
        if (gridArr.Length < 3)
            throw new InvalidOperationException("Failed to unpack grid shape from int array, length was less than 3 and therefore cannot contain enough data.");
        
        return new Grid<bool>(gridArr[0], gridArr[1], (int idx, int x, int y, out bool cell) =>
        {
            //GD.Print($"Unpacking grid {idx} -> {gridArr[idx + 2]} == {gridArr[idx + 2] > 0}");
            cell = gridArr[idx + 2] > 0;
            return true;
        });
    }
}