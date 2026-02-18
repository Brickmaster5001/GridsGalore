#if TOOLS
using Godot;

[Tool]
public partial class InventoryConceptPlugin : EditorPlugin
{
	private InventoryConceptInspectorPlugin _plugin;

	public override void _EnterTree()
	{
		_plugin = new InventoryConceptInspectorPlugin();
		AddInspectorPlugin(_plugin);
	}

	public override void _ExitTree()
	{
		RemoveInspectorPlugin(_plugin);
	}
}
#endif
