using Godot;
using System;

public class SceneManager : Node
{
    public override void _Ready()
    {

    }

    public void GotoScene(string path)
    {
        GetNode("/root/MasterScene").GetChild(0).QueueFree();

        var nextScene = GD.Load<PackedScene>(path).Instance();
        GetNode("/root/MasterScene").AddChild(nextScene);
    }

    public void StartServer()
    {
        GotoScene("res://Scenes/Server.tscn");
    }

    public void StartClient()
    {
        GotoScene("res://Scenes/Client.tscn");
    }

    public void GoToMenu()
    {
        GotoScene("res://Scenes/Main.tscn");
    }
}