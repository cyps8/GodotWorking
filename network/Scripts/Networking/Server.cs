using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Server : Node
{
	private static int port;
	private static TcpListener tcpListener;
	private static UdpClient udpListener;
	public static List<Connection> connections = new List<Connection>();
	public delegate void PacketHandler(int _fromClient, Packet _packet);
    public static Dictionary<int, PacketHandler> packetHandlers;
	public static int maxPlayers = 7; // not including host
	public static string gameName = "My Server";
	static bool isConnected;
	public void ServerStart()
	{
		port = 42069;

		tcpListener = new TcpListener(IPAddress.Any, port);
		tcpListener.Start(128);

		GD.Print($"Server started on port: {port}.");

		Init();
		
		isConnected = true;

		DataManager.Send.MMNewGame();

		tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

		udpListener = new UdpClient(port);

		udpListener.BeginReceive(UDPReceiveCallback, null);
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
			{ (int)ClientPackets.playerMovement, DataManager.Handle.ClientMovement },
			{ (int)ClientPackets.voiceChat, DataManager.Handle.ClientVoiceChat },
			{ (int)ClientPackets.newBullet, DataManager.Handle.ClientNewBullet },
			{ (int)ClientPackets.playerHurt, DataManager.Handle.ClientHurt },
		};
	}

	private void TCPConnectCallback(IAsyncResult result)
	{
		if (tcpListener == null || isConnected == false)
		return;

		try
		{
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
        catch (Exception _ex)
        {
            GD.Print($"Error receiving TCP callback: {_ex}");
        }
	}

	private static void UDPReceiveCallback(IAsyncResult _result)
    {
		if (udpListener == null || isConnected == false)
		return;

        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 42069);
            byte[] data = udpListener.EndReceive(_result, ref clientEndPoint);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            if (data.Length < 4)
            {
                return;
            }

            using (Packet _packet = new Packet(data))
            {
				int clientID = _packet.ReadInt();

                if (clientID == -1)
                {
                    return;
                }

                if (connections[clientID].udpEndPoint == null)
                {
                    connections[clientID].ConnectUdp(clientEndPoint);
                    return;
                }

                if (connections[clientID].udpEndPoint.ToString() == clientEndPoint.ToString())
                {
                    connections[clientID].HandleUdpData(_packet);
                }
            }
        }
        catch (Exception _ex)
        {
            GD.Print($"Error receiving UDP data: {_ex}");
			for (int i = 0; i < connections.Count; i++)
			{
				if (!connections[i].isConnected)
				connections[i].udpEndPoint = null;
			}
			udpListener.BeginReceive(UDPReceiveCallback, null);
        }
    }

	public static void SendUdpData(IPEndPoint _clientEndPoint, Packet _packet)
    {
		if (udpListener == null || isConnected == false)
		return;

        try
        {
            if (_clientEndPoint != null)
            {
                udpListener.BeginSend(_packet.ToArray(), _packet.Length(), _clientEndPoint, null, null);
            }
        }
        catch (Exception _ex)
        {
            GD.Print($"Error sending data to {_clientEndPoint} via UDP: {_ex}");
        }
    }

	public void ServerStop()
	{
		for (int i = 0; i <= maxPlayers; i++)
        {
			if (connections[i].isConnected)
			connections[i].Disconnect();
        }

		DataManager.Send.MMGameClosed();

		SceneManager.mmClient.Disconnect();

		isConnected = false;

		tcpListener.Stop();
		tcpListener = null;

		udpListener.Close();
		udpListener = null;
	}
}
