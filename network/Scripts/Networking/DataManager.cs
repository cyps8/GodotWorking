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
                _packet.Write(Player.position);

                for (int i = 0; i < GameManager.otherPlayers.Count; i++)
                {
                    _packet.Write(GameManager.GetPlayerInfo(i).id);
                    _packet.Write(GameManager.GetPlayerInfo(i).username);
                    _packet.Write(GameManager.GetPlayerInfo(i).position);
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

        public static void ServerChatMsg(string _msg)
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(_msg);

                ServerSendTCPAll(_packet);
            }
        }

        public static void ServerSpreadChatMsg(string _msg, int _id)
        {
            using (Packet _packet = new Packet((int)ServerPackets.chatMsg))
            {
                _packet.Write(_msg);

                ServerSendTCPAll(_id, _packet);
            }
        }

        public static void ClientChatMsg(string _msg)
        {
            using (Packet _packet = new Packet((int)ClientPackets.chatMsg))
            {
                _packet.Write(_msg);

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
    }
    public class Handle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int clientIDCheck = _packet.ReadInt();
            string username = _packet.ReadString();

            GD.Print($"{Server.connections[_fromClient].tcpClient.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != clientIDCheck)
            {
                GD.Print($"Player {username} (ID: {_fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
            }
            GameManager.NewPlayer(_fromClient, username, new Vector2(0, 0));
            Send.NewPlayer(username, _fromClient, new Vector2(0, 0));
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
                GameManager.NewPlayer(id, username, position);
            }

            Send.WelcomeReceived();

            Client.ConnectUDP(((IPEndPoint)Client.tcpClient.Client.LocalEndPoint).Port);
        }

        public static void PlayerDisconnected(Packet _packet)
        {
            int _id = _packet.ReadInt();
            GameManager.DeletePlayer(_id);
        }

        public static void ClientChatMsg(int _fromClient, Packet _packet) // How a server handles a client chat message
        {
            string _msg = _packet.ReadString();

            TextBox.AddMsg(_msg);

            Send.ServerSpreadChatMsg(_msg, _fromClient);
        }

        public static void ServerChatMsg(Packet _packet) // How a client handles a server chat message
        {
            string _msg = _packet.ReadString();

            TextBox.AddMsg(_msg);
        }

        public static void NewPlayer(Packet _packet) // How a client handles a new player
        {
            int id = _packet.ReadInt();
            string username = _packet.ReadString();
            Vector2 position = _packet.ReadVector2();
            GameManager.NewPlayer(id, username, position);
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
    }
}
