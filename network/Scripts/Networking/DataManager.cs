using Godot;
using System;

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

                ServerSendTCP(_toClient, _packet);
            }
        }

        public static void WelcomeReceived()
        {
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.id);
                //_packet.Write(); add string to be sent here

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
    }
    public class Handle
    {
        public static void WelcomeReceived(int _fromClient, Packet _packet)
        {
            int _clientIDCheck = _packet.ReadInt();
            //string _username = _packet.ReadString(); add string to be reveived here

            GD.Print($"{Server.connections[_fromClient].tcpClient.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
            if (_fromClient != _clientIDCheck)
            {
                GD.Print($"Player (ID: {_fromClient}) has assumed the wrong client ID ({_clientIDCheck})!");
                //GD.Print($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong client ID ({_clientIDCheck})!");
            }
            //Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void Welcome(Packet _packet)
        {
            string _msg = _packet.ReadString();
            int _id = _packet.ReadInt();

            GD.Print($"Message from server: {_msg}");
            Client.id = _id;
            Send.WelcomeReceived();

            //Client.ConnectUDP(((IPEndPoint)Client.tcpClient.Client.LocalEndPoint).Port);
        }

        public static void PlayerDisconnected(Packet _packet)
        {
            int _id = _packet.ReadInt();

            //GameManager.players[_id].QueueFree();
            //GameManager.players.Remove(_id);
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
    }
}
