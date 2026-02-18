using Godot;

namespace InventoryConcept.cs;

[GlobalClass]
public partial class Item : Resource
{
    [Export] public Texture2D Texture { get; set; } = new PlaceholderTexture2D();
    [Export] public string DisplayName { get; set; } = "Item Name";
    [Export] public string Identifier { get; set; } = "identifier";
    [Export] public GridShape GridShape { get; set; } = new GridShape();
    [Export] public Vector2I GridCellSize { get; set; } =  new Vector2I(48, 48);
    
}