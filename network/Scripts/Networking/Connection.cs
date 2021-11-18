using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class Connection : Node
{
    private int id;

    private NetworkStream stream;

    private TcpClient tcpClient;

    public void ConnectTcp(TcpClient _tcpClient)
    {
        tcpClient = _tcpClient;

        stream = tcpClient.GetStream();

        
    }

    public TcpClient GetTcpClient()
    {
        return tcpClient;
    }

    public void SetId(int _newId)
    {
        id = _newId;
    }

    public int GetId()
    {
        return id;
    }
}
