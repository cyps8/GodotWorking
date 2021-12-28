using System;
using System.Net;
using System.Net.Sockets;

namespace TTTT_MatchmakingServer
{
    public class DataManager // Class with methods that deal with sending data and handling data
    {
        private static void ServerSendTCP(int _toClient, Packet _packet)  // How a server sends a packet to a specific client using TCP
        {
            _packet.WriteLength();
            Server.connections[_toClient].SendTCP(_packet);
        }

        public class Send// Methods of how data is sent
        {
            public static void Welcome(int _toClient, string _msg) // Mathcmaking server sending welcome package
            {
                using (Packet _packet = new Packet((int)MMServerPackets.welcome))
                {
                    _packet.Write(_msg);
                    _packet.Write(_toClient);

                    ServerSendTCP(_toClient, _packet);
                }
            }

            public static void GamesData(int _toClient) // Matchmaking server sending games data
            {
                using (Packet _packet = new Packet((int)MMServerPackets.gamesData))
                {
                    _packet.Write(GamesManager.games.Count);

                    for (int i = 0; i < GamesManager.games.Count; i++)
                    {
                        _packet.Write(GamesManager.GetData(i).hostID);
                        _packet.Write(GamesManager.GetData(i).maxPlayers);
                        _packet.Write(GamesManager.GetData(i).playerCount);
                        _packet.Write(GamesManager.GetData(i).gameName);
                    }

                    ServerSendTCP(_toClient, _packet);
                }
            }

            public static void SendJoin(int _toClient, int _joinID) // Matchmaking server sending join data
            {
                using (Packet _packet = new Packet((int)MMServerPackets.sendJoin))
                {
                    string ip = GamesManager.GetIDData(_joinID).ip;
                    int i = 0;
                    while (i == 0)
                    {
                        if (ip.EndsWith(':'))
                        i++;
                        ip = ip.Remove(ip.Length - 1);
                    }
                    _packet.Write(ip);
                    _packet.Write(GamesManager.GetIDData(_joinID).port);

                    ServerSendTCP(_toClient, _packet);
                }
            }
        }

        public class Handle // Methods of how received data is handled
        {
            public static void WelcomeReceived(int _fromClient, Packet _packet) // How matchmaking server handles welcome package response
            {
                int clientIDCheck = _packet.ReadInt();
                string username = _packet.ReadString();

                Console.WriteLine($"{Server.connections[_fromClient].tcpClient.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                if (_fromClient != clientIDCheck)
                {
                    Console.WriteLine($"Player {username} (ID: {_fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
                }
            }

            public static void NewGame(int _fromClient, Packet _packet) // How matchmaking server handles new game data
            {
                int maxPlayers = _packet.ReadInt();
                string gameName = _packet.ReadString();
                int port = _packet.ReadInt();

                GamesManager.NewGame(_fromClient, maxPlayers, gameName, port);
            }

            public static void GameClosed(int _fromClient, Packet _packet) // How matchmaking server handles a server closing
            {
                GamesManager.GameClosed(_fromClient);
            }

            public static void RequestData(int _fromClient, Packet _packet) // How matchmaking server handles a games data request
            {
                Send.GamesData(_fromClient);
            }

            public static void GameData(int _fromClient, Packet _packet) // How matchmaking server handles updated game data
            {
                int playerCount = _packet.ReadInt();
                GamesManager.Update(_fromClient, playerCount);
            }

            public static void AttemptJoin(int _fromClient, Packet _packet) // How matchmaking server handles client attempting to join a server
            {
                int joinID = _packet.ReadInt();
                DataManager.Send.SendJoin(_fromClient, joinID);
            }
        }
    }
}
