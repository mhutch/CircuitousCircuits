using System;
using Godot;
using Settworks.Hexagons;

public class PuzzleTileHex : Node2D
{
    bool isUnderMouse;
    bool isDragging;
    Vector2 mouseOffset;

    Sprite background;

    Area2D dragArea;
    Tween snapTween;

    public override void _Ready()
    {
        base._Ready();

        background = AddSprite(Resources.Textures.Tile);

        ZIndex = (int)ZLayers.DroppedTile;

        MakeDraggable();

        snapTween = new Tween();
        AddChild(snapTween);

        if (LineDescriptions == null)
        {
            LineDescriptions = new[] { 0, 1, 2, 3, 4, 5 };
        }

        AddLines();
    }

    public int[] LineDescriptions = new int[6];
    public int[] Paths = new int[6];

    Sprite AddSprite(Texture texture, int zindex = 0)
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
            var delta = Constrain(i - LineDescriptions[i]);
            if (delta > 3)
            {
                continue;
            }

            int rot = i;
            Texture texture;
            switch (delta)
            {
                case 0:
                    texture = Resources.Textures.Stop;
                    break;
                case 1:
                    texture = Resources.Textures.CurveSmall;
                    break;
                case 2:
                    texture = Resources.Textures.CurveLarge;
                    break;
                case 3:
                    texture = Resources.Textures.Line;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            var s = AddSprite(texture, i);
            s.Rotation = rot * Mathf.Pi / 3f;
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

    public void MakeFixed()
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
            if (!isDragging)
            {
                return;
            }

            mouseOffset = Position - GetViewport().GetMousePosition() / GlobalScale;
            oldZIndex = ZIndex;
            ZIndex = (int)ZLayers.DragTile;

            OnStartDrag();
        }

        if (!Input.IsActionPressed("left_click"))
        {
            isDragging = false;
            OnDrop();
            return;
        }

        Position = GetViewport().GetMousePosition() / GlobalScale + mouseOffset;
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

    Puzzle GetPuzzle() => (Puzzle)GetParent();

    void OnStartDrag()
    {
        GetPuzzle().SoundPlayerPickup.Play(0);
    }

    void OnDrop()
    {
        var puzzle = GetPuzzle();

        ZIndex = oldZIndex;
        puzzle.HideCursor();

        var coord = HexCoord.AtPosition(Position);
        var mapCell = puzzle.Map.TryGetCell(coord);

        if (!IsValidDrop(mapCell))
        {
            GetPuzzle().SoundPlayerWhoosh.Play(0);
            puzzle.ResetTile(this);
            return;
        }

        GetPuzzle().SoundPlayerDrop.Play(0);

        MakeFixed();

        puzzle.Map.SetCell(new CellInfo(coord, this));
        puzzle.SnapTileToCell(this, coord);
        puzzle.SpawnTile();

        PrintPath();

        //propagate neighbor paths
        for (int side = 0; side < directions.Length; side++)
        {
            var neighborCell = puzzle.Map.TryGetCell(coord + directions[side]);
            if (neighborCell != null)
            {
                if (neighborCell.Value.Tile is PuzzleTileHex neighbor)
                {
                    GD.Print($"{neighbor.Name} is on side {side} of {Name}");
                    //get the path id of the neighboring tile
                    var pathID = neighbor.Paths[(side + 3) % 6];
                    GD.Print($"Incoming path ID: {pathID}");
                    PropagateIncomingPath(side, pathID);
                }
            }
        }

        //ensure all sides have path IDs
        for (int side = 0; side < directions.Length; side++)
        {
            if (Paths[side] == 0)
            {
                Paths[side] = puzzle.GetNextPathId();
            }
        }
    }

    void PropagateIncomingPath (int side, int pathID)
    {
        //if this tile side does not have a path ID, propagate it
        if (Paths[side] == 0)
        {
            Paths[side] = pathID;
            PathIncrement(pathID);
        }
        //if it does, join the paths
        else
        {
            pathID = PathJoin(Paths[side], pathID);
        }

        //check the internal connection in this tile
        var connectedSide = LineDescriptions[side];
        GD.Print($"Side {side} is internally connected to side {connectedSide}");

        //if it's a dead end, kill the path
        if (connectedSide == side)
        {
            PathDead(pathID);
            return;
        }

        var internalConnectedPath = Paths[connectedSide];

        // if the internal connection has no path ID, propagate this one,
        // else join them
        if (internalConnectedPath == 0)
        {
            Paths[connectedSide] = pathID;
            PathIncrement(pathID);
        }
        else
        {
            PathJoin(Paths[connectedSide], pathID);
        }

    }

