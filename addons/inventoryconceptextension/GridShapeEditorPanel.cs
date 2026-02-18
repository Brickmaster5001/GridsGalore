using Godot;
using System;
using System.Linq;
using InventoryConcept.cs;

[GlobalClass]
[Tool]
public partial class GridShapeEditorPanel : PanelContainer
{
    [Signal]
    public delegate void GridShapeEdittedEventHandler(int[] newValue);
    
    [Export] public Theme GridButtonTheme;
    [Export] public GridContainer GridContainer;
    [Export] public EditorSpinSlider HeightSlider;
    [Export] public EditorSpinSlider WidthSlider;
    [Export] public ScrollContainer GridScroll;
    [Export] public Button ClearButton;
    [Export] public Button FillButton;

    private ulong _lastTransformChangedNotif = 0;

    private Grid<bool> _previousGrid;
    private Grid<bool> _grid = new Grid<bool>(1, 1, (int idx, int i, int i1, out bool cell) =>
    {
        cell = false;
        return true;
    });
    
    private Grid<bool> Grid
    {
        set
        {
            _previousGrid = _grid;
            _grid = value;
        }
        get => _grid;
    }

    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        RegenerateButtonGrid();
        WidthSlider.ValueChanged += OnGridSizeChanged;
        HeightSlider.ValueChanged += OnGridSizeChanged;
        ClearButton.Pressed += () => UpdateWholeGrid(false);
        FillButton.Pressed += () => UpdateWholeGrid(true);
    }
    
    public override void _Notification(int what)
    {
        //GD.Print($"GridShapeEditorPanel received notif {what}");
        if (what == NotificationTransformChanged)
        {
            var processFrame = Engine.GetProcessFrames();
            if (processFrame - 300 > _lastTransformChangedNotif)
            {
                RegenerateButtonGrid();
                _lastTransformChangedNotif = processFrame;
            }
        }
    }

    private void UpdateWholeGrid(bool on)
    {
        var newGrid = new Grid<bool>(_grid.Width, _grid.Height, (int idx, int i, int i1, out bool cell) =>
            {
                cell = on;
                return true;
            });
        UpdateValues(newGrid, _grid.Width, _grid.Height);
        EmitSignal(SignalName.GridShapeEditted, PackGridShapeToInt32Array());
        RegenerateButtonGrid();
    }

    public void UpdateValues(Grid<bool> existingGrid, int newWidth = 3, int newHeight = 3)
    {
        WidthSlider.SetValueNoSignal(newWidth);   
        HeightSlider.SetValueNoSignal(newHeight);
        
        var newGrid = new Grid<bool>(newWidth, newHeight, (int idx, int x, int y, out bool cell) =>
        {
            if (existingGrid.TryGetCell(x, y, out bool value))
            {
                cell = value;
                return true;
            }
            cell = false;
            return true;
        });
        Grid = newGrid;
    }
    
    public void RegenerateButtonGrid()
    {
        
        foreach (var child in GridContainer.GetChildren())
        {
            if (child.IsQueuedForDeletion()) continue;
            child.QueueFree();
        }

        GridContainer.Columns = (int)WidthSlider.Value;
        int cellCount = (int)HeightSlider.Value * (int)WidthSlider.Value;
        //GD.Print($"GridScroll size at button regen {GridScroll.Size}");
        var smallestAspect = Math.Min(GridScroll.Size.X, GridScroll.Size.Y);
        var largestSpan = Math.Max(WidthSlider.Value, HeightSlider.Value);
        var containerSize = (smallestAspect - (largestSpan * 4)) / largestSpan;
        Vector2 buttonSize = new Vector2(Math.Max((float)containerSize, 16.0f), Math.Max((float)containerSize, 16.0f));
        
        for (int i = 0; i < cellCount; i++)
        {
            var button = new Button();
            button.ToggleMode = true;
            button.SetCustomMinimumSize(buttonSize);
            button.ActionMode = BaseButton.ActionModeEnum.Press;
            button.SetTheme(GridButtonTheme);
            int idx = i;
            button.Toggled += (bool val) => { OnGridCellToggled(val, idx); };
            var gridPos = new Vector2I(idx % (int)WidthSlider.Value, idx / (int)WidthSlider.Value);
            if (Grid.TryGetCell(gridPos, out bool val2))
            {
                button.SetPressedNoSignal(val2);
            }
            GridContainer.AddChild(button);
        }
    }

    private void OnGridCellToggled(bool toggledOn, int idx)
    {
        var gridPos = new Vector2I(idx % (int)WidthSlider.Value, idx / (int)WidthSlider.Value);
        Grid.SetCell(gridPos, toggledOn);
        EmitSignal(SignalName.GridShapeEditted, PackGridShapeToInt32Array());
    }

    private void OnGridSizeChanged(double value)
    {
        UpdateValues(Grid, (int)WidthSlider.Value, (int)HeightSlider.Value);
        EmitSignal(SignalName.GridShapeEditted, PackGridShapeToInt32Array());
        RegenerateButtonGrid();
    }
    
    /*
     Creates an array 2 longer than the number of grid cells, adds the grids width and height to the start
     then converts the grids values into an int that represents true/false.
     */
    private int[] PackGridShapeToInt32Array()
    {
        int gridSize = Grid.Width * Grid.Height;
        int[] arr = new int[gridSize + 2];
        arr[0] = Grid.Width;
        arr[1] = Grid.Height;
        bool[] gridArr = Grid.GetCellAllCellsUnsafe();
        for (int i = 0; i < gridArr.Length; i++)
        {
            arr[i+2] = gridArr[i] ? 1 : 0;
        }
        //GD.Print($"Packed current Grid into arr {arr.Stringify()}");
        return arr;
    }

    public void UpdateFromInt32Array(int[] arr)
    {
        //GD.Print($"Updating grid from int array = {arr.Stringify()}");
        var newGrid = UnpackGridShapeFromInt32Array(arr);
        UpdateValues(newGrid, newGrid.Width, newGrid.Height);
        RegenerateButtonGrid();
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
