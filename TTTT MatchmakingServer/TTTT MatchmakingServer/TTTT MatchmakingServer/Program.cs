using System;

namespace TTTT_MatchmakingServer
{
    class Program
    {
        private static bool exit;

        static void Main(string[] args)
        {
            exit = false;
            Console.Title = "TTTT MatchmakingServer";
            Server.ServerStart(42068);
            while (!exit)
            {
            }
        }
    }
}
