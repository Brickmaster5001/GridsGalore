using Godot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Godot.Collections;
using Vector2 = Godot.Vector2;
using Vector3 = Godot.Vector3;

public partial class GridRendering : Node3D
{
    internal partial class WorldControl : Node3D
    {
        [Export] public MeshInstance3D MeshInstance { get; set; }
        [Export] public SubViewport HudViewport { get; set; }
        [Export] public Area3D HudArea { get; set; }
    }
    
    [Export] public MeshInstance3D MeshInstance { get; set; }
    [Export] public SubViewport HudViewport { get; set; }
    [Export] public Area3D HudArea { get; set; }
    
    //[Export] public WorldControl[] = [];
    //private Dictionary<ulong, WorldControl> _asdas;
    
    public override void _Ready()
    {
        HudArea.MouseEntered += OnMouseEntered;
        HudArea.MouseExited += OnMouseExited;
        HudArea.InputEvent += OnInputEvent;
    }

    private bool _isMouseInside;
    private Vector2 _lastScreenEventPos;
    private long _lastEventTime = -1;
    
    private void OnMouseEntered()
    {
        _isMouseInside = true;
        //GD.Print("Mouse entered");
        HudViewport.NotifyMouseEntered();
    }

    private void OnMouseExited()
    {
        HudViewport.NotifyMouseExited();
        //GD.Print("Mouse exited");
        _isMouseInside = false;
    }
    
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton || @event is InputEventMouseMotion || @event is InputEventScreenDrag ||
            @event is InputEventScreenTouch) return;
        //GD.Print("Unhandled Input");
        HudViewport.PushInput(@event);
    }

    private void OnInputEvent(Node node, InputEvent inputEvent, Vector3 eventPos, Vector3 eventNormal, long shapeIdx)
    {
        //GD.Print(eventPos);
        Camera3D camera = (Camera3D)node;
        Vector2 meshSize = ((PlaneMesh)MeshInstance.Mesh).Size;
        Vector3 worldEventPos = eventPos;

        long now = (long)(Time.GetTicksMsec() / 1000);

        worldEventPos = MeshInstance.GlobalTransform.AffineInverse() * worldEventPos;

        Vector2 screenEventPos = new Vector2();

        if (_isMouseInside)
        {
            screenEventPos = new Vector2(worldEventPos.X, worldEventPos.Z);
            screenEventPos.X = screenEventPos.X / meshSize.X;
            screenEventPos.Y = screenEventPos.Y / meshSize.Y;
            screenEventPos.X += 0.5f;
            screenEventPos.Y += 0.5f;
            screenEventPos.X *= HudViewport.Size.X;
            screenEventPos.Y *= HudViewport.Size.Y;
            //GD.Print(screenEventPos);
        }
        else if (!_lastScreenEventPos.IsZeroApprox())
        {
            screenEventPos = _lastScreenEventPos;
        }

        if (inputEvent is InputEventMouse)
        {
            ((InputEventMouse)inputEvent).Position = screenEventPos;
            ((InputEventMouse)inputEvent).GlobalPosition = screenEventPos;
        }

        if (inputEvent is InputEventMouseMotion)
        {
            if (_lastScreenEventPos.IsZeroApprox())
            {
                ((InputEventMouseMotion)inputEvent).Relative = Vector2.Zero;
            }
            else
            {
                ((InputEventMouseMotion)inputEvent).Relative = screenEventPos - _lastScreenEventPos;
                ((InputEventMouseMotion)inputEvent).Velocity = ((InputEventMouseMotion)inputEvent).Relative / (now - _lastEventTime);
            }
        }
        
        _lastScreenEventPos = screenEventPos;
        _lastEventTime = now;
        HudViewport.PushInput(inputEvent);
    }



    private readonly List<Vector3> _points = new();

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true } mouseEvent)
        {
            GD.Print("Mouse Button Pressed");
            if (mouseEvent.ButtonIndex == MouseButton.Left)
            { // Add new point, create line if we have a pair
                GD.Print("Mouse Button Pressed Left");
                if (GetMousePosition(out var mousePos))
                {
                    _points.Add(mousePos);
                    if (_points.Count > 1)
                    {
                        GD.Print($"Drawing line | {_points[0]} -> {_points[1]} ...");
                        DrawLine(_points[0], _points[1], Colors.Crimson);
                        _points.Clear();
                    }
                }

                if (GetHudMousePosition(out var mousePosHud))
                {
                    
                }
            }

            if (mouseEvent.ButtonIndex == MouseButton.Right)
            { // Clear stored points
                GD.Print($"Cleared {_points.Count} points ...");
                _points.Clear();
            }
        }
    }

    private bool GetMousePosition([MaybeNullWhen(false)]out Vector3 position)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        Vector2 mousePos = GetViewport().GetMousePosition();
        Camera3D camera = GetTree().Root.GetCamera3D();
        
        GD.Print($"Mouse Position: {mousePos}");

        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePos) * 1000;
        
        GD.Print($"Ray Origin: {rayOrigin}");

        PhysicsRayQueryParameters3D rayParams = new();
        rayParams.From = rayOrigin;
        rayParams.To = rayEnd;
        rayParams.CollisionMask = 1;
        rayParams.Exclude = [];
        
        GD.Print($"Ray Parameters: {rayParams}");

        Dictionary rayResult = spaceState.IntersectRay(rayParams);
        if (rayResult.TryGetValue("position", out var value))
        {
            position = (Vector3)value;
            return true;
        }

        position = Vector3.Zero;
        return false;
    }

    private bool GetHudMousePosition([MaybeNullWhen(false)] out Vector3 position)
    {
        PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
        Vector2 mousePos = GetViewport().GetMousePosition();
        Camera3D camera = GetTree().Root.GetCamera3D();
        
        //GD.Print($"Mouse Position: {mousePos}");

        Vector3 rayOrigin = camera.ProjectRayOrigin(mousePos);
        Vector3 rayEnd = rayOrigin + camera.ProjectRayNormal(mousePos) * 1000;
        
        //GD.Print($"Ray Origin: {rayOrigin}");

        PhysicsRayQueryParameters3D rayParams = new();
        rayParams.From = rayOrigin;
        rayParams.To = rayEnd;
        rayParams.CollisionMask = 2;
        rayParams.Exclude = [];
        rayParams.CollideWithAreas = true;
        rayParams.CollideWithBodies = false;
        
        //GD.Print($"Ray Parameters: {rayParams}");

        Dictionary rayResult = spaceState.IntersectRay(rayParams);
        if (rayResult.TryGetValue("position", out var globalValue))
        {
            Area3D hitArea = (Area3D)rayResult["collider"];
            GD.Print($"Hit area: {hitArea.GetInstanceId()} {hitArea.GlobalTransform.ToString()}");
            //position = (Vector3)globalValue;
            GD.Print($"Global hit: {(Vector3)globalValue} | To Local -> {hitArea.ToLocal((Vector3)globalValue)}");
            Vector3 localPos = hitArea.ToLocal((Vector3)globalValue);
            Vector2I viewportPixelPos = new Vector2I((int)(512f * (localPos.X / 16.0f)), (int)(512f * (localPos.Z / 16.0f))); 
            GD.Print($"Calculated viewport pixel pos as {viewportPixelPos}");
            position = (Vector3)globalValue;
            return true;
        }

        position = Vector3.Zero;
        return false;
    }

    public MeshInstance3D DrawLine(Vector3 start, Vector3 end, Color color)
    {
        GD.Print(start);
        GD.Print(end);
        MeshInstance3D instance = new MeshInstance3D();
        ImmediateMesh mesh = new ImmediateMesh();
        OrmMaterial3D material = new OrmMaterial3D();
        instance.Mesh = mesh;
        instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        
        mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
        mesh.SurfaceAddVertex(start);
        mesh.SurfaceAddVertex(end);
        mesh.SurfaceEnd();

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        material.AlbedoColor = color;
        
        GetTree().GetRoot().AddChild(instance);
        return instance;
    }
}
