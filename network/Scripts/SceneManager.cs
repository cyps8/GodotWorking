using Godot;
using System;

public class SceneManager : Node
{
    public static MMClient mmClient;
    public static bool isServer;
    public static string username;
    public static int joined;
    public override void _Ready()
    {
        username = "";
        isServer = false;

        mmClient = new MMClient();
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

    public void GoToGameSelector()
    {
        GameSelector.Refresh();
        GotoScene("res://Scenes/GameSelector.tscn");
    }

    public void GoToGameCreator()
    {
        GotoScene("res://Scenes/GameCreator.tscn");
    }

    public void GoToMenu()
    {
        GameManager.Clear();
        GameSelector.Clear();
        GotoScene("res://Scenes/Main.tscn");
        isServer = false;
    }
}