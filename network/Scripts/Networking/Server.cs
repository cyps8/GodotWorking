using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class Server : Node
{
	private static int port;

	private static TcpListener tcpListener;
    
	public static List<Connection> connections = new List<Connection>();
	public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;

	public static int maxPlayers; // not including host


	public void ServerStart()
	{
		port = 42069;

		tcpListener = new TcpListener(IPAddress.Any, port);
		tcpListener.Start(128);

		GD.Print($"Server started on port: {port}.");

		maxPlayers = 3;

		Init();

		tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
	}

	private void Init()
	{
		for (int i = 0; i <= maxPlayers; i++)
        {
			Connection _connection = new Connection();
            connections.Insert(i, _connection);
			connections[i].SetId(i);
        }

		packetHandlers = new Dictionary<int, PacketHandler>()
		{
			{ (int)ClientPackets.welcomeReceived, DataManager.Handle.WelcomeReceived },
			{ (int)ClientPackets.chatMsg, DataManager.Handle.ClientChatMsg },
			// TODO: add packets to be handled by server.
		};
	}

	private void TCPConnectCallback(IAsyncResult result)
	{
		if (tcpListener == null)
		{
			return;
		}

		TcpClient _client = tcpListener.EndAcceptTcpClient(result);
		tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

		for (int i = 0; i < maxPlayers; i++)
		{
			if (connections[i].GetTcpClient() == null)
			{
				connections[i].ConnectTcp(_client);
				GD.Print($"Connection: {_client.Client.RemoteEndPoint} set to client {connections[i].GetId()}");
				return;
			}
		}

		GD.Print($"Failed to connect: {_client.Client.RemoteEndPoint}, Server full");

	}
	public void ServerStop()
	{
		for (int i = 0; i <= maxPlayers; i++)
        {
			if (connections[i].IsConnected())
			connections[i].Disconnect();
        }

		tcpListener.Stop();
		tcpListener = null;
	}
}
