using Godot;
using System;

public class GameCreator : Node2D
{
    public override void _Ready()
    {
        SceneManager.mmClient.StartClient();
    }

    public void ButtonReturn()
    {
        SceneManager.mmClient.Disconnect();
        GetNode<Node>("/root/MasterScene").CallDeferred("GoToMenu");
    }

    public void ButtonCreate()
    {
        Server.gameName =  GetNode<LineEdit>("CanvasLayer/ServerName").Text;
        GetNode<Node>("/root/MasterScene").Call("StartServer");
    }
}
