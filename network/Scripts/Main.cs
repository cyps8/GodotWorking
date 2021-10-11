using Godot;
using System;

public class Main : Node2D
{
    public override void _Ready()
    {
        
    }

    private void ButtonHostPressed()
    {
        GetTree().ChangeScene("res://Scenes/Server.tscn");
    }

    private void ButtonClientPressed()
    {
        GetTree().ChangeScene("res://Scenes/Client.tscn");
    }
}