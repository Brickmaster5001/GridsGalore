using Godot;
using Godot.Collections;

namespace InventoryConcept.cs;

public partial class InventoryFrame : Control
{
    [Export] public Array<Inventory> Inventories { get; set; }
    
}