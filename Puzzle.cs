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

        instance = (PuzzleTile)scene.Instance();
        instance.Position = new Vector2(480, 480);
        AddChild(instance);
    }
}
