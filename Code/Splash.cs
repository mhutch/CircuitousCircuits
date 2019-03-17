using Godot;
using System;

public class Splash : Node2D
{
    Sprite splashImg;

    public override void _Ready()
    {
        splashImg = new Sprite();
        splashImg.Texture = Resources.Textures.Splash;
        AddChild(splashImg);

        GetViewport().Connect("size_changed", this, nameof(Rescale));
        Rescale();
    }

    public override void _Process(float delta)
    {
        if (Input.IsActionJustPressed("fullscreen"))
        {
            OS.WindowFullscreen = !OS.WindowFullscreen;
        }
        else if (Input.IsActionJustPressed("start"))
        {
            GetTree().ChangeScene("res://Puzzle.tscn");//.Quit();
        }
        else if (Input.IsActionJustPressed("back"))
        {
            GetTree().Quit();
        }
    }

    public void Rescale()
    {
        var rect = GetViewportRect();
        var scale = rect.Size.y/splashImg.Texture.GetHeight();
        splashImg.Scale = new Vector2(scale, scale);
        splashImg.Position = new Vector2(rect.Size.x / 2f, rect.Size.y / 2f);
    }
}
