using Godot;

public class PuzzleTileHex : Node2D
{
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
        var textureHeight = sprite.Texture.GetHeight();
        float scale = 2f / textureHeight;
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
            new Vector2 (0, -1f),
            new Vector2 (1f, -0.5f),
            new Vector2 (1f, 0.5f),
            new Vector2 (0, 1f),
            new Vector2 (-1f, 0.5f),
            new Vector2 (-1f, -5f),
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

    int oldZIndex;

    void HandleDrag()
    {
        if (!isDragging)
        {
            if (!isUnderMouse)
            {
                return;
            }

            isDragging = Input.IsActionPressed("left_click");
            mouseOffset = Position - GetViewport().GetMousePosition()/GlobalScale;
            ZIndex = 1000;
        }

        if (!Input.IsActionPressed("left_click"))
        {
            isDragging = false;
            OnDrop();
            return;
        }

        Position = GetViewport().GetMousePosition()/GlobalScale + mouseOffset;
        GetPuzzle().ShowCursor(Position);
    }

    Puzzle GetPuzzle() => (Puzzle) GetParent();

    void OnDrop()
    {
        ZIndex = oldZIndex;
        var puzzle = GetPuzzle();
        puzzle.SnapTileToCell(this);
        puzzle.HideCursor();
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