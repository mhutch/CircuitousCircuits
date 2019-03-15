using Godot;
using Settworks.Hexagons;
using System;

public class Puzzle : Node2D
{
    public const float PuzzleScale = 60;

    Sprite hexCursor;

    public override void _Ready()
    {
        this.Scale = new Vector2 (PuzzleScale, PuzzleScale);

        base._Ready();

        hexCursor = new Sprite();
        hexCursor.Texture = (Texture)GD.Load("res://TileCursor.png");
        var textureHeight = hexCursor.Texture.GetHeight();
        float scale = 2f / textureHeight;
        hexCursor.Scale = new Vector2(scale, scale);
        hexCursor.Visible = false;
        hexCursor.ZIndex = 1;
        AddChild(hexCursor);

        var x = new PuzzleTileHex();
        x.Position = new Vector2(4, 4);
        AddChild(x);

        ShowPlaceholder(3, 3);
    }

    CellInfo[,] grid = new CellInfo[7, 7];

    public void ShowCursor(Vector2 position)
    {
        var coord = HexCoord.AtPosition(position);
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

    public void ShowPlaceholder (int q, int r)
    {
        hexCursor.Position = new HexCoord(q, r).Position ();
        hexCursor.Visible = true;
    }
}

public struct CellInfo
{
    public int Q { get; set; }
    public int R { get; set; }
    public PuzzleTileHex Tile { get; set; }
}

