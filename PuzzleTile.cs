using System;
using Godot;

public class PuzzleTile : Sprite
{
    bool isUnderMouse;
    bool isDragging;
    Vector2 mouseOffset;

    public override void _Process(float delta)
    {
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

    public void OnMouseEntered()
    {
        isUnderMouse = true;
    }

    public void OnMouseExited()
    {
        isUnderMouse = false;
    }
}