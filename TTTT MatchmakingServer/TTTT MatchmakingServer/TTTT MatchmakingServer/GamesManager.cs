using System;
using System.Collections.Generic;
using System.Text;

namespace TTTT_MatchmakingServer
{
    class GamesManager
    {
        public static List<Game> games = new List<Game>();

        public static void NewGame(int _hostID, int _maxPlayers, string _gameName, int _port)
        {
            Game newGame = new Game()
            {
                hostID = _hostID,
                maxPlayers = _maxPlayers,
                gameName = _gameName,
                ip = Server.connections[_hostID].ip,
                port = _port
            };
            games.Add(newGame);
        }

        public static void GameClosed(int _hostID)
        {
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].hostID == _hostID)
                {
                    if (games[i].backup != -1)
                    {
                        // host migrate
                        return;
                    }
                    games.RemoveAt(i);
                    return;
                }
            }
        }

        public static Game GetData(int _i)
        {
            return games[_i];
        }

        public static Game GetIDData(int _i)
        {
            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].hostID == _i)
                return games[i];
            }
            return games[0];
        }

        public static void Update(int _hostID, int _newPlayerCount)
        {
            GetIDData(_hostID).Update(_newPlayerCount);
        }
    }
}
