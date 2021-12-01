using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class Connection
{
    private int id;
    private NetworkStream stream;
    public TcpClient tcpClient;
    public IPEndPoint udpEndPoint;
    public static int bufferSize = 4096;
	private byte[] bytes;
    private Packet receivedPacket;
    public bool isConnected;

    public void ConnectTcp(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;

        stream = tcpClient.GetStream();

        isConnected = true;

        DataManager.Send.MMGameData();

        bytes = new byte[bufferSize];

        receivedPacket = new Packet();

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);

        DataManager.Send.Welcome(id, "Welcome to the server!");
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        if(tcpClient == null || !isConnected)
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
            receivedPacket.Reset(HandleTcpData(data));

            stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error receiving TCP data: {ex}");
            Disconnect();
        }
    }

    private bool HandleTcpData(byte[] _data)
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

                Server.packetHandlers[_packetID](id, packet);
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

    public void SendTCP(Packet _packet)
    {
        try
        {
            if (tcpClient != null)
            {
                stream.BeginWrite(_packet.ToArray(), 0, _packet.Length(), null, null);
            }
        }
        catch (Exception _ex)
        {
            GD.PrintErr($"Error sending data to server via TCP: {_ex}");
        }
    }

    public TcpClient GetTcpClient()
    {
        return tcpClient;
    }

    public void SetId(int _newId)
    {
        id = _newId;

        isConnected = false;
    }

    public int GetId()
    {
        return id;
    }

    public void ConnectUdp(IPEndPoint _endPoint)
    {
        udpEndPoint = _endPoint;
    }

    public void SendUDP(Packet _packet)
    {
        Server.SendUdpData(udpEndPoint, _packet);
    }

    public void HandleUdpData(Packet _receivedPacket)
    {
        int packetLength = _receivedPacket.ReadInt();
        byte[] packetBytes = _receivedPacket.ReadBytes(packetLength);

        using (Packet packet = new Packet(packetBytes))
        {
            int packetID = packet.ReadInt();
            Server.packetHandlers[packetID](id, packet);
        }
    }

    public void Disconnect()
    {
        udpEndPoint = null;
        isConnected = false;

        GameManager.DeletePlayer(id);
        DataManager.Send.PlayerDisconnected(id);
        DataManager.Send.MMGameData();

        tcpClient.Close();
        tcpClient = null;

        stream.Close();
        stream = null;
    }
}
