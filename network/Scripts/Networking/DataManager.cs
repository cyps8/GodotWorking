using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class DataManager
{
    private static void ServerSendTCP(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.connections[_toClient].SendTCP(_packet);
    }

    private static void ServerSendUDP(int _toClient, Packet _packet)
    {
        _packet.WriteLength();
        Server.connections[_toClient].SendUDP(_packet);
    }

    private static void ServerSendTCPAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            Server.connections[i].SendTCP(_packet);
        }
    }

    private static void ServerSendTCPAll(int _except, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            if (i != _except)
            {
                Server.connections[i].SendTCP(_packet);
            }
        }
    }

    private static void ServerSendUDPAll(Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            Server.connections[i].SendUDP(_packet);
        }
    }

    private static void ServerSendUDPAll(int _except, Packet _packet)
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            if (i != _except)
            {
                Server.connections[i].SendUDP(_packet);
            }
        }
    }

    private static void ClientSendTCP(Packet _packet)
    {
        _packet.WriteLength();
        Client.SendTCP(_packet);
    }

    private static void ClientSendUDP(Packet _packet)
    {
        _packet.WriteLength();
        Client.SendUDP(_packet);
    }

    private static void MMClientSendTCP(Packet _packet)
    {
        _packet.WriteLength();
        MMClient.SendTCP(_packet);
    }

    public class Send
    {
        public static void Welcome(int _toClient, string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                
                _packet.Write(GameManager.otherPlayers.Count + 1);

                _packet.Write(-1);
                _packet.Write(SceneManager.username);
                _packet.Write(MyPlayer.kinBody.Position);
                _packet.Write(MyPlayer.healthPoints);

                for (int i = 0; i < GameManager.otherPlayers.Count; i++)
                {
                    _packet.Write(GameManager.GetPlayerInfo(i).id);
                    _packet.Write(GameManager.GetPlayerInfo(i).username);
                    _packet.Write(GameManager.GetPlayerInfo(i).position);
                    _packet.Write(GameManager.GetPlayerInfo(i).hP);
                }

                ServerSendTCP(_toClient, _packet);
            }
        }

        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.id);
                _packet.Write(SceneManager.username);

                ClientSendTCP(_packet);
            }
        }
        public static void PlayerDisconnected(int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
            {
                _packet.Write(_id);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerChatMsg(string _msg, int _msgType)
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(-1);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadChatMsg(string _msg, int _msgType, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(_id);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientChatMsg(string _msg, int _msgType)
        {
            using (Packet _packet = new Packet((int)ClientPackets.chatMsg))
            {
                _packet.Write(Client.id);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ClientSendTCP(_packet);
            }
        }

        public static void NewPlayer(string _name, int _id, Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.newPlayer))
            {
                _packet.Write(_id);
                _packet.Write(_name);
                _packet.Write(_pos);
                _packet.Write(200f);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ServerMovement(Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
            {
                _packet.Write(-1);
                _packet.Write(_pos);

                ServerSendUDPAll(_packet);
            }
        }

        public static void ServerSpreadMovement(Vector2 _pos, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendUDPAll(_id, _packet);
            }
        }

        public static void ClientMovement(Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
            {
                _packet.Write(Client.id);
                _packet.Write(_pos);

                ClientSendUDP(_packet);
            }
        }

        public static void ServerVoiceChat(byte[] _bytes)
        {
            using (Packet _packet = new Packet((int)ServerPackets.voiceChat))
            {
                _packet.Write(-1);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ServerSendUDPAll(_packet);
            }
        }

        public static void ServerSpreadVoiceChat(byte[] _bytes, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.voiceChat))
            {
                _packet.Write(_id);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ServerSendUDPAll(_id, _packet);
            }
        }

        public static void ClientVoiceChat(byte[] _bytes)
        {
            using (Packet _packet = new Packet((int)ClientPackets.voiceChat))
            {
                _packet.Write(Client.id);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ClientSendUDP(_packet);
            }
        }

        public static void ServerNewBullet(Vector2 _position, Vector2 _direction)
        {
            using (Packet _packet = new Packet((int)ServerPackets.newBullet))
            {
                _packet.Write(-1);
                _packet.Write(_position);
                _packet.Write(_direction);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadNewBullet(Vector2 _position, Vector2 _direction, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.newBullet))
            {
                _packet.Write(_id);
                _packet.Write(_position);
                _packet.Write(_direction);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientNewBullet(Vector2 _position, Vector2 _direction)
        {
            using (Packet _packet = new Packet((int)ClientPackets.newBullet))
            {
                _packet.Write(Client.id);
                _packet.Write(_position);
                _packet.Write(_direction);

                ClientSendTCP(_packet);
            }
        }

        public static void ServerHurt(float _dmg, int _hurtId)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadHurt(float _dmg, int _hurtId, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientHurt(float _dmg, int _hurtId)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ClientSendTCP(_packet);
            }
        }

        public static void ServerRespawn(int _id, Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadRespawn(int _id, Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientRespawn(int _id, Vector2 _pos)
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ClientSendTCP(_packet);
            }
        }

        public static void MMWelcomeReceived()
        {
            using (Packet _packet = new Packet((int)MMClientPackets.welcomeReceived))
            {
                _packet.Write(MMClient.id);
                _packet.Write(SceneManager.username);

                MMClientSendTCP(_packet);
            }
        }

        public static void MMNewGame()
        {
            using (Packet _packet = new Packet((int)MMClientPackets.newGame))
            {
                _packet.Write(Server.maxPlayers + 1);
                _packet.Write(Server.gameName);

                MMClientSendTCP(_packet);
            }
        }

        public static void MMGameClosed()
        {
            using (Packet _packet = new Packet((int)MMClientPackets.gameClosed))
            {
                MMClientSendTCP(_packet);
            }
        }

        public static void MMRequestData()
        {
            using (Packet _packet = new Packet((int)MMClientPackets.requestData))
            {
                MMClientSendTCP(_packet);
            }
        }

        public static void MMGameData()
        {
            using (Packet _packet = new Packet((int)MMClientPackets.gameData))
            {
                int playerCount = 1;
                for (int i = 0; i < Server.connections.Count; i++)
                {
                    if (Server.connections[i].isConnected)
                    playerCount++;
                }
                _packet.Write(playerCount);
                MMClientSendTCP(_packet);
            }
        }
    }
    public class Handle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int clientIDCheck = _packet.ReadInt();
            string username = _packet.ReadString();

            GD.Print($"{username} connected successfully and is now player {_fromClient}.");
            if (_fromClient != clientIDCheck)
            {
                GD.Print($"Player {username} (ID: {_fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            GameManager.NewPlayer(_fromClient, username, new Vector2(0, 0), 200);
            Send.NewPlayer(username, _fromClient, new Vector2(0, 0));

            TextBox.PlayerConnected(username);
        }

        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _id = _packet.ReadInt();

            GD.Print($"Message from server: {_msg}");
            Client.id = _id;

            int newPlayers = _packet.ReadInt();

            for (int i = 0; i < newPlayers; i++)
            {
                int id = _packet.ReadInt();
                string username = _packet.ReadString();
                Vector2 position = _packet.ReadVector2();
                float hP = _packet.ReadFloat();
                GameManager.NewPlayer(id, username, position, hP);
            }

            Send.WelcomeReceived();

            Client.ConnectUDP(((IPEndPoint)Client.tcpClient.Client.LocalEndPoint).Port);
        }

        public static void PlayerDisconnected(Packet _packet)
        {
            int _id = _packet.ReadInt();
            TextBox.PlayerDisconnected(_id);
            GameManager.DeletePlayer(_id);
        }

        public static void ClientChatMsg(int _fromClient, Packet _packet) // How a server handles a client chat message
        {
            int _from = _packet.ReadInt();
            string _msg = _packet.ReadString();
            int _msgType = _packet.ReadInt();

            TextBox.AddMsg(_from, _msg, _msgType);

            Send.ServerSpreadChatMsg(_msg, _msgType, _fromClient);
        }

        public static void ServerChatMsg(Packet _packet) // How a client handles a server chat message
        {
            int _from = _packet.ReadInt();
            string _msg = _packet.ReadString();
            int _msgType = _packet.ReadInt();

            TextBox.AddMsg(_from, _msg, _msgType);
        }

        public static void NewPlayer(Packet _packet) // How a client handles a new player
        {
            int id = _packet.ReadInt();
            string username = _packet.ReadString();
            Vector2 position = _packet.ReadVector2();
            float hP = _packet.ReadFloat();
            GameManager.NewPlayer(id, username, position, 200);

            TextBox.PlayerConnected(username);
        }

        public static void ClientMovement(int _fromClient, Packet _packet) // How a server handles client movement
        {
            int id = _packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();

            GameManager.UpdatePlayerPos(id, pos);

            Send.ServerSpreadMovement(pos, _fromClient);
        }

        public static void ServerMovement(Packet _packet) // How a client handles server movement
        {
            int id = _packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();

            GameManager.UpdatePlayerPos(id, pos);
        }

        public static void ClientVoiceChat(int _fromClient, Packet _packet) // How a server handles client voice chat
        {
            int id = _packet.ReadInt();
            int length = _packet.ReadInt();
            byte[] data = _packet.ReadBytes(length);

            GameManager.PlayVoice(data);

            Send.ServerSpreadVoiceChat(data, _fromClient);
        }

        public static void ServerVoiceChat(Packet _packet) // How a client handles server voice chat
        {
            int id = _packet.ReadInt();
            int length = _packet.ReadInt();
            byte[] data = _packet.ReadBytes(length);

            GameManager.PlayVoice(data);
        }

        public static void ClientNewBullet(int _fromClient, Packet _packet) // How a server handles a client chat message
        {
            int id = _packet.ReadInt();
            Vector2 position = _packet.ReadVector2();
            Vector2 direction = _packet.ReadVector2();

            GameManager.NewBullet(position, direction, false, id);

            Send.ServerSpreadNewBullet(position, direction, _fromClient);
        }

        public static void ServerNewBullet(Packet _packet) // How a client handles a server chat message
        {
            int id = _packet.ReadInt();
            Vector2 position = _packet.ReadVector2();
            Vector2 direction = _packet.ReadVector2();

            GameManager.NewBullet(position, direction, false, id);
        }

        public static void ClientHurt(int _fromClient, Packet _packet) // How a server handles a client chat message
        {
            float dmg = _packet.ReadFloat();
            int hurtId = _packet.ReadInt();

            GameManager.DmgPlayer(dmg, hurtId);

            Send.ServerSpreadHurt(dmg, hurtId, _fromClient);
        }

        public static void ServerHurt(Packet _packet) // How a client handles a server chat message
        {
            float dmg = _packet.ReadFloat();
            int hurtId = _packet.ReadInt();

            GameManager.DmgPlayer(dmg, hurtId);
        }

        public static void ClientRespawn(int _fromClient, Packet _packet) // How a server handles a client chat message
        {
            int id = _packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();

            GameManager.Respawn(id, pos);

            Send.ServerSpreadRespawn(id, pos);
        }

        public static void ServerRespawn(Packet _packet) // How a client handles a server chat message
        {
            int id = _packet.ReadInt();
            
            Vector2 pos = _packet.ReadVector2();

            GameManager.Respawn(id, pos);
        }

        public static void MMWelcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _id = _packet.ReadInt();

            GD.Print($"Message from matchmaking server: {_msg}");
            MMClient.id = _id;

            Send.MMWelcomeReceived();

            if (!SceneManager.isServer)
            Send.MMRequestData();
        }

        public static void MMGamesData(Packet _packet)
        {
            int gamesCount = _packet.ReadInt();

            GameSelector.Clear();

            for (int i = 0; i < gamesCount; i++)
            {
                int id = _packet.ReadInt();
                int maxPlayers = _packet.ReadInt();
                int playerCount = _packet.ReadInt();
                string gameName = _packet.ReadString();
                GameSelector.AddGame(id, maxPlayers, playerCount, gameName);
            }
        }
    }
}