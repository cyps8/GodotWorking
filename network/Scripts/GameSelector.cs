using Godot;
using System;
using System.Collections.Generic;

public class GameSelector : Node2D
{

    [Export]
    public PackedScene selectableGame;

    public static List<SelectableGame> selectableGames = new List<SelectableGame>();

    public struct GValues
    {
        public int id;
        public int maxPlayers;
        public int playerCount;
        public string gameName;
    }
    public static List<GValues> gamesQueue = new List<GValues>();
    public override void _Ready()
    {
        SceneManager.mmClient.StartClient();
    }

    public override void _Process(float dt)
    {
        while (gamesQueue.Count > 0)
        {
            SelectableGame newGame = selectableGame.Instance<SelectableGame>();
            newGame.MarginTop = 100 + (60 * selectableGames.Count);
            newGame.Init(gamesQueue[0].id, gamesQueue[0].maxPlayers, gamesQueue[0].playerCount, gamesQueue[0].gameName);
            GetNode<CanvasLayer>("CanvasLayer").AddChild(newGame);
            selectableGames.Add(newGame);
            gamesQueue.RemoveAt(0);
        }
    }
    public static void AddGame(int _id, int _maxPlayers, int _playerCount, string _gameName)
    {
        GValues values = new GValues();

        values.id = _id;
        values.maxPlayers = _maxPlayers;
        values.playerCount = _playerCount;
        values.gameName = _gameName;

        gamesQueue.Add(values);
    }

    public void Join()
    {
        SceneManager.mmClient.Disconnect();
    }

    public static void Refresh()
    {
        Clear();
        DataManager.Send.MMRequestData();
    }

    public void ButtonReturn()
    {
        GetNode<Node>("/root/MasterScene/GameSelector/MMClientManager").CallDeferred("Disconnect");
        SceneManager.mmClient.Disconnect();
    }

    public static void Clear()
    {
        for (int i = 0; i < selectableGames.Count; i++)
        {
            selectableGames[i].Delete();
        }
        selectableGames.Clear();
    }
}
