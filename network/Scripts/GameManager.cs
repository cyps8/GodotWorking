using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node2D
{
    [Export]
    public PackedScene otherPlayer;

    [Export]
    public PackedScene bullet;
    public static List<OtherPlayer> otherPlayers = new List<OtherPlayer>();
    public static List<Bullet> bullets = new List<Bullet>();

    public struct OPValues
    {
        public int id;
        public string username;
        public Vector2 position;
    }

    public struct BValues
    {
        public Vector2 position;
        public float rotation;
    }

    public static List<OPValues> otherPlayersQueue = new List<OPValues>();
    public static List<BValues> bulletsQueue = new List<BValues>();

    public static OPValues GetPlayerInfo(int index)
    {
        OPValues values = new OPValues();
        values.id = otherPlayers[index].id;
        values.username = otherPlayers[index].myUsername;
        values.position = otherPlayers[index].Position;
        return values;
    }

    public static void NewPlayer(int _id, string _username, Vector2 _position)
    {
        OPValues values = new OPValues();

        values.id = _id;
        values.username = _username;
        values.position = _position;

        otherPlayersQueue.Add(values);
    }

    public static void NewBullet(Vector2 _position, float _rotation)
    {
        BValues values = new BValues();

        values.position = _position;
        values.rotation = _rotation;

        bulletsQueue.Add(values);
    }

    public override void _Process(float dt)
    {
        while (otherPlayersQueue.Count > 0)
        {
            OtherPlayer newPlayer = otherPlayer.Instance<OtherPlayer>();
            newPlayer.Init(otherPlayersQueue[0].id, otherPlayersQueue[0].username, otherPlayersQueue[0].position);
            otherPlayers.Add(newPlayer);
            AddChild(newPlayer);
            otherPlayersQueue.RemoveAt(0);
        }

        while (bulletsQueue.Count > 0)
        {
            Bullet newBullet = bullet.Instance<Bullet>();
            newBullet.Init(bulletsQueue[0].position, bulletsQueue[0].rotation);
            bullets.Add(newBullet);
            AddChild(newBullet);
            bulletsQueue.RemoveAt(0);
        }

        for (int i = 0; i < bullets.Count; i++)
        {
            if (bullets[i].toBeRemoved)
            {
                bullets[i].Delete();
                bullets.RemoveAt(i);
                i--;
            }
        }
    }

    public static void UpdatePlayerPos(int _id, Vector2 _pos)
    {
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _id)
            {
                otherPlayers[i].UpdatePos(_pos);
                return;
            }
        }
    }

    public static void DeletePlayer(int _id)
    {
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _id)
            {
                otherPlayers[i].Delete();
                otherPlayers.RemoveAt(i);
                return;
            }
        }
    }

    public static void Clear()
    {
        otherPlayersQueue.Clear();
        otherPlayers.Clear();
        bulletsQueue.Clear();
        bullets.Clear();
    }
}
