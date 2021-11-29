using System;
using System.Net;
using System.Net.Sockets;

namespace TTTT_MatchmakingServer
{
    public class DataManager
    {
        private static void ServerSendTCP(int _toClient, Packet _packet)
        {
            _packet.WriteLength();
            Server.connections[_toClient].SendTCP(_packet);
        }

        public class Send
        {
            public static void Welcome(int _toClient, string _msg)
            {
                using (Packet _packet = new Packet((int)MMServerPackets.welcome))
                {
                    _packet.Write(_msg);
                    _packet.Write(_toClient);

                    ServerSendTCP(_toClient, _packet);
                }
            }

            public static void GamesData(int _toClient)
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
        }

        public class Handle
        {
            public static void WelcomeReceived(int _fromClient, Packet _packet)
            {
                int clientIDCheck = _packet.ReadInt();
                string username = _packet.ReadString();

                Console.WriteLine($"{Server.connections[_fromClient].tcpClient.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}.");
                if (_fromClient != clientIDCheck)
                {
                    Console.WriteLine($"Player {username} (ID: {_fromClient}) has assumed the wrong client ID ({clientIDCheck})!");
                }
            }

            public static void NewGame(int _fromClient, Packet _packet)
            {
                int maxPlayers = _packet.ReadInt();
                string gameName = _packet.ReadString();

                GamesManager.NewGame(_fromClient, maxPlayers, gameName);
            }

            public static void GameClosed(int _fromClient, Packet _packet)
            {
                GamesManager.GameClosed(_fromClient);
            }

            public static void RequestData(int _fromClient, Packet _packet)
            {
                Send.GamesData(_fromClient);
            }

            public static void GameData(int _fromClient, Packet _packet)
            {
                int playerCount = _packet.ReadInt();
                GamesManager.Update(_fromClient, playerCount);
            }
        }
    }
}
