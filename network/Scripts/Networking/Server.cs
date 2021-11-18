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
	//private static TcpClient[] client = new TcpClient[4];
    
	private List<Connection> connections = new List<Connection>();
    
	public static int bufferSize = 4096;
	private static Byte[] bytes;
	private static String data;
	private static NetworkStream[] stream = new NetworkStream[4];
	private static bool connected = false;

	private int maxPlayers;

	int counter; // temporary value

	public void ServerStart()
	{
		port = 42069;

		tcpListener = new TcpListener(IPAddress.Any, port);
		tcpListener.Start(128);

		GD.Print($"Server started on port: {port}.");

		bytes = new Byte[4096];
		data = null;

		counter = 0;

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
	}

	private void TCPConnectCallback(IAsyncResult result)
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

		

		// client[counter] = _client;
		// stream[counter] = client[counter].GetStream();
		// stream[counter].BeginRead(bytes, 0 ,bufferSize, ReceiveCallback, null);
		// connected = true;

		//counter++;
	}

	public override void _Process(float delta)
	{
		//GD.Print($"Connections: {connections.Count}");
		// if (connected == true)
		// {
		//     int i;

		//     // Loop to receive all the data sent by the client.
		//     while((i = stream.Read(bytes, 0, bytes.Length))!=0)
		//     {
		//         // Translate data bytes to a ASCII string.
		//         data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
		//         Console.WriteLine("Received: {0}", data);

		//         // Process the data sent by the client.
		//         data = data.ToUpper();

		//         GD.Print($"Received: {data}");

		//         byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

		//         // Send back a response.
		//         stream.Write(msg, 0, msg.Length);
		//         Console.WriteLine("Sent: {0}", data);
		//     }
		// }
	}

	private void ReceiveCallback(IAsyncResult _result)
	{
		try
		{
			int byteLength = stream[0].EndRead(_result);
			if (byteLength <= 0)
			{
				Disconnect();
				return;
			}

			byte[] _data = new byte[byteLength];
			Array.Copy(bytes, _data, byteLength);

			//receivedPacket.Reset(HandleData(_data));
			stream[0].BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error receiving TCP data: {ex}");
			Disconnect();
		}
	}

	public void ServerStop()
	{
		tcpListener.Stop();
	}

	private void Disconnect()
	{
		//GD.Print($"{.Client.RemoteEndPoint} has disco netted.");

		//client[0].Close();
		//udp.Disconnect();

		//Send.PlayerDisconnected(id);
	}
}
