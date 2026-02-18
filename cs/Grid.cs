#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace InventoryConcept.cs;

public readonly struct Grid<TCellContents>
{
    public delegate bool Populator(int idx, int x, int y, out TCellContents cell);
    
    public int Width { get; init; }
    public int Height { get; init; }
    private readonly Cell<TCellContents>[] _cells;
    
    public Grid(int width, int height, Populator populator)
    {
        Width = Math.Abs(width);
        Height = Math.Abs(height);
        
        _cells = new Cell<TCellContents>[Width * Height];
        
        for (int idx = 0; idx < _cells.Length; idx++)
        {
            if (populator(idx, (idx % Width), (idx / Width), out var contents))
            {
                _cells[idx] = Cell<TCellContents>.Filled(contents);
            }
            else
            {
                _cells[idx] = Cell<TCellContents>.Empty;
            }
        }
    }
    
    private Vector2I IndexToCoord(int index) => new Vector2I(index % Width, index / Width);
    
    private int CoordToIndex(int x, int y) => (y * Width) + x;
    private int CoordToIndex(Vector2I coords) => (Math.Abs(coords.Y) * Width) + Math.Abs(coords.X);
    
    public Vector2I BoundingCellCoords => new Vector2I(Width-1, Height-1);
    
    public bool IsCellEmpty(int x, int y)
    {
        if (x >= Width || y >= Height) return true;
        return !_cells[CoordToIndex(x, y)].HasValue;
    }

    public bool IsCellEmpty(Vector2I coords)
    {
        if (coords.X >= Width || coords.Y >= Height) return true;
        return !_cells[CoordToIndex(coords)].HasValue;
    }

    public bool TryGetCell(int x, int y, [MaybeNullWhen(false)] out TCellContents value)
    {
        int idx = CoordToIndex(x, y);

        if (idx < 0 || idx >= _cells.Length || x >= Width || y >= Height)
        {
            value = default!;
            return false;
            // This would be neat, but its actually a lot more valuable for this to return false and move on.
            // See: Comparing two differently sized grids against one another in order to lossily preserve as much data as possible.
            if (x >= Width) throw new ArgumentOutOfRangeException(nameof(x), $"Spacial grid index {idx} from grid pos (>{x}<, {y}) is out of range.");
            if (y >= Height) throw new ArgumentOutOfRangeException(nameof(y), $"Spacial grid index {idx} from grid pos ({x}, >{y}<) is out of range.");
        }
        
        return Cell<TCellContents>.TryGetCellValue(_cells[idx], out value);
    }
    
    public bool TryGetCell(Vector2I coords, [MaybeNullWhen(false)] out TCellContents value)
    {
        int idx = CoordToIndex(coords);

        if (idx < 0 || idx >= _cells.Length || coords.X >= Width || coords.Y >= Height)
        {
            value = default!;
            return false;
            // See: above TryGetCell comment.
            throw new ArgumentOutOfRangeException(nameof(coords), $"Spacial grid index {idx} from grid pos ({coords}) is out of range.");
        }

        return Cell<TCellContents>.TryGetCellValue(_cells[idx], out value);
    }

    public bool SetCell(int x, int y, TCellContents cell)
    {
        if (x >= Width || y >= Height || !_cells[CoordToIndex(x, y)].HasValue) return false;
        _cells[CoordToIndex(x, y)] = Cell<TCellContents>.Filled(cell);
        return true;
    }
    
    public bool SetCell(Vector2I coords, TCellContents cell)
    {
        if (Math.Abs(coords.X) >= Width || Math.Abs(coords.Y) >= Height || !_cells[CoordToIndex(coords)].HasValue) return false;
        _cells[CoordToIndex(coords)] = Cell<TCellContents>.Filled(cell);
        return true;
    }
    
    /*Out values can be meaningless if the grid cell was not created by Populator. E.g. a default value.*/
    public TCellContents[] GetCellAllCellsUnsafe()
    {
        TCellContents[] output = new TCellContents[_cells.Length];
        for (int i = 0; i < _cells.Length; i++)
        {
            output[i] = _cells[i].Value;
        }
        return output;
    }

    public IEnumerable<TCellContents> GetAllCells()
    {
        foreach (var cell in _cells)
        {
            if (cell.HasValue) yield return cell.Value;
        }
    }
    
    public List<TCellContents> GetCellsFromCoords(Vector2I[] coords)
    {
        List<TCellContents> output = new();
        foreach (var coord in coords)
        {
            if (TryGetCell(coord, out var cell)) output.Add(cell);
        }
        return output;
    }
    
    /*
     * STEPS TO ROTATE GRID BY 90
     * - For every grid cell, rotate by 90 degrees (RadToDeg) 
     * - Offset to positive-only coordinate space by adding Width-1 to X value
     * - Now we have a map of current grid XY -> rotated grid XY
     * - Make new grid using swapped width and height and populator
     * - Populator method should use ask new grid 
     *
     *
     * 
     */
    
    // Returns a newly created grid rotated by the right angle amount specified
    public Grid<TCellContents> GetRotatedGrid(GridRotationTypeEnum rotationType = GridRotationTypeEnum.Clockwise)
    {
        Dictionary<Vector2I, Vector2I> transformedCells = new(); // Rotated XY -> old XY
        Vector2 offset = rotationType == GridRotationTypeEnum.Clockwise
            ? new Vector2(Height - 1, 0f)
            : new Vector2(0f, Height - 1);
        for (int i = 0; i < _cells.Length; i++)
        {
            var cellCoord = IndexToCoord(i);
            var vecRot = ((Vector2)cellCoord).Rotated(Mathf.DegToRad((float)rotationType));
            vecRot += offset;
            transformedCells.TryAdd((Vector2I)vecRot.Abs().Round(), cellCoord);
        }
        var self = this;
        Grid<TCellContents> rotatedGrid = new Grid<TCellContents>(Height, Width,
            (int idx, int x, int y, out TCellContents cell) =>
            {
                if (self.TryGetCell(transformedCells.GetValueOrDefault(new Vector2I(x, y), Vector2I.Zero), out var value))
                {
                    cell = value; 
                    return true;
                }
                cell = default!;
                return false;
            });
        return rotatedGrid;
    }

    public enum GridRotationTypeEnum
    {
        Clockwise = 90,
        CounterClockwise = -90,
    }
    
    internal readonly struct Cell<T>
    {
        public bool HasValue { get; }
        public T Value { get; }

        private Cell(bool hasValue, T value)
        {
            HasValue = hasValue;
            Value = value;
        }

        public static bool TryGetCellValue(Cell<T> cell, out T value)
        {
            if (cell.HasValue)
            {
                value = cell.Value;
                return true;
            }

            value = default!;
            return false;
        }

        public static Cell<T> Empty => new(false, default!);
        public static Cell<T> Filled(T value) => new(true, value);
    }
}