    void PathDead(int pathID)
    {
        GD.Print($"Path {pathID} is dead");
    }

    int PathJoin(int oldPath, int newPath)
    {
        if (oldPath == newPath)
        {
            GD.Print("PATH 1 IS A CIRCUIT");
        }
        foreach (var tile in GetPuzzle().Map.GetAllTiles<PuzzleTileHex>())
        {
            for (int i = 0; i < 6; i++)
            {
                if (tile.Paths[i] == oldPath)
                {
                    tile.Paths[i] = newPath;
                }
            }
        }
        GD.Print($"Path {oldPath} was consumed by path {newPath}");
        return newPath;
    }

    void PathIncrement (int pathID)
    {
        GD.Print($"Path {pathID} grew longer");
    }

    static HexCoord[] directions = {
        new HexCoord (-1,  1),
        new HexCoord (-1,  0),
        new HexCoord ( 0, -1),
        new HexCoord ( 1, -1),
        new HexCoord ( 1,  0),
        new HexCoord ( 0,  1)
    };

    public void OnMouseEntered()
    {
        isUnderMouse = true;
    }

    public void OnMouseExited()
    {
        isUnderMouse = false;
    }

    public void RotateRight()
    {
        GetPuzzle().SoundPlayerRotate.Play(0);

        rotations++;
        RotateDefinitionRight(LineDescriptions);
        AnimateRotation();
    }

    public void RotateLeft()
    {
        GetPuzzle().SoundPlayerRotate.Play(0);

        rotations--;
        int tmp = LineDescriptions[0];
        for (int i = 0; i < LineDescriptions.Length - 1; i++)
        {
            LineDescriptions[i] = Constrain(LineDescriptions[i + 1] - 1);
        }
        LineDescriptions[5] = Constrain(tmp - 1);
        AnimateRotation();
    }

    int rotations;

    void AnimateRotation()
    {
        var newRotation = rotations * Mathf.Pi / 3f;
        snapTween.InterpolateProperty(this, "rotation", null, newRotation, 0.2f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
        snapTween.Start();
    }

    static int Constrain(int i) => (i + 6) % 6;

    static readonly int[][] tilesDefinitions = {
        new[] { 0, 1, 2, 3, 4, 5 }, // all ends
        new[] { 1, 0, 2, 3, 4, 5 }, // short and 4 ends
        new[] { 1, 0, 3, 2, 4, 5 }, // 2 short, adjacent
        new[] { 1, 0, 2, 4, 3, 5 }, // 2 short, ends between them
        new[] { 1, 0, 3, 2, 5, 4 }, // 3 short

        new[] { 2, 1, 0, 3, 4, 5 }, // 1 long
        new[] { 2, 1, 0, 5, 4, 3 }, // 2 long opposite, 2 ends
        new[] { 2, 3, 0, 1, 4, 5 }, // 2 long adjacent, 2 ends
        new[] { 2, 3, 0, 1, 5, 4 }, // 2 long adjacent, 1 short
        new[] { 2, 4, 0, 5, 1, 3 }, // 2 long opposite, 1 straight

        new[] { 2, 1, 0, 4, 3, 5 }, // long, short, end
        new[] { 2, 1, 0, 3, 5, 4 }, // long, end, short
        new[] { 2, 4, 0, 3, 1, 5 }, // long, end, straight, end
        new[] { 1, 0, 5, 4, 3, 2 }, // short, straight, short

        new[] { 3, 4, 5, 0, 1, 2 }, // 3 straight
        new[] { 3, 4, 2, 0, 1, 5 }, // 2 straight
        new[] { 3, 1, 2, 0, 4, 5 }, // 1 straight
    };

    static Random random = new Random();

    public static PuzzleTileHex GetRandomTile()
    {
        var idx = random.Next(0, tilesDefinitions.Length - 1);
        var def = (int[])tilesDefinitions[idx].Clone();
        var rotations = random.Next(0, 5);
        for (int i = 0; i < rotations; i++)
        {
            RotateDefinitionRight(def);
        }
        return new PuzzleTileHex { LineDescriptions = def };
    }

    static void RotateDefinitionRight(int[] definition)
    {
        int tmp = definition[definition.Length - 1];
        for (int i = definition.Length - 1; i > 0; i--)
        {
            definition[i] = Constrain(definition[i - 1] + 1);
        }
        definition[0] = Constrain(tmp + 1);
    }

    void PrintPath ()
    {
        GD.Print($"{Name} has connections {LineDescriptions[0]}{LineDescriptions[1]}{LineDescriptions[2]}{LineDescriptions[3]}{LineDescriptions[4]}{LineDescriptions[5]}");
    }
}
