using Godot;
using System;

public class GameClient : Node2D
{
    public override void _Ready()
    {
        
    }

    private void ButtonLeave()
    {
        GetTree().ChangeScene("res://Scenes/Main.tscn");
    }
}
