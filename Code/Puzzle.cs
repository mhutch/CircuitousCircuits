using System;
using Godot;
using Settworks.Hexagons;

public class Puzzle : Node2D
{
    const int levelCount = 10;

    Sprite hexCursor;
    Tween snapTween;

    public HexMap Map => map;
    public int GetNextPathId() => nextPathId++;

    int nextPathId = 1;
    HexCoord spawnCoord;
    Node2D board;
    HexMap map;

    int currentLevel;

    public Color TileColor { get; private set; } = Colors.Red;
    public Color StaticTileColor { get; private set; } = Colors.DarkRed;
    public Color BoardColor { get; private set; } = Colors.Blue;
    public Color BackgroundColor { get; private set; } = Colors.DarkGray;
    public Color LineHighlightColor { get; private set; } = Colors.Yellow;

    public override void _Ready()
    {
        CreateCursor();

        snapTween = new Tween();
        AddChild(snapTween);

        InitializeSound();
        LoadMusic();
        StartMusic();

        LoadLevel(1);

        GetViewport().Connect("size_changed", this, nameof(Rescale));
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("fullscreen"))
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
        else if (Input.IsActionJustPressed("back"))
        {
            GetTree().ChangeScene("res://Splash.tscn");//.Quit();
        }
        else if (Input.IsActionJustPressed("reset"))
        {
            ResetLevel();
        }
    }

    void CreateCursor ()
    {
        var texture = Resources.Textures.Cursor;
        var textureHeight = texture.GetHeight();
        float scale = 2f / textureHeight;

        hexCursor = new Sprite
        {
            Texture = texture,
            Scale = new Vector2(scale, scale),
            ZIndex = (int)ZLayers.Cursor,
            Visible = false
        };
        AddChild(hexCursor);
    }

    Sprite AddBoardCell (HexCoord c)
    {
        var texture = Resources.Textures.Tile;
        var textureHeight = texture.GetHeight();
        float scale = 2f / textureHeight;

        var s = new Sprite
        {
            Texture = texture,
            Scale = new Vector2(scale, scale),
            ZIndex = (int)ZLayers.Background,
            Position = c.Position(),
            Modulate = BoardColor
        };
        board.AddChild(s);
        var overlay = new Sprite
        {
            Texture = Resources.Textures.BoardGradient,
            Scale = new Vector2(scale, scale),
            ZIndex = (int)ZLayers.Background+1,
            Position = c.Position()
        };
        board.AddChild(overlay);
        return s;
    }

    public void ShowCursor(HexCoord coord)
    {
        if (!hexCursor.Visible)
        {
            hexCursor.Position = coord.Position();
            hexCursor.Visible = true;

        }
        else
        {
            snapTween.InterpolateProperty(hexCursor, "position", null, coord.Position(), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
            snapTween.Start();
        }
    }

    public void HideCursor()
    {
        hexCursor.Visible = false;
    }

    public void SnapTileToCell (PuzzleTileHex tile, HexCoord coord)
    {
        snapTween.InterpolateProperty(tile, "position", null, coord.Position(), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
        snapTween.Start();
    }

    int tileID = 0;

    public void SpawnTile ()
    {
        var x = PuzzleTileHex.GetRandomTile(this);
        x.MakeDraggable();
        x.Name = $"tile_{tileID++}";
        x.Position = spawnCoord.Position();
        board.AddChild(x);
    }

    public void ResetTile(PuzzleTileHex tile) => SnapTileToCell(tile, spawnCoord);

    AudioStreamPlayer[] musicLayers;
    int activeMusicLayerCount = 0;

    void LoadMusic ()
    {
        AudioStreamOGGVorbis[] musicLayerResources = {
            Resources.Music.Melody,
            Resources.Music.Bass,
            Resources.Music.Swells,
            Resources.Music.Decoration,
            Resources.Music.Eighths
        };

        musicLayers = new AudioStreamPlayer[musicLayerResources.Length];
        for (int i = 0; i < musicLayers.Length; i++)
        {
            var player = new AudioStreamPlayer
            {
                Stream = musicLayerResources[i]
            };
            AddChild(player);
            musicLayers[i] = player;
        }
        musicLayers[0].Connect("finished", this, nameof(OnMusicFinished));
    }

    void StartMusic ()
    {
        activeMusicLayerCount = 1;
        musicLayers[0].Play(0);
    }

    void OnMusicFinished ()
    {
        activeMusicLayerCount++;
        for (int i = 0; i < musicLayers.Length; i++)
        {
            if (i < activeMusicLayerCount)
            {
                if (musicLayers[i].IsPlaying())
                {
                    //GODOT: if one of the layers is still playing when we (re)start them all,
                    //we get a loud click glitch. however, if we wait for all of them to finish
                    //we sometimes get a slight gap.
                    GD.Print($"Music layer {i} was out of sync");
                }
                musicLayers[i].Play(0);

            }
            else
            {
                musicLayers[i].Stop();
            }
        }
    }

    void InitializeSound()
    {
        AudioStreamPlayer CreatePlayer (AudioStreamOGGVorbis s)
        {
            //GODOT: would be nice if the ctor let us pass the stream in
            //GODOT: not obvious what to do for one-shot effects. should i create as needed and destroy when they're done? should I pool players?
            var p = new AudioStreamPlayer { Stream = s };
            p.Autoplay = false;
            s.Loop = false;
            //GODOT: it would be REALLY nice if AddChild returned the child
            AddChild(p);
            return p;
        }

        SoundPlayerRotate = CreatePlayer(Resources.Sfx.RotateTile);
        SoundPlayerDrop = CreatePlayer(Resources.Sfx.PlaceTile);
        SoundPlayerWhoosh = CreatePlayer(Resources.Sfx.WhooshBack);
        SoundPlayerPickup = CreatePlayer(Resources.Sfx.PickUpTile);
        SoundPlayerComplete = CreatePlayer(Resources.Sfx.CircuitComplete);
        SoundPlayerFail = CreatePlayer(Resources.Sfx.CircuitFail);
    }


    public AudioStreamPlayer SoundPlayerRotate { get; private set; }
    public AudioStreamPlayer SoundPlayerDrop { get; private set; }
    public AudioStreamPlayer SoundPlayerWhoosh { get; private set; }
    public AudioStreamPlayer SoundPlayerPickup { get; private set; }
    public AudioStreamPlayer SoundPlayerComplete { get; private set; }
    public AudioStreamPlayer SoundPlayerFail { get; private set; }

    void CreateLevel(int size)
    {
        //reset everything
        nextPathId = 1;
        map = null;
        if (board != null)
        {
            RemoveChild(board);
            board.QueueFree();
        }
        board = new Node2D();
        AddChild(board);
        HideCursor();

        map = new HexMap(size);
        map.Initialize(c => new CellInfo(c, AddBoardCell(c)));

        spawnCoord = new HexCoord(size * 2, 0);
        SpawnTile();
    }

    void LoadLevel (int number)
    {
        currentLevel = number;

        //GODOT: how on earth do I load a text file?
        //var resource = GD.Load($"res://Levels/{number}.txt");
        //this took 30mins to figure out. every operation is different than the
        //C# BCL version and the docs are not helpful
        var f = new File();
        f.Open($"res://Levels/{number}.txt", (int)File.ModeFlags.Read);
        var lines = new System.Collections.Generic.List<string>();
        while(!f.EofReached())
        {
            lines.Add(f.GetLine());
        }
        //var lines = System.IO.File.ReadAllLines(System.IO.Path.Combine("Levels", $"{number}.txt"));

        var boardDef = lines[0].Split('|');
        int boardSize = int.Parse(boardDef[0]);
        if (boardDef.Length == 6)
        {
            TileColor = new Color(boardDef[1]);

            if (string.IsNullOrEmpty(boardDef[2]))
            {
                StaticTileColor = TileColor.Darkened(0.2f);
            }
            else
            {
                StaticTileColor = new Color(boardDef[2]);
            }

            BoardColor = new Color(boardDef[3]);

            if (string.IsNullOrEmpty(boardDef[4]))
            {
                BackgroundColor = Colors.Black;
            }
            else
            {
                BackgroundColor = new Color(boardDef[4]);
            }

            LineHighlightColor = new Color(boardDef[5]);
        }
        else
        {
            TileColor = new Color("e017c2");
            StaticTileColor = TileColor.Darkened(0.2f);
            BoardColor = new Color("bdf0ec");
            BackgroundColor = Colors.Black;
            LineHighlightColor = Colors.Yellow;
        }

        VisualServer.SetDefaultClearColor(BackgroundColor);

        CreateLevel(boardSize);

        for (int i = 1; i < lines.Count; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            int commaIdx = line.IndexOf(',');
            int q = int.Parse(line.Substring(0,commaIdx));

            var pipeIdx = line.IndexOf('|');
            int r = int.Parse(line.Substring(commaIdx + 1, pipeIdx - commaIdx - 1));


            int[] desc = new int[6];
            for (int j = 0; j < 6; j++)
            {
                desc[j] = line[pipeIdx+ 1+j] - '0';
            }

            //offset due to the map we're using to figure out positions
            int offset = Math.Max(0, 4 - boardSize);
            var coord = new HexCoord(q, r - offset);
            var tile = new PuzzleTileHex { LineDescriptions = desc, Position = coord.Position() };
            tile.IsStatic = true;
            map.SetCell(new CellInfo(coord, tile));
            board.AddChild(tile);
            tile.CalculatePaths(this, coord);
        }

        var c = TileColor;
        c.a = 0.6f;
        hexCursor.Modulate = c;

        Rescale();
    }

    public void NextLevel()
    {
        currentLevel = (currentLevel % levelCount) + 1;
        LoadLevel(currentLevel);
    }

    public void ResetLevel () => LoadLevel(currentLevel);

    public void Rescale ()
    {
        var rect = GetViewportRect();

        //scale such that the puzzle fits with a half tile margin

        var radius = rect.Size.y /((map.Size * 0.75f + 0.25f) + 1f) / 2f;
        Scale = new Vector2(radius, radius);

        var center = new HexCoord(map.EdgeSize - 1, map.EdgeSize - 1);
        var centerPos = center.Position() * Scale;
        var centerScreen = new Vector2(rect.Size.x / 2f, rect.Size.y / 2f);

        Position = centerScreen - centerPos;
    }
}

enum ZLayers
{
    Background = 0,
    Cursor = 100,
    DroppedTile = 200,
    DragTile = 300
}
