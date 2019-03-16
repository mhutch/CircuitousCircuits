using System;
using Godot;
using Settworks.Hexagons;

public class Puzzle : Node2D
{
    public const float PuzzleScale = 50;

    Sprite hexCursor;
    Tween snapTween;

    readonly HexCoord spawnCoord = new HexCoord(8, 0);

    public HexMap Map { get; private set; }

    public override void _Ready()
    {
        Scale = new Vector2 (PuzzleScale, PuzzleScale);
        Position = new Vector2(0, PuzzleScale * 1.5f);

        base._Ready();

        CreateCursor();

        Map = new HexMap(4);
        Map.Initialize(c => new CellInfo(c, AddBoardCell (c)));

        snapTween = new Tween();
        AddChild(snapTween);

        SpawnTile();

        InitializeSound();
        LoadMusic();
        StartMusic();
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
        var texture = Resources.Textures.Board;
        var textureHeight = texture.GetHeight();
        float scale = 2f / textureHeight;

        var s = new Sprite
        {
            Texture = texture,
            Scale = new Vector2(scale, scale),
            ZIndex = (int)ZLayers.Background,
            Position = c.Position()
        };
        AddChild(s);
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
        var x = PuzzleTileHex.GetRandomTile();
        x.Name = $"tile_{tileID++}";
        x.Position = spawnCoord.Position();
        AddChild(x);
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
}

enum ZLayers
{
    Background = 0,
    Cursor = 100,
    DroppedTile = 200,
    DragTile = 300
}
