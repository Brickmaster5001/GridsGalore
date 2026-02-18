#if TOOLS
using Godot;
using InventoryConcept.cs;

[Tool]
public partial class InventoryConceptInspectorPlugin : EditorInspectorPlugin
{
    public override bool _CanHandle(GodotObject @object)
    {
        //@object.IsClass("GridShape")
        return true;
    }

    public override void _ParseBegin(GodotObject @object)
    {
        if (@object is GridShape gridShape)
        {
            base._ParseBegin(@object);
            var label = new Label();
            label.Text = "GridShape Object Label";
            AddCustomControl(label);
        }
    }

    public override bool _ParseProperty(GodotObject @object, Variant.Type type,
        string name, PropertyHint hintType, string hintString,
        PropertyUsageFlags usageFlags, bool wide)
    {
        // Replace Dictionary only if owning object is the GridShape resource-extending class 
        if (@object is GridShape && type == Variant.Type.PackedInt32Array)
        {
            AddPropertyEditor(name, new GridShapeEditor(), false, "Grid Ship Editor");
            return true;
        }

        return false;
    }
}
#endif