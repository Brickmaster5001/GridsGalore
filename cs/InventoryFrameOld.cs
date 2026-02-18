using System.Linq;
using Godot;
using Godot.Collections;

namespace InventoryConcept.cs;

[GlobalClass]
public partial class InventoryFrameOld : Control
{
    private Array<Inventory> _inventories = [];
    
    public override void _Ready()
    {
        UpdateInventoryChildren();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationChildOrderChanged)
        {
            UpdateInventoryChildren();
        }
    }

    private void UpdateInventoryChildren()
    {
        GD.Print("Updating child inventories for inventory frame ...");
        _inventories.Clear();
        var nodes = FindChildren("", nameof(Inventory), true).OfType<Inventory>();
        _inventories = new Array<Inventory>(nodes);
        foreach (var node in _inventories)
        {
            GD.Print($"-> Inventory {node.Name} | {node.GetInstanceId()}");
        }
    }
}