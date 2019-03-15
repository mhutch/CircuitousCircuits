using Godot;
using System;

public class Puzzle : Node2D
{
    public override void _Ready()
    {
        var scene = GD.Load<PackedScene>("res://PuzzleTile.tscn");
        var instance = (PuzzleTile)scene.Instance();
        instance.Position = new Vector2(240, 480);
        AddChild(instance);

        var x = new PuzzleTileHex();
        x.Position = new Vector2(480, 480);
        AddChild(x);
    }

    CellInfo[,] grid = new CellInfo[5, 5];

    public CellInfo? GetCellFromPosition(Vector2 position)
    {
        return null;
    }

}

public struct CellInfo
{
    public int X { get; set; }
    public int Y { get; set; }
    public PuzzleTileHex Tile { get; set; }
}

