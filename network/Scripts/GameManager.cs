using Godot;
using System;
using System.Collections.Generic;

public class GameManager : Node2D
{
    [Export]
    public PackedScene otherPlayer;

    [Export]
    public PackedScene bullet;
    public static MyPlayer userPlayer;
    public static List<OtherPlayer> otherPlayers = new List<OtherPlayer>();
    public static List<Bullet> bullets = new List<Bullet>();

    public struct OPValues
    {
        public int id;
        public string username;
        public Vector2 position;
        public float hP;
    }

    public struct BValues
    {
        public Vector2 position;
        public Vector2 direction;
        public bool isMine;
        public int owner;
    }

    public class Spawn
    {
        public Vector2 position;
        public bool isFree;

        public bool GetFree()
        {return isFree; }

        public void SetFree(bool _new)
        {isFree = _new; }
    }

    public static List<OPValues> otherPlayersQueue = new List<OPValues>();
    public static List<BValues> bulletsQueue = new List<BValues>();
    public static List<Spawn> spawns = new List<Spawn>();
    public static float timer;
    public static Label timerLbl;
    public static AudioStreamPlayer2D audioStream;
    private static AudioStreamSample _recording;
    public static float pingSent;
    private float pingTimer = 0;

    public override void _Ready()
    {
        timer = 0f;
        pingSent = 0f;
        audioStream = GetNode<AudioStreamPlayer2D>("AudioPlayer");
        _recording = new AudioStreamSample();
        userPlayer = GetNode<MyPlayer>("UserPlayer");
        timerLbl = GetNode<Label>("HUD/Timer");
        AddSpawn(new Vector2(0, 0));
        AddSpawn(new Vector2(-500, 0));
        AddSpawn(new Vector2(-1000, 0));
        AddSpawn(new Vector2(-250, 300));
    }

    private void AddSpawn(Vector2 _pos)
    {
        Spawn newSpawn = new Spawn();
        newSpawn.position = _pos;
        newSpawn.isFree = true;
        spawns.Add(newSpawn);
    }

