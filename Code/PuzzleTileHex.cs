using System;
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

        sprite = AddSprite(
            (Texture)GD.Load("res://Tiles/HexTile.png")
        );

        ZIndex = (int)ZLayers.DroppedTile;

        MakeDraggable();

        snapTween = new Tween();
        AddChild(snapTween);

        Connections = new[] {
            //2, 1, 4, 3, 6, 5
            4, 3, 1, 3, 0, 5
        };

        AddLines();
    }

    Sprite AddSprite (Texture texture, int zindex = 0)
    {
        var textureHeight = texture.GetHeight();
        float scale = 2f / textureHeight;

        var s = new Sprite
        {
            Texture = texture,
            ZAsRelative = true,
            ZIndex = zindex,
            Scale = new Vector2(scale, scale)
        };

        AddChild(s);

        return s;
    }

    void AddLines()
    {
        for (int i = 0; i < 6; i++)
        {
            var delta = Constrain(i - Connections[i]);
            if (delta > 3)
            {
                continue;
            }

            Texture texture;
            switch (delta)
            {
                case 0:
                    texture = (Texture)GD.Load("res://Tiles/stop.png");
                    break;
                case 1:
                    texture = (Texture)GD.Load("res://Tiles/smallCurve.png");
                    break;
                case 2:
                    texture = (Texture)GD.Load("res://Tiles/largeCurve.png");
                    break;
                case 3:
                    texture = (Texture)GD.Load("res://Tiles/line.png");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var s = AddSprite(texture, i);
            s.Rotation = (i - 1) * Mathf.Pi / 3f;
        }
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
            Connections[i] = Constrain(Connections[i-1] + 1);
        }
        Connections[0] = Constrain(tmp + 1);
        AnimateRotation();
    }

    public void RotateLeft()
    {
        rotations--;
        int tmp = Connections[0];
        for (int i = 0; i < Connections.Length - 1; i++)
        {
            Connections[i] = Constrain(Connections[i + 1] - 1);
        }
        Connections[5] = Constrain(tmp - 1);
        AnimateRotation();
    }

    int rotations;

    void AnimateRotation()
    {
        var newRotation = rotations * Mathf.Pi / 3f;
        snapTween.InterpolateProperty(this, "rotation", null, newRotation, 0.2f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
        snapTween.Start();
    }

    int Constrain(int i) => (i + 6) % 6;
}