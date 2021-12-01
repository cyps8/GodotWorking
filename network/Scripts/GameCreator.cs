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
        if (GetNode<LineEdit>("CanvasLayer/ServerName").Text != "")
        Server.gameName =  GetNode<LineEdit>("CanvasLayer/ServerName").Text;

        if (GetNode<OptionButton>("CanvasLayer/PlayerCount").Selected == 0)
        Server.maxPlayers = 1;
        else if (GetNode<OptionButton>("CanvasLayer/PlayerCount").Selected == 1)
        Server.maxPlayers = 3;
        else if (GetNode<OptionButton>("CanvasLayer/PlayerCount").Selected == 2)
        Server.maxPlayers = 7;
        else if (GetNode<OptionButton>("CanvasLayer/PlayerCount").Selected == 3)
        Server.maxPlayers = 15;


        GetNode<Node>("/root/MasterScene").Call("StartServer");
    }
}
