using Godot;
using System;

public class GameServer : Node2D
{
    Server server;
    public override void _Ready()
    {
        server = GetNode<Server>("ServerManager");
        server.ServerStart();
    }

    private void ButtonLeave()
    {
        server.ServerStop();
        GetTree().ChangeScene("res://Scenes/Main.tscn");
    }
}
