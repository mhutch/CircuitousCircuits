using Godot;
using Settworks.Hexagons;
using System;

public class Puzzle : Node2D
{
    public const float PuzzleScale = 50;

    Sprite hexCursor;

    public override void _Ready()
    {
        this.Scale = new Vector2 (PuzzleScale, PuzzleScale);
        this.Position = new Vector2(0, PuzzleScale * 1.5f);

        base._Ready();

        hexCursor = new Sprite();
        hexCursor.Texture = (Texture)GD.Load("res://TileCursor.png");
        var textureHeight = hexCursor.Texture.GetHeight();
        float scale = 2f / textureHeight;
        hexCursor.Scale = new Vector2(scale, scale);
        hexCursor.ZIndex = 1;
        hexCursor.Visible = false;
        AddChild(hexCursor);

        var x = new PuzzleTileHex();
        x.Position = new Vector2(4, 4);
        AddChild(x);

        InitializeMap(4);
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
                arr[j] = new CellInfo { Q = q, R = q, Tile = tile };
            }
        }
    }

    CellInfo? GetMapCell (HexCoord c)
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

    public void ShowCursor(Vector2 position)
    {
        var coord = HexCoord.AtPosition(position);
        var mapCell = GetMapCell(coord);
        if (mapCell == null) // || mapCell.Value.Tile != null)
        {
            hexCursor.Visible = false;
            return;
        }
        hexCursor.Position = coord.Position();
        hexCursor.Visible = true;
    }

    public void HideCursor()
    {
        hexCursor.Visible = false;
    }

    public void SnapTileToCell (PuzzleTileHex tile)
    {
        var coord = HexCoord.AtPosition(tile.Position);
        tile.Position = coord.Position();
    }
}

public struct CellInfo
{
    public int Q { get; set; }
    public int R { get; set; }
    public Node2D Tile { get; set; }
}

