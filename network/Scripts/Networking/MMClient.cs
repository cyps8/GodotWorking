using Godot;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

public class MMClient : Node
{
    private static int port;
    public static TcpClient tcpClient;
    public string ip = "127.0.0.1";
    public static int id;
    public static int bufferSize = 4096;
    private static Byte[] bytes;
    private static NetworkStream stream;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    private Packet receivedPacket;
    public static bool isConnected;
    private static bool exit = false;
    public override void _Ready()
    {
        exit = false;
    }

    public int GetID()
    {
        return id;
    }

    public override void _Process(float dt)
    {
        if(exit)
        {
            GetNode<Node>("/root/MasterScene").CallDeferred("GoToMenu");
        }
    }
    public void StartClient()  // This starts a TCP client connection to a set Matchmaking server
    {
        exit = false;

        port = 42068;

        tcpClient = new TcpClient();

        Init();

        receivedPacket = new Packet();

        isConnected = true;

        tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient);
    }

    private void ConnectCallback(IAsyncResult _result) // When result received or timed out, this function is run
    {
        if (!tcpClient.Connected)
        {
            Disconnect();
            return;
        }

        tcpClient.EndConnect(_result);

        stream = tcpClient.GetStream();

        bytes = new byte[bufferSize];

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult _result) // If a packet is received, this function is called
    {
        if(!isConnected)
        return;

        try
        {
            int byteLength = stream.EndRead(_result);
            if (byteLength <= 0)
            {
                Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(bytes, data, byteLength);
            receivedPacket.Reset(HandleData(data));

            stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error receiving TCP data: {ex}");
            Disconnect();
        }
    }

    private bool HandleData(byte[] _data)
    {
        int packetLength = 0;
        
        receivedPacket.SetBytes(_data);
        
        if (receivedPacket.UnreadLength() >= 4)
        {
            packetLength = receivedPacket.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }

        while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength())
        {
            byte[] packetBytes = receivedPacket.ReadBytes(packetLength);

            using (Packet packet = new Packet(packetBytes))
            {
                int _packetID = packet.ReadInt();
                packetHandlers[_packetID](packet);
            }

            packetLength = 0;
            if (receivedPacket.UnreadLength() >= 4)
            {
                packetLength = receivedPacket.ReadInt();
                if (packetLength <= 0)
                {
                    return true;
                }
            }
        }

        if (packetLength <= 1)
        {
            return true;
        }

        return false;
    }
    public static void SendTCP(Packet _packet)
    {
        try
        {
            if (tcpClient != null)
            {
                IAsyncResult _result = stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
                stream.EndWrite(_result);
            }
        }
        catch (Exception _ex)
        {
            GD.PrintErr($"Error sending data to server via TCP: {_ex}");
        }
    }

    private static void HandleUdpData(byte[] _data)
    {
        using (Packet packet = new Packet(_data))
        {
            int packetLength = packet.ReadInt();
            _data = packet.ReadBytes(packetLength);
        }

        using (Packet packet = new Packet(_data))
        {
            int packetID = packet.ReadInt();
            packetHandlers[packetID](packet);
        }
    }

    private void Init()
    {
        packetHandlers = new Dictionary<int, PacketHandler>() // sets each ID to a method to be run
        {
            { (int)MMServerPackets.welcome, DataManager.Handle.MMWelcome},
            { (int)MMServerPackets.gamesData, DataManager.Handle.MMGamesData},
            { (int)MMServerPackets.sendJoin, DataManager.Handle.MMSendJoin},
        };
    }
    public void Disconnect()
    {
        if (isConnected)
        {
            isConnected = false;

            stream = null;

            tcpClient.Close();
            tcpClient = null;

            GD.Print("Disconnected from matchmaking server.");

            exit = true;
        }
    }
}
