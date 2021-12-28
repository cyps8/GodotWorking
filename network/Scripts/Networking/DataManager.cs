using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class DataManager // Class with methods that deal with sending data and handling data
{
    private static void ServerSendTCP(int _toClient, Packet _packet) // How a server sends a packet to a specific client using TCP
    {
        _packet.WriteLength();
        Server.connections[_toClient].SendTCP(_packet);
    }

    private static void ServerSendUDP(int _toClient, Packet _packet) // How a server sends a packet to a specific client using UDP
    {
        _packet.WriteLength();
        Server.connections[_toClient].SendUDP(_packet);
    }

    private static void ServerSendTCPAll(Packet _packet) // How a server sends a packet to all clients using TCP
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            Server.connections[i].SendTCP(_packet);
        }
    }

    private static void ServerSendTCPAll(int _except, Packet _packet) // How a server sends a packet to all clients except a specific client using TCP
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

    private static void ServerSendUDPAll(Packet _packet) // How a server sends a packet to all clients using UDP
    {
        _packet.WriteLength();
        for (int i = 0; i <= Server.maxPlayers; i++)
        {
            Server.connections[i].SendUDP(_packet);
        }
    }

    private static void ServerSendUDPAll(int _except, Packet _packet) // How a server sends a packet to all clients except a specific client using UDP
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

    private static void ClientSendTCP(Packet _packet) // How a client sends a packet to the server using TCP
    {
        _packet.WriteLength();
        Client.SendTCP(_packet);
    }

    private static void ClientSendUDP(Packet _packet) // How a client sends a packet to the server using UDP
    {
        _packet.WriteLength();
        Client.SendUDP(_packet);
    }

    private static void MMClientSendTCP(Packet _packet) // How a matchmaking client sends a packet to the matchmaking server using TCP
    {
        _packet.WriteLength();
        MMClient.SendTCP(_packet);
    }

    public class Send // Methods of how data is sent
    {
        public static void Welcome(int _toClient, string _msg) // Server sending welcome package
        {
            using (Packet _packet = new Packet((int)ServerPackets.welcome))
            {
                _packet.Write(_msg);
                _packet.Write(_toClient);
                _packet.Write(GameManager.GetFreeSpawn(true));
                
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

        public static void WelcomeReceived() // Client sending welcome response
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.id);
                _packet.Write(SceneManager.username);
                _packet.Write(MyPlayer.kinBody.Position);

                ClientSendTCP(_packet);
            }
        }
        public static void PlayerDisconnected(int _id) // Server sending a player disconnection
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerDisconnected))
            {
                _packet.Write(_id);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerChatMsg(string _msg, int _msgType) // Server sending a chat message
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(-1);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadChatMsg(string _msg, int _msgType, int _id) // Server spreading a chat message received from client
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(_id);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientChatMsg(string _msg, int _msgType) // Client sending a chat message
        {
            using (Packet _packet = new Packet((int)ClientPackets.chatMsg))
            {
                _packet.Write(Client.id);
                _packet.Write(_msg);
                _packet.Write(_msgType);

                ClientSendTCP(_packet);
            }
        }

        public static void NewPlayer(string _name, int _id, Vector2 _pos) // Server sending a player joining
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

        public static void ServerMovement(Vector2 _pos) // Server sending player movement
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
            {
                _packet.Write(-1);
                _packet.Write(_pos);

                ServerSendUDPAll(_packet);
            }
        }

        public static void ServerSpreadMovement(Vector2 _pos, int _id) // Server spreading player movement from a client
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerMovement))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendUDPAll(_id, _packet);
            }
        }

        public static void ClientMovement(Vector2 _pos) // Client sending player movement
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
            {
                _packet.Write(Client.id);
                _packet.Write(_pos);

                ClientSendUDP(_packet);
            }
        }

        public static void ServerVoiceChat(byte[] _bytes) // Server sending voice chat data
        {
            using (Packet _packet = new Packet((int)ServerPackets.voiceChat))
            {
                _packet.Write(-1);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ServerSendUDPAll(_packet);
            }
        }

        public static void ServerSpreadVoiceChat(byte[] _bytes, int _id) // Server spreading voice chat data received from a client
        {
            using (Packet _packet = new Packet((int)ServerPackets.voiceChat))
            {
                _packet.Write(_id);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ServerSendUDPAll(_id, _packet);
            }
        }

        public static void ClientVoiceChat(byte[] _bytes) // Client sending voice chat data
        {
            using (Packet _packet = new Packet((int)ClientPackets.voiceChat))
            {
                _packet.Write(Client.id);
                _packet.Write(_bytes.Length);
                _packet.Write(_bytes);

                ClientSendUDP(_packet);
            }
        }

        public static void ServerNewBullet(Vector2 _position, Vector2 _direction) // Server sending new bullet data
        {
            using (Packet _packet = new Packet((int)ServerPackets.newBullet))
            {
                _packet.Write(-1);
                _packet.Write(_position);
                _packet.Write(_direction);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadNewBullet(Vector2 _position, Vector2 _direction, int _id) // Server spreading new bullet data from a client
        {
            using (Packet _packet = new Packet((int)ServerPackets.newBullet))
            {
                _packet.Write(_id);
                _packet.Write(_position);
                _packet.Write(_direction);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientNewBullet(Vector2 _position, Vector2 _direction) // Client sending new bullet data
        {
            using (Packet _packet = new Packet((int)ClientPackets.newBullet))
            {
                _packet.Write(Client.id);
                _packet.Write(_position);
                _packet.Write(_direction);

                ClientSendTCP(_packet);
            }
        }

        public static void ServerHurt(float _dmg, int _hurtId) // Server sending hurt data
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadHurt(float _dmg, int _hurtId, int _id) // Server spreading hurt data from client
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientHurt(float _dmg, int _hurtId) // Client sending hurt data
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerHurt))
            {
                _packet.Write(_dmg);
                _packet.Write(_hurtId);

                ClientSendTCP(_packet);
            }
        }

        public static void ServerRespawn(int _id, Vector2 _pos) // Server sending respawn data
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadRespawn(int _id, Vector2 _pos) // Server spreading respawn data from a client
        {
            using (Packet _packet = new Packet((int)ServerPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientRespawn(int _id, Vector2 _pos) // Client sending respawn data
        {
            using (Packet _packet = new Packet((int)ClientPackets.playerRespawn))
            {
                _packet.Write(_id);
                _packet.Write(_pos);

                ClientSendTCP(_packet);
            }
        }

        public static void SyncTimer(int _toClient, float _timerTime) // Server sending timer data to client
        {
            using (Packet _packet = new Packet((int)ServerPackets.timerSync))
            {
                _packet.Write(_timerTime);
                _packet.Write(_toClient);

                ServerSendTCP(_toClient, _packet);
            }
        }

        public static void ServerPing() // Server sending ping to all clients
        {
            using (Packet _packet = new Packet((int)ServerPackets.ping))
            {
                GameManager.StartPing();

                _packet.Write(GameManager.otherPlayers.Count);

                for (int i = 0; i < GameManager.otherPlayers.Count; i++)
                {
                    _packet.Write(GameManager.otherPlayers[i].id);
                    _packet.Write(GameManager.otherPlayers[i].ping);
                }

                ServerSendTCPAll(_packet);
            }
        }

        public static void ClientPing(int _id) // Client sending ping to server
        {
            using (Packet _packet = new Packet((int)ClientPackets.ping))
            {
                _packet.Write(_id);

                ClientSendTCP(_packet);
            }
        }

        public static void MMWelcomeReceived() // Matchmaking client sending welcome package response
        {
            using (Packet _packet = new Packet((int)MMClientPackets.welcomeReceived))
            {
                _packet.Write(MMClient.id);
                _packet.Write(SceneManager.username);

                MMClientSendTCP(_packet);
            }
        }

        public static void MMNewGame() // Matchmaking client sending new server data
        {
            using (Packet _packet = new Packet((int)MMClientPackets.newGame))
            {
                _packet.Write(Server.maxPlayers + 1);
                _packet.Write(Server.gameName);
                _packet.Write(Server.port);

                MMClientSendTCP(_packet);
            }
        }

        public static void MMGameClosed() // Matchmaking client sendign server closed data
        {
            using (Packet _packet = new Packet((int)MMClientPackets.gameClosed))
            {
                MMClientSendTCP(_packet);
            }
        }

        public static void MMRequestData() // Matchmaking client sending a get data request
        {
            using (Packet _packet = new Packet((int)MMClientPackets.requestData))
            {
                MMClientSendTCP(_packet);
            }
        }

        public static void MMGameData() // Matchmaking client sending updataed server data to matchmaking server
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

        public static void MMAttemptJoin(int _id) // Matchmaking client sending request to join a server
        {
            using (Packet _packet = new Packet((int)MMClientPackets.attemptJoin))
            {
                _packet.Write(_id);
                MMClientSendTCP(_packet);
            }
        }
    }
    public class Handle // Methods of how received data is handled
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet) // How a server handles a client welcome response
        {
            int clientIDCheck = _packet.ReadInt();
            string username = _packet.ReadString();
            Vector2 pos = _packet.ReadVector2();

            GD.Print($"{username} connected successfully and is now player {_fromClient}.");
            if (_fromClient != clientIDCheck)
            {
                GD.Print($"Player {username} (ID: {_fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            GameManager.NewPlayer(_fromClient, username, pos, 200);
            Send.NewPlayer(username, _fromClient, pos);

            Send.SyncTimer(_fromClient, GameManager.timer);

            TextBox.PlayerConnected(username);
        }

        public static void Welcome(Packet _packet) // How a client handles a welcome package
        {
            string _msg = _packet.ReadString();
            int _id = _packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();
            MyPlayer.kinBody.Position = pos;

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

            GameManager.timer = 0f;

            Send.WelcomeReceived();

            Client.ConnectUDP(((IPEndPoint)Client.tcpClient.Client.LocalEndPoint).Port);
        }

        public static void PlayerDisconnected(Packet _packet) // How a client handles a player disconnection
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

        public static void ClientNewBullet(int _fromClient, Packet _packet) // How a server handles a client new bullet
        {
            int id = _packet.ReadInt();
            Vector2 position = _packet.ReadVector2();
            Vector2 direction = _packet.ReadVector2();

            GameManager.NewBullet(position, direction, false, id);

            Send.ServerSpreadNewBullet(position, direction, _fromClient);
        }

        public static void ServerNewBullet(Packet _packet) // How a client handles a server new bullet
        {
            int id = _packet.ReadInt();
            Vector2 position = _packet.ReadVector2();
            Vector2 direction = _packet.ReadVector2();

            GameManager.NewBullet(position, direction, false, id);
        }

        public static void ClientHurt(int _fromClient, Packet _packet) // How a server handles a client hurt
        {
            float dmg = _packet.ReadFloat();
            int hurtId = _packet.ReadInt();

            GameManager.DmgPlayer(dmg, hurtId);

            Send.ServerSpreadHurt(dmg, hurtId, _fromClient);
        }

        public static void ServerHurt(Packet _packet) // How a client handles a server hurt
        {
            float dmg = _packet.ReadFloat();
            int hurtId = _packet.ReadInt();

            GameManager.DmgPlayer(dmg, hurtId);
        }

        public static void ClientRespawn(int _fromClient, Packet _packet) // How a server handles a client respawn
        {
            int id = _packet.ReadInt();
            Vector2 pos = _packet.ReadVector2();

            GameManager.Respawn(id, pos);

            Send.ServerSpreadRespawn(id, pos);
        }

        public static void ServerRespawn(Packet _packet) // How a client handles a server respawn
        {
            int id = _packet.ReadInt();
            
            Vector2 pos = _packet.ReadVector2();

            GameManager.Respawn(id, pos);
        }

        public static void SyncTimer(Packet _packet) // how a client handles a timer syncronization
        {
            float _time = _packet.ReadFloat();
            int _id = _packet.ReadInt();

            GameManager.timer = _time + (GameManager.timer / 2);
        }

        public static void ClientPing(int _fromClient, Packet _packet) // How a server handles a client ping
        {
            GameManager.SetPing(_fromClient);
        }

        public static void ServerPing(Packet _packet) // How a client handles a server ping
        {
            int count = _packet.ReadInt();
            for (int i = 0; i < count; i++)
            {
                int id = _packet.ReadInt();
                float ping = _packet.ReadFloat();
                GameManager.SetPing(id, ping);
            }
            
            Send.ClientPing(Client.id);
        }

        public static void MMWelcome(Packet _packet) // How a matchmaking client handles a welcome package
        {
            string _msg = _packet.ReadString();
            int _id = _packet.ReadInt();

            GD.Print($"Message from matchmaking server: {_msg}");
            MMClient.id = _id;

            Send.MMWelcomeReceived();

            if (!SceneManager.isServer)
            Send.MMRequestData();
        }

        public static void MMGamesData(Packet _packet) // How a matchmaking client handles receiving games data
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

        public static void MMSendJoin(Packet _packet) // How a matchmaking client handles joining a server
        {
            string ip = _packet.ReadString();
            int port = _packet.ReadInt();

            GameClient.StartServer(ip, port);
        }
    }
}