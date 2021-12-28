using Godot;
using System;

public class SelectableGame : Button
{
    int id;
    public void Init(int _id, int _maxPlayers, int _playerCount, string _gameName)
    {
        id = _id;
        Text = $"{_gameName}    Players: {_playerCount}/{_maxPlayers}";
    }
    public void Selected()
    {
        SceneManager.joined = id;
        GetNode<Node>("/root/MasterScene/GameSelector").Call("Join");
        GetNode<Node>("/root/MasterScene").Call("StartClient");
    }
    public void Delete()
    {
        QueueFree();
    }
}