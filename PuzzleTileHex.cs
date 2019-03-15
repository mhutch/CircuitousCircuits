using Godot;
using Settworks.Hexagons;

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

    public void MakeFixed ()
    {
        RemoveChild(dragArea);
        dragArea.QueueFree();
        dragArea = null;
    }

    public bool IsDraggable => dragArea != null;

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
        var coord = HexCoord.AtPosition(Position);

        var puzzle = GetPuzzle();
        var mapCell = puzzle.GetMapCell(coord);
        if (!IsValidDrop(mapCell))
        {
            puzzle.HideCursor();
            return;
        }

        puzzle.ShowCursor(coord);
    }

    bool IsValidDrop(CellInfo? mapCell) => mapCell != null && !(mapCell.Value.Tile is PuzzleTileHex);

    Puzzle GetPuzzle() => (Puzzle) GetParent();

    void OnDrop()
    {
        var puzzle = GetPuzzle();

        ZIndex = oldZIndex;
        puzzle.HideCursor();

        var coord = HexCoord.AtPosition(Position);
        var mapCell = puzzle.GetMapCell(coord);

        if (IsValidDrop(mapCell))
        {
            MakeFixed();
            puzzle.DropTile(this, coord);
        }
        else
        {
            puzzle.ResetTile(this);
        }
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