using Godot;
using System;

public class SceneManager : Node
{
    public static bool isServer;
    public static string username;
    public override void _Ready()
    {
        username = "";
        isServer = false;
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
        isServer = true;
    }

    public void StartClient()
    {
        GotoScene("res://Scenes/Client.tscn");
    }

    public void GoToMenu()
    {
        GameManager.Clear();
        GotoScene("res://Scenes/Main.tscn");
        isServer = false;
    }
}