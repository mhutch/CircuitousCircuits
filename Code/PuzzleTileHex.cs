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

    Puzzle puzzle;

    public override void _Ready()
    {
        puzzle = (Puzzle)GetParent().GetParent();

        background = AddSprite(Resources.Textures.Tile);

        ZIndex = (int)ZLayers.DroppedTile;

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
    readonly Sprite[] pathSprites = new Sprite[6];

    public Sprite GetPathSprite(int index) => pathSprites[(index - rotations + 6000000) % 6];

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

            pathSprites[i] = s;
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

        var mapCell = puzzle.Map.TryGetCell(coord);
        if (!IsValidDrop(mapCell))
        {
            puzzle.HideCursor();
            return;
        }

        puzzle.ShowCursor(coord);
    }

    bool IsValidDrop(CellInfo? mapCell) => mapCell != null && !(mapCell.Value.Tile is PuzzleTileHex);

    void OnStartDrag()
    {
        puzzle.SoundPlayerPickup.Play(0);
    }

    void OnDrop()
    {
        ZIndex = oldZIndex;
        puzzle.HideCursor();

        var coord = HexCoord.AtPosition(Position);
        var mapCell = puzzle.Map.TryGetCell(coord);

        if (!IsValidDrop(mapCell))
        {
            puzzle.SoundPlayerWhoosh.Play(0);
            puzzle.ResetTile(this);
            return;
        }

        puzzle.SoundPlayerDrop.Play(0);

        MakeFixed();

        puzzle.Map.SetCell(new CellInfo(coord, this));
        puzzle.SnapTileToCell(this, coord);
        puzzle.SpawnTile();

        CalculatePaths(puzzle, coord);

        if (puzzle.Map.TileCount == puzzle.Map.CellCount)
        {
            OnFail();
        }
    }

    public void CalculatePaths(Puzzle puzzle, HexCoord coord)
    {
        GD.Print($"Placing tile {Name} ({GetPathString()})");

        //propagate neighbor paths
        for (int side = 0; side < directions.Length; side++)
        {
            var neighborCell = puzzle.Map.TryGetCell(coord + directions[side]);
            if (neighborCell != null)
            {
                if (neighborCell.Value.Tile is PuzzleTileHex neighbor)
                {
                    GD.Print($"Side {side} neighbor is {neighbor.Name} ({neighbor.GetPathString()})");
                    //get the path id of the neighboring tile
                    var pathID = neighbor.Paths[(side + 3) % 6];
                    GD.Print($"Neighbor path ID is {pathID}");
                    PropagateIncomingPath(side, pathID);
                }
            }
        }

        //ensure all sides have path IDs
        for (int side = 0; side < directions.Length; side++)
        {
            if (Paths[side] == 0)
            {
                var id = puzzle.GetNextPathId();
                Paths[side] = id;
                Paths[LineDescriptions[side]] = id;
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

        //if it's a dead end, kill the path
        if (connectedSide == side)
        {
            PathDead(pathID);
            return;
        }

        GD.Print($"Side {side} is internally connected to side {connectedSide}");

        var internalConnectedPath = Paths[connectedSide];

        // if the internal connection has no path ID, propagate this one,
        // else join them
        if (internalConnectedPath == 0)
        {
            Paths[connectedSide] = pathID;
            PathIncrement(pathID);
        }
        //if it wasn't handled by previous join, join it
        else if (internalConnectedPath != pathID)
        {
            PathJoin(internalConnectedPath, pathID);
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
            GD.Print($"PATH {newPath} IS A CIRCUIT");
            OnSuccess(newPath);
            return newPath;
        }

        foreach (var tile in puzzle.Map.GetAllTiles<PuzzleTileHex>())
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

    void OnSuccess(int path)
    {
        var timer = AddChild2(new Timer { OneShot = true });
        timer.Connect("timeout", puzzle, nameof(puzzle.NextLevel));
        timer.Start(2f);

        puzzle.SoundPlayerComplete.Play(0);
        TintPath(path, Colors.Yellow, 1f);
    }

    void OnFail()
    {
        var timer = AddChild2(new Timer { OneShot = true });
        timer.Connect("timeout", puzzle, nameof(puzzle.ResetLevel));
        timer.Start(1f);

        puzzle.SoundPlayerFail.Play(0);

        foreach (var tile in puzzle.Map.GetAllTiles<PuzzleTileHex>())
        {
            snapTween.InterpolateProperty(
                tile.background, "modulate", null, Colors.DarkGray, 1f,
                Tween.TransitionType.Sine, Tween.EaseType.In);
            snapTween.Start();
        }
    }

    void PathIncrement (int pathID)
    {
        GD.Print($"Incrementing path {pathID}");
    }

    void TintPath(int pathID, Color color, float pulseTime = 0f)
    {
        foreach (var tile in puzzle.Map.GetAllTiles<PuzzleTileHex>())
        {
            for (int i = 0; i < 6; i++)
            {
                if (tile.Paths[i] == pathID)
                {
                    var sprite = tile.GetPathSprite(i);
                    if (sprite != null)
                    {
                        if (pulseTime == 0f)
                        {
                            sprite.Modulate = color;
                        }
                        else
                        {
                            var initial = sprite.Modulate;
                            snapTween.InterpolateProperty(
                                sprite, "modulate", null, color, pulseTime/2f,
                                Tween.TransitionType.Sine, Tween.EaseType.Out);
                            snapTween.InterpolateProperty(
                                sprite, "modulate", color, initial, pulseTime/2f,
                                Tween.TransitionType.Sine, Tween.EaseType.In, pulseTime / 2f);
                            snapTween.Start();
                        }
                    }
                }
            }
        }
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
        puzzle.SoundPlayerRotate.Play(0);

        rotations++;
        RotateDefinitionRight(LineDescriptions);
        AnimateRotation();
    }

    public void RotateLeft()
    {
        puzzle.SoundPlayerRotate.Play(0);

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

    public static PuzzleTileHex GetRandomTile(Puzzle puzzle)
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

    string GetPathString () => $"{LineDescriptions[0]}{LineDescriptions[1]}{LineDescriptions[2]}{LineDescriptions[3]}{LineDescriptions[4]}{LineDescriptions[5]}";

    //GODOT: this cleans up code a lot
    T AddChild2<T>(T child) where T : Node
    {
        AddChild(child);
        return child;
    }
}
