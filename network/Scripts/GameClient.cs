using Godot;
using System;

public class GameClient : Node2D
{
    Client client;
    CanvasLayer cl;
    public override void _Ready()
    {
        client = GetNode<Client>("ClientManager");
        client.StartClient();
        
        cl = GetNode<CanvasLayer>("/root/MasterScene/Client/Options");
        cl.GetNode<Button>("Button_Leave").Hide();
    }

    public void Connected()
    {
        cl.GetNode<Button>("Button_Leave").Show();
    }

    private void ButtonLeave()
    {
        GetNode<Node>("/root/MasterScene/Client/ClientManager").CallDeferred("Disconnect");
    }
}
