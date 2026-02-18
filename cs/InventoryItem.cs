using Godot;

namespace InventoryConcept.cs;

public partial class InventoryItem : Node2D
{
    private Texture2D _texture;
    private ImageTexture _scaledTexture;
    private Vector2I _gridCellSize;
    private Vector2I _gridPixelSize;
    private Grid<bool> _gridShape;
    private Inventory _sourceInventory;
    private InventoryFrame _sourceFrame;
    
    private bool _isSelected = false;
    private bool _isHovered = false;
    
    public InventoryItem() : this(null, null, null) {}

    public InventoryItem(Item item, Inventory sourceInventory, InventoryFrame sourceFrame)
    {
        _texture = item.Texture;
        _gridCellSize = item.GridCellSize;
        _gridShape = item.GridShape.ToGrid();
        _gridPixelSize = new Vector2I(_gridShape.Width * _gridCellSize.X, _gridShape.Height * _gridCellSize.Y);
        var scaledImage = _texture.GetImage();
        scaledImage.Resize(_gridPixelSize.X, _gridPixelSize.Y, Image.Interpolation.Lanczos);
        _scaledTexture = ImageTexture.CreateFromImage(scaledImage);
        _sourceInventory = sourceInventory;
        _sourceFrame = sourceFrame;
    }

    public override void _Ready()
    {
        
    }

    public override void _Process(double delta)
    {
        if (!_isSelected) return;
        
    }
}