using Godot;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
public class Client : Node
{
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
    public void StartClient() // All tcp stuff here
    {
        port = 42069;

        tcpClient = new TcpClient();

        udpEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);

        Init();

        receivedPacket = new Packet();

        isConnected = true;

        tcpClient.BeginConnect(ip, port, ConnectCallback, tcpClient);
    }

    private void ConnectCallback(IAsyncResult _result)
    {
        if (!tcpClient.Connected)
        {
            Disconnect();
            return;
        }

        tcpClient.EndConnect(_result);

        stream = tcpClient.GetStream();

        GetNode<Node>("/root/MasterScene/Client").Call("Connected");

        bytes = new byte[bufferSize];

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult _result)
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

    public static void ConnectUDP(int _localPort) // all udp from here downwards
    {
        udpClient = new UdpClient(_localPort);

        udpClient.BeginReceive(ReceiveUdpCallback, null);

        using (Packet _packet = new Packet())
        {
            SendUDP(_packet);
        }

        udpConnected = true;
    }

    private static void ReceiveUdpCallback(IAsyncResult _result)
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

            HandleUdpData(data);
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
                udpClient.Send(_packet.ToArray(), _packet.Length(), udpEndPoint);
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
            packetHandlers[packetID](packet);
        }
    }

    private void Init()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, DataManager.Handle.Welcome},
            { (int)ServerPackets.playerDisconnected, DataManager.Handle.PlayerDisconnected},
            { (int)ServerPackets.chatMsg, DataManager.Handle.ServerChatMsg},
            { (int)ServerPackets.newPlayer, DataManager.Handle.NewPlayer},
            { (int)ServerPackets.playerMovement, DataManager.Handle.ServerMovement},
            { (int)ServerPackets.voiceChat, DataManager.Handle.ServerVoiceChat},
            { (int)ServerPackets.newBullet, DataManager.Handle.ServerNewBullet},
            { (int)ServerPackets.playerHurt, DataManager.Handle.ServerHurt},
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
