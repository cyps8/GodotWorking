using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class Connection
{
    private int id;

    private NetworkStream stream;

    public TcpClient tcpClient;

    public static int bufferSize = 4096;
	private byte[] bytes;
	private static String data;

    private Packet receivedPacket;
    private bool connected;

    public void ConnectTcp(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;

        stream = tcpClient.GetStream();

        connected = true;

        bytes = new byte[bufferSize];

        receivedPacket = new Packet();

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);

        DataManager.Send.Welcome(id, "Welcome to the server!");
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        if(tcpClient == null)
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

    public void SendUDP(Packet _packet)
    {

    }

    public TcpClient GetTcpClient()
    {
        return tcpClient;
    }

    public void SetId(int _newId)
    {
        id = _newId;

        connected = false;
    }

    public int GetId()
    {
        return id;
    }

    public bool IsConnected()
    {
        return connected;
    }

    public void Disconnect()
    {
        tcpClient.Close();
        tcpClient = null;
        stream = null;
        connected = false;
    }
}
