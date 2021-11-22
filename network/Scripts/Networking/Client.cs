using Godot;
using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
public class Client : Node
{
    private static int port;
    private static TcpClient client;

    public static int id;

    public static int bufferSize = 4096;

    private static Byte[] bytes;
    private static String data;

    private static NetworkStream stream;
    private static bool connected = false;

    private delegate void PacketHandler(Packet _packet);
    private static Dictionary<int, PacketHandler> packetHandlers;

    private Packet receivedPacket;

    private bool isConnected;
    public void StartClient()
    {
        port = 42069;

        client = new TcpClient();

        Init();

        receivedPacket = new Packet();

        client.BeginConnect("127.0.0.1", port, ConnectCallback, client);

        isConnected = true;
    }

    private void ConnectCallback(IAsyncResult _result)
    {
        if (!client.Connected)
        {
            Disconnect();
            return;
        }

        client.EndConnect(_result);

        stream = client.GetStream();

        connected = true;

        GetNode<Node>("/root/MasterScene/Client").Call("Connected");

        bytes = new byte[bufferSize];

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        if(!isConnected)
        {
            return;
        }

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
                GD.Print($"{_packetID}");
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
            if (client != null)
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

    public static void ConnectUDP()
    {

    }

    public static void SendUDP(Packet _packet)
    {

    }

    private void Init()
    {
        packetHandlers = new Dictionary<int, PacketHandler>()
        {
            { (int)ServerPackets.welcome, DataManager.Handle.Welcome},
            { (int)ServerPackets.playerDisconnected, DataManager.Handle.PlayerDisconnected},
            { (int)ServerPackets.chatMsg, DataManager.Handle.ServerChatMsg},
        };
    }
    private void Disconnect()
    {
        isConnected = false;

        stream = null;

        client.Close();
        client = null;

        GD.Print("Disconnected from server.");

        GetNode<Node>("/root/MasterScene").Call("GoToMenu");
    }
}
