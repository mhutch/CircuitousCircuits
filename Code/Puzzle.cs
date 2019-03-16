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
        Map.Initialize(c => new CellInfo(c, AddCellFill (c)));

        snapTween = new Tween();
        AddChild(snapTween);

        SpawnTile();
    }

    void CreateCursor ()
    {
        hexCursor = new Sprite();
        hexCursor.Texture = (Texture)GD.Load("res://Tiles/TileCursor.png");
        var textureHeight = hexCursor.Texture.GetHeight();
        float scale = 2f / textureHeight;
        hexCursor.Scale = new Vector2(scale, scale);
        hexCursor.ZIndex = 1;
        hexCursor.Visible = false;
        AddChild(hexCursor);
    }

    Sprite AddCellFill (HexCoord c)
    {
        var s = new Sprite();
        s.Texture = (Texture)GD.Load("res://Tiles/TileBoard.png");
        var textureHeight = s.Texture.GetHeight();
        float scale = 2f / textureHeight;
        s.Scale = new Vector2(scale, scale);
        s.ZIndex = -1;
        s.Position = c.Position();
        AddChild(s);
        return s;
    }

    public void ShowCursor(HexCoord coord)
    {
        hexCursor.Position = coord.Position();
        hexCursor.Visible = true;
    }

    public void HideCursor()
    {
        hexCursor.Visible = false;
    }

    void SnapTileToCell (PuzzleTileHex tile, HexCoord coord)
    {
        snapTween.InterpolateProperty(tile, "position", null, coord.Position(), 0.1f, Tween.TransitionType.Cubic, Tween.EaseType.Out, 0);
        snapTween.Start();
    }

    public void SpawnTile ()
    {
        var x = new PuzzleTileHex();
        x.Position = spawnCoord.Position();
        AddChild(x);
    }

    public void DropTile (PuzzleTileHex tile, HexCoord coord)
    {
        Map.SetCell(new CellInfo(coord, tile));
        SnapTileToCell(tile, coord);
        GD.Print($"Snapping drop to {coord}");
        SpawnTile();
    }

    public void ResetTile(PuzzleTileHex tile) => SnapTileToCell(tile, spawnCoord);
}
