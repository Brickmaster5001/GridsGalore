#if TOOLS
using Godot;
using Godot.Collections;

public partial class GridShapeEditor : EditorProperty
{
    private PackedScene _packedGridShapeEditorPanel =
        ResourceLoader.Load<PackedScene>("res://addons/inventoryconceptextension/grid_shape_editor_panel.tscn");
    
    private GridShapeEditorPanel _propertyControl;

    private int _width = 0;
    private int _height = 0;
    private int[] _currentValue = [1, 1, 1];
    private bool _updating = false;

    public GridShapeEditor()
    {
        //_currentValue = (int[])GetEditedObject().Get(GetEditedProperty());
        _propertyControl = _packedGridShapeEditorPanel.Instantiate<GridShapeEditorPanel>();
        AddChild(_propertyControl);
        SetBottomEditor(_propertyControl);
        AddFocusable(_propertyControl);
        _propertyControl.GridShapeEditted += OnGridPanelUpdated;
        RefreshGridEditor();
    }

    private void OnGridPanelUpdated(int[] newValue)
    {
        // Ignore the signal if the property is currently being updated.
        if (_updating)
        {
            //GD.Print("Grid panel update returned because _updating");
            return;
        }

        // Generate a new random integer between 0 and 99.
        _currentValue = newValue;
        //RefreshGridEditor();
        EmitChanged(GetEditedProperty(), _currentValue);
    }

    public override void _UpdateProperty()
    {
        // Read the current value from the property.
        var newValue = (int[])GetEditedObject().Get(GetEditedProperty());
        if (newValue == _currentValue)
        {
            return;
        }

        // Update the control with the new value.
        _updating = true;
        _currentValue = newValue;
        if (_currentValue.Length > 3)
        {
            _width = newValue[0];
            _height = newValue[1];
        }
        
        RefreshGridEditor();
        _updating = false;
    }

    private void RefreshGridEditor()
    {
        _propertyControl.UpdateFromInt32Array(_currentValue);
    }
}
#endif