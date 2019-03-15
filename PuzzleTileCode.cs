using Godot;

public class PuzzleTileHex : Node2D
{
    const float radius = 60;

    bool isUnderMouse;
    bool isDragging;
    Vector2 mouseOffset;

    Sprite sprite;
    Area2D dragArea;

    public override void _Ready()
    {
        base._Ready();

        sprite = new Sprite();
        sprite.Texture = (Texture)GD.Load("res://HexTile.png");
        var textureWidth = sprite.Texture.GetWidth();
        float scale = radius * 2f / textureWidth;
        sprite.Scale = new Vector2(scale, scale);
        AddChild(sprite);

        MakeDraggable();
    }

    public void MakeDraggable()
    {
        dragArea = new Area2D();
        AddChild(dragArea);

        dragArea.Connect("mouse_entered", this, nameof(OnMouseEntered));
        dragArea.Connect("mouse_exited", this, nameof(OnMouseExited));

        var shape = new ConvexPolygonShape2D();
        shape.SetPoints(new Vector2[]
        {
            new Vector2 (0, -radius),
            new Vector2 (radius, -radius/2),
            new Vector2 (radius, radius/2),
            new Vector2 (0, radius),
            new Vector2 (-radius, radius/2),
            new Vector2 (-radius, -radius/2),
        });

        var ownerId = dragArea.CreateShapeOwner(dragArea);
        dragArea.ShapeOwnerAddShape(ownerId, shape);
    }

    public override void _Process(float delta)
    {
        if (dragArea != null)
        {
            HandleDrag();
        }
    }

    void HandleDrag()
    {
        if (!isDragging)
        {
            if (!isUnderMouse)
            {
                return;
            }

            isDragging = Input.IsActionPressed("left_click");
            mouseOffset = Position - GetViewport().GetMousePosition();
        }

        if (!Input.IsActionPressed("left_click"))
        {
            isDragging = false;
            OnDrop();
            return;
        }

        Position = GetViewport().GetMousePosition() + mouseOffset;

        var puzzle = (Puzzle)GetParent();
        var cell = puzzle.GetCellFromPosition(Position);
    }

    void OnDrop ()
    {
    }

    public void OnMouseEntered()
    {
        isUnderMouse = true;
    }

    public void OnMouseExited()
    {
        isUnderMouse = false;
    }
}