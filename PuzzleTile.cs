using System;
using Godot;

public class PuzzleTile : Sprite
{
    bool isUnderMouse;
    bool isDragging;
    Vector2 mouseOffset;

    public override void _Process(float delta)
    {
        GD.Print("PRESSED " + Input.IsActionPressed("left_click"));

        if (isUnderMouse && !isDragging)
        {
            isDragging = Input.IsActionPressed("left_click");
            mouseOffset = Position - GetViewport().GetMousePosition();
        }

        if (isDragging)
        {
            if (!Input.IsActionPressed("left_click"))
            {
                isDragging = false;
            }
            else
            {
                Position = GetViewport().GetMousePosition() + mouseOffset;
            }
        }
    }

    public override void _Ready()
    {
        base._Ready();
    }

    public void OnMouseEntered()
    {
        GD.Print("ENTER");
        isUnderMouse = true;
    }

    public void OnMouseExited()
    {
        GD.Print("EXIT");
        isUnderMouse = false;
    }
}