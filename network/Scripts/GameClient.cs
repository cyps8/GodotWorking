using Godot;
using System;

public class GameClient : Node2D
{
    Client client;
    CanvasLayer cl;
    static string ip;
    static int port;
    public override void _Ready()
    {
        client = GetNode<Client>("ClientManager");
        port = 0;
        DataManager.Send.MMAttemptJoin(SceneManager.joined);
        while (port == 0)
        {
        }
        client.StartClient(ip, port);
        
        cl = GetNode<CanvasLayer>("/root/MasterScene/Client/Options");
        cl.GetNode<Button>("Button_Leave").Hide();
    }

    public static void StartServer(string _ip, int _port)
    {
        ip = _ip;
        port = _port;
    }

    public void Connected()
    {
        cl.GetNode<Button>("Button_Leave").Show();
    }

    private void ButtonLeave()
    {
        SceneManager.mmClient.Disconnect();
        GetNode<Node>("/root/MasterScene/Client/ClientManager").CallDeferred("Disconnect");
    }
}
