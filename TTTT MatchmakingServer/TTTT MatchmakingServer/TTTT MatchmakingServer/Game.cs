using System;
using System.Collections.Generic;
using System.Text;

namespace TTTT_MatchmakingServer
{
    class Game
    {
        public int hostID;

        public int maxPlayers;

        public int playerCount;

        public string gameName;

        public string ip;

        public int port;

        public int backup = -1;

        public Game()
        {
            playerCount = 1;
        }

        public void Update(int _newPlayerCount)
        {
            playerCount = _newPlayerCount;
        }

        public void AddBackup(int _id)
        {
            backup = _id;
        }
    }
}