    public static Vector2 GetFreeSpawn(bool _checkSelf)
    {
        Vector2 pos = new Vector2(0, 0);
        for (int i = 0; i < spawns.Count; i++)
        {
            spawns[i].SetFree(true);
        }
        if (_checkSelf)
        {
            int closest = 0;
            float distance = 99999f;
            for (int i = 0; i < spawns.Count; i++)
            {
                if (Mathf.Abs((spawns[i].position - MyPlayer.kinBody.Position).Length()) < distance)
                {
                    closest = i;
                    distance = Mathf.Abs((spawns[i].position - MyPlayer.kinBody.Position).Length());
                }
            }
            spawns[closest].SetFree(false);
        }
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            int closest = 0;
            float distance = 99999f;
            for (int j = 0; j < spawns.Count; j++)
            {
                if (Mathf.Abs((spawns[j].position - otherPlayers[i].kinBody.Position).Length()) < distance)
                {
                    closest = j;
                    distance = Mathf.Abs((spawns[j].position - otherPlayers[i].kinBody.Position).Length());
                }
            }
            spawns[closest].SetFree(false);
        }
        for (int i = 0; i < spawns.Count; i++)
        {
            if (spawns[i].GetFree())
            {
                pos = spawns[i].position;
                return pos;
            }
        }
        return pos;
    }

    public static void PlayVoice(byte[] _voiceData)
    {
        _recording.Data = _voiceData;
        audioStream.Stream = _recording;
        audioStream.Play();
    }

    public static OPValues GetPlayerInfo(int index)
    {
        OPValues values = new OPValues();
        values.id = otherPlayers[index].id;
        values.username = otherPlayers[index].myUsername;
        values.position = otherPlayers[index].Position;
        values.hP = otherPlayers[index].healthPoints;
        return values;
    }

    public static OPValues GetPlayerIdInfo(int _id)
    {
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _id)
            {
                OPValues values = new OPValues();
                values.id = otherPlayers[i].id;
                values.username = otherPlayers[i].myUsername;
                values.position = otherPlayers[i].Position;
                values.hP = otherPlayers[i].healthPoints;
                return values;
            }
        }

        return new OPValues(); // shouldn't happen
    }

    public static void NewPlayer(int _id, string _username, Vector2 _position, float _hP)
    {
        OPValues values = new OPValues();

        values.id = _id;
        values.username = _username;
        values.position = _position;
        values.hP = _hP;

        otherPlayersQueue.Add(values);
    }

    public static void NewBullet(Vector2 _position, Vector2 _direction, bool _isMine, int _owner)
    {
        BValues values = new BValues();

        values.position = _position;
        values.direction = _direction;
        values.isMine = _isMine;
        values.owner = _owner;

        bulletsQueue.Add(values);
    }

    public override void _Process(float dt)
    {
        timer += dt;
        float _timer = Mathf.Round(timer * 1000.0f);
        timerLbl.Text = (_timer / 1000).ToString();

        pingTimer += dt;
        if (pingTimer > 3 && Server.connections.Count > 0)
        {
            pingTimer = 0f;
            DataManager.Send.ServerPing();
        }

        while (otherPlayersQueue.Count > 0)
        {
            OtherPlayer newPlayer = otherPlayer.Instance<OtherPlayer>();
            newPlayer.Init(otherPlayersQueue[0].id, otherPlayersQueue[0].username, otherPlayersQueue[0].position, otherPlayersQueue[0].hP);
            otherPlayers.Add(newPlayer);
            AddChild(newPlayer);
            otherPlayersQueue.RemoveAt(0);
        }

        while (bulletsQueue.Count > 0)
        {
            Bullet newBullet = bullet.Instance<Bullet>();
            AddChild(newBullet);
            newBullet.Init(bulletsQueue[0].position, bulletsQueue[0].direction, bulletsQueue[0].isMine, bulletsQueue[0].owner);
            bullets.Add(newBullet);
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

        GetNode<Button>("HUD/RespawnButton").Visible = userPlayer.isDead;
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

    public static void StartPing()
    {
        pingSent = timer;
    }

    public static void SetPing(int _id, float _ping = 0)
    {
        if (SceneManager.isServer)
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _id)
            {
                otherPlayers[i].UpdatePing((timer - pingSent) / 2);
                return;
            }
        }
        else
        {
            if (_id == Client.id)
            {
                userPlayer.UpdatePing(_ping / 1000);
            }

            for (int i = 0; i < otherPlayers.Count; i++)
            {
                if (otherPlayers[i].id == _id)
                {
                    otherPlayers[i].UpdatePing(_ping / 1000);
                    return;
                }
            }
        }
    }

    public static void Respawn(int _id, Vector2 _pos)
    {
        if(SceneManager.isServer == true)
        {
            if (_id == -1)
            {
                userPlayer.Respawn(_pos);
                return;
            }
        }
        else
        {
            if (_id == Client.id)
            {
                userPlayer.Respawn(_pos);
                return;
            }
        }

        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _id)
            {
                otherPlayers[i].Respawn(_pos);
                return;
            }
        }
    }

    public void RespawnPressed()
    {
        Vector2 respawnPos = new Vector2(200, 0);
        userPlayer.Respawn(respawnPos);

        if(SceneManager.isServer == true)
        DataManager.Send.ServerRespawn(-1, respawnPos);
        else
        DataManager.Send.ClientRespawn(Client.id, respawnPos);
    }

    public static void DmgPlayer(float _dmg, int _hurtId)
    {
        if(SceneManager.isServer == true)
        {
            if (_hurtId == -1)
            {
                userPlayer.Hurt(_dmg);
            }
        }
        else
        {
            if (_hurtId == Client.id)
            {
                userPlayer.Hurt(_dmg);
            }
        }
        
        for (int i = 0; i < otherPlayers.Count; i++)
        {
            if (otherPlayers[i].id == _hurtId)
            {
                otherPlayers[i].Hurt(false, _dmg);
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
