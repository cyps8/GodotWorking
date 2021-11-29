using Godot;
using System;

public class SelectableGame : Button
{
    public void Init(int _id, int _maxPlayers, int _playerCount, string _gameName)
    {
        Text = $"{_gameName}    Players: {_playerCount}/{_maxPlayers}";
    }
    public void Selected()
    {
        GetNode<Node>("/root/MasterScene/GameSelector").Call("Join");
        GetNode<Node>("/root/MasterScene").Call("StartClient");
    }
    public void Delete()
    {
        QueueFree();
    }
}
