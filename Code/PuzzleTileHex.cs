using Godot;
using Settworks.Hexagons;

public class PuzzleTileHex : Node2D
{
    bool isUnderMouse;
    bool isDragging;
    Vector2 mouseOffset;

    Sprite sprite;
    Area2D dragArea;
    Tween snapTween;

    public override void _Ready()
    {
        base._Ready();

        sprite = new Sprite();
        sprite.Texture = (Texture)GD.Load("res://Tiles/HexTile.png");
        var textureHeight = sprite.Texture.GetHeight();
        float scale = 2f / textureHeight;
        sprite.Scale = new Vector2(scale, scale);
        AddChild(sprite);

        ZIndex = (int)ZLayers.DroppedTile;

        MakeDraggable();

        snapTween = new Tween();
        AddChild(snapTween);

        AddLines();
    }

    void AddLines ()
    {
        var lines = new Sprite();
        lines.Texture = (Texture)GD.Load("res://Tiles/largeCurve.png");
        var textureHeight = lines.Texture.GetHeight();
        float scale = 2f / textureHeight;
        lines.Scale = new Vector2(scale, scale);
        lines.ZAsRelative = true;
        lines.ZIndex = 1;
        lines.Modulate = Colors.Aquamarine;
        AddChild(lines);
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

    // this is true for now
    public bool HasFocus => IsDraggable;

    public override void _Process(float delta)
    {
        if (IsDraggable)
        {
            HandleDrag();
        }

        if (HasFocus)
        {
            if (Input.IsActionJustPressed("rotate_right"))
            {
                RotateRight();
            }
            else if (Input.IsActionJustPressed("rotate_left"))
            {
                RotateLeft();
            }
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
            oldZIndex = ZIndex;
            ZIndex = (int)ZLayers.DragTile;
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
        var mapCell = puzzle.Map.TryGetCell(coord);
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
        var mapCell = puzzle.Map.TryGetCell(coord);

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

    public int[] Connections = new int[6];

    public void RotateRight()
    {
        rotations++;
        int tmp = Connections[5];
        for (int i = 1; i < Connections.Length; i++)
        {
            Connections[i] = Connections[i-1];
        }
        Connections[0] = tmp;
        AnimateRotation();
    }

    public void RotateLeft()
    {
        rotations--;
        int tmp = Connections[0];
        for (int i = 0; i < Connections.Length - 1; i++)
        {
            Connections[i] = Connections[i + 1];
        }
        Connections[5] = tmp;
        AnimateRotation();
    }

    int rotations;

    void AnimateRotation()
    {
        var newRotation = rotations * Mathf.Pi / 3f;
        snapTween.InterpolateProperty(this, "rotation", null, newRotation, 0.2f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
        snapTween.Start();
    }
}