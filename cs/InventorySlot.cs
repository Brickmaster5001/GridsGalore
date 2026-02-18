using Godot;

namespace InventoryConcept.cs;

public struct InventorySlot
{
     public Vector2I GridPosition { get; init; }
     public bool IsFilled { get; set; }
     public bool IsHovered { get; set; }
 
     public InventorySlot(bool startFilled, int x, int y)
     {
         IsFilled = startFilled;
         GridPosition = new Vector2I(x, y);
     }
 
     public void Destroy()
     {
         IsFilled = false;
     }
 }