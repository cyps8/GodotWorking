using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TTTT_MatchmakingServer
{
	public class Server
	{
		private static int port;
		private static TcpListener tcpListener;
		public static List<Connection> connections = new List<Connection>();
		public delegate void PacketHandler(int _fromClient, Packet _packet);
		public static Dictionary<int, PacketHandler> packetHandlers;
		public static int maxConnections; // not including host
		static bool isConnected;
		public static void ServerStart(int _port)
		{
			port = _port;

			tcpListener = new TcpListener(IPAddress.Any, port);
			tcpListener.Start(128);

			Console.WriteLine($"Server started on port: {port}.");

			maxConnections = 100;

			Init();

			isConnected = true;

			tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
		}

		private static void Init()
		{
			for (int i = 0; i <= maxConnections; i++)
			{
				Connection _connection = new Connection();
				connections.Insert(i, _connection);
				connections[i].SetId(i);
			}

			packetHandlers = new Dictionary<int, PacketHandler>() // sets each ID to a method to be run
			{
				{ (int)MMClientPackets.welcomeReceived, DataManager.Handle.WelcomeReceived },
				{ (int)MMClientPackets.gameData, DataManager.Handle.GameData },
				{ (int)MMClientPackets.newGame, DataManager.Handle.NewGame },
				{ (int)MMClientPackets.gameClosed, DataManager.Handle.GameClosed },
				{ (int)MMClientPackets.requestData, DataManager.Handle.RequestData },
				{ (int)MMClientPackets.attemptJoin, DataManager.Handle.AttemptJoin },
			};
		}

		private static void TCPConnectCallback(IAsyncResult result)
		{
			if (tcpListener == null || isConnected == false)
				return;

			try
			{
				TcpClient _client = tcpListener.EndAcceptTcpClient(result);
				tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

				for (int i = 0; i < maxConnections; i++)
				{
					if (connections[i].GetTcpClient() == null)
					{
						connections[i].ConnectTcp(_client);
						Console.WriteLine($"Connection: {_client.Client.RemoteEndPoint} set to client {connections[i].GetId()}");
						return;
					}
				}

				Console.WriteLine($"Failed to connect: {_client.Client.RemoteEndPoint}, matchmaking server full");
			}
			catch (Exception _ex)
			{
				Console.WriteLine($"Error receiving TCP callback: {_ex}");
			}
		}

		public static void ServerStop()
		{
			for (int i = 0; i <= maxConnections; i++)
			{
				if (connections[i].isConnected)
					connections[i].Disconnect();
			}

			isConnected = false;

			tcpListener.Stop();
			tcpListener = null;
		}
	}
}