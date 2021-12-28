using Godot;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
public class Client : Node
{
    // Variables initialised
    private static int port;
    public static TcpClient tcpClient;
    private static UdpClient udpClient;
    public static IPEndPoint udpEndPoint;
    public string ip = "127.0.0.1";
    public static int id;
    public static int bufferSize = 4096;
    private static Byte[] bytes;
    private static NetworkStream stream;
    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;
    private Packet receivedPacket;
    public static bool isConnected;
    public static bool udpConnected = false;
    private static bool exit = false;
    public override void _Ready()
    {
        exit = false;
    }

    public override void _Process(float dt)
    {
        if(exit)
        {
            GetNode<Node>("/root/MasterScene").CallDeferred("GoToMenu");
        }
    }
    public void StartClient(string _ip, int _port) // This starts a TCP client connection to a specific server
    {
        ip = _ip;

        port = _port;

        tcpClient = new TcpClient(); // TCP client initialised

        udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port); // UDP endpoint set

        Init();

        receivedPacket = new Packet();

        isConnected = true;

        tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient); // Start connect callback to attempt to connect to server
    }

    private void ConnectCallback(IAsyncResult _result) // When result received or timed out, this function is run
    {
        if (!tcpClient.Connected) // if client failed to connect, disconnect client
        {
            Disconnect();
            return;
        }

        tcpClient.EndConnect(_result);

        stream = tcpClient.GetStream(); // get stream reference from tcpclient

        GetNode<Node>("/root/MasterScene/Client").Call("Connected");

        bytes = new byte[bufferSize];

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null); // Start receive callback for receiving tcp packets from server
    }

    private void ReceiveCallback(IAsyncResult _result) // If a packet is received, this function is called
    {
        if(!isConnected) // makes sure client is connected before proceeding
        return;

        try
        {
            int byteLength = stream.EndRead(_result); // data received from stream
            if (byteLength <= 0) // makes sure there is data before proceeding
            {
                Disconnect();
                return;
            }

            byte[] data = new byte[byteLength];
            Array.Copy(bytes, data, byteLength);
            receivedPacket.Reset(HandleData(data)); // handles packet data

            stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null); // Restarts receive callback for receving next packet
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
        
        if (receivedPacket.UnreadLength() >= 4) // checks the packet has more than 1 byte of data
        {
            packetLength = receivedPacket.ReadInt();
            if (packetLength <= 0)
            {
                return true;
            }
        }

        while (packetLength > 0 && packetLength <= receivedPacket.UnreadLength()) // while packet has unread data
        {
            byte[] packetBytes = receivedPacket.ReadBytes(packetLength);

            using (Packet packet = new Packet(packetBytes))
            {
                int _packetID = packet.ReadInt(); // handles packet data depending on packet id
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
    public static void SendTCP(Packet _packet) // sends packet using tcp
    {
        try
        {
            if (tcpClient != null)
            {
                stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null); // data is sent to stream
            }
        }
        catch (Exception _ex)
        {
            GD.PrintErr($"Error sending data to server via TCP: {_ex}");
        }
    }

    public static void ConnectUDP(int _localPort) // Connects UDP client
    {
        udpClient = new UdpClient(_localPort);

        udpClient.BeginReceive(ReceiveUdpCallback, null); // Begins receiving UDP packets

        using (Packet _packet = new Packet())
        {
            SendUDP(_packet);
        }

        udpConnected = true;
    }

    private static void ReceiveUdpCallback(IAsyncResult _result) // Runs if UDP packet is received
    {
        if (!isConnected)
        return;

        try
        {
            byte[] data = udpClient.EndReceive(_result, ref udpEndPoint);
            udpClient.BeginReceive(ReceiveUdpCallback, null);

            if (data.Length < 4)
            {
                Disconnect();
                return;
            }

            HandleUdpData(data); // Handles UDP data
        }
        catch
        {
            Disconnect();
        }
    }

    public static void SendUDP(Packet _packet)
    {
        try
        {
            _packet.InsertInt(id);
            if (udpClient != null)
            {
                udpClient.Send(_packet.ToArray(), _packet.Length(), udpEndPoint); // sends UDP data to server endpoint
            }
        }
        catch (Exception _ex)
        {
            GD.Print($"Error sending data to server via UDP: {_ex}");
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
            packetHandlers[packetID](packet); // Handles UDP data depending on packet ID
        }
    }

    private void Init()
    {
        packetHandlers = new Dictionary<int, PacketHandler>() // sets each ID to a method to be run
        {
            { (int)ServerPackets.welcome, DataManager.Handle.Welcome},
            { (int)ServerPackets.playerDisconnected, DataManager.Handle.PlayerDisconnected},
            { (int)ServerPackets.chatMsg, DataManager.Handle.ServerChatMsg},
            { (int)ServerPackets.newPlayer, DataManager.Handle.NewPlayer},
            { (int)ServerPackets.playerMovement, DataManager.Handle.ServerMovement},
            { (int)ServerPackets.voiceChat, DataManager.Handle.ServerVoiceChat},
            { (int)ServerPackets.newBullet, DataManager.Handle.ServerNewBullet},
            { (int)ServerPackets.playerHurt, DataManager.Handle.ServerHurt},
            { (int)ServerPackets.playerRespawn, DataManager.Handle.ServerRespawn},
            { (int)ServerPackets.timerSync, DataManager.Handle.SyncTimer},
            { (int)ServerPackets.ping, DataManager.Handle.ServerPing},
        };
    }
    private static void Disconnect()
    {
        isConnected = false;

        stream = null;

        tcpClient.Close();
        tcpClient = null;

        udpEndPoint = null;
        if (udpConnected)
        udpClient.Close();
        udpClient = null;

        GD.Print("Disconnected from server.");

        udpConnected = false;
        exit = true;
    }
}
