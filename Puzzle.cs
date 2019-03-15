using Godot;
using Settworks.Hexagons;
using System;

public class Puzzle : Node2D
{
    public const float PuzzleScale = 50;

    Sprite hexCursor;
    Tween snapTween;

    readonly HexCoord spawnCoord = new HexCoord(8, 0);

    public override void _Ready()
    {
        Scale = new Vector2 (PuzzleScale, PuzzleScale);
        Position = new Vector2(0, PuzzleScale * 1.5f);

        base._Ready();

        CreateCursor();
        InitializeMap(4);

        snapTween = new Tween();
        AddChild(snapTween);

        SpawnTile();
    }

    void CreateCursor ()
    {
        hexCursor = new Sprite();
        hexCursor.Texture = (Texture)GD.Load("res://TileCursor.png");
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
        s.Texture = (Texture)GD.Load("res://TileBoard.png");
        var textureHeight = s.Texture.GetHeight();
        float scale = 2f / textureHeight;
        s.Scale = new Vector2(scale, scale);
        s.ZIndex = -1;
        s.Position = c.Position();
        AddChild(s);
        return s;
    }

    CellInfo[][] map;

    void InitializeMap (int edgeSize)
    {
        int size = edgeSize * 2 - 1;
        map = new CellInfo[size][];

        for (int r = 0; r < size; r++)
        {
            var arr = new CellInfo[size - Math.Abs(edgeSize - 1 - r)];
            map[r] = arr;
            int offset = -Math.Max(0, edgeSize - 1 - r);
            for (int j = 0; j < arr.Length; j++)
            {
                var q = j - offset;
                var tile = AddCellFill(new HexCoord(q, r));
                arr[j] = new CellInfo (q, r, tile);
            }
        }
    }

    public CellInfo? GetMapCell (HexCoord c)
    {
        if (c.r >= 0 && c.r < map.Length)
        {
            var row = map[c.r];
            int edgeSize = (map.Length + 1) / 2;
            var index = c.q - Math.Max(0, edgeSize - 1 - c.r);
            if (index >= 0 && index < row.Length)
            {
                return row[index];
            }
        }
        return null;
    }

    void SetMapCell(HexCoord c, CellInfo info)
    {
        var row = map[c.r];
        int edgeSize = (map.Length + 1) / 2;
        var index = c.q - Math.Max(0, edgeSize - 1 - c.r);
        row[index] = info;
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
        SetMapCell(coord, new CellInfo(coord.q, coord.r, tile));
        SnapTileToCell(tile, coord);
        GD.Print($"Snapping drop to {coord}");
        SpawnTile();
    }

    public void ResetTile(PuzzleTileHex tile) => SnapTileToCell(tile, spawnCoord);
}

public struct CellInfo
{
    public CellInfo (int q, int r, Node2D tile)
    {
        Q = q;
        R = r;
        Tile = tile;
    }

    public int Q { get; }
    public int R { get;}
    public Node2D Tile { get; }
}

