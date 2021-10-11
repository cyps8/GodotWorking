using Godot;
using System;
using System.Net;
using System.Net.Sockets;

public class Server : Node
{
    private static int port;

    private static TcpListener tcpListener;

    private static TcpClient client;

    private static Byte[] bytes;
    private static String data;
    private static NetworkStream stream;
    private static bool connected = false;

    public void ServerStart()
    {
        port = 42069;

        tcpListener = new TcpListener(IPAddress.Any, port);
        tcpListener.Start();

        GD.Print($"Server started on port: {port}.");

        //tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        
    }

    private void Connect()
    {
        client = tcpListener.AcceptTcpClient();

        GD.Print($"Connect: {client.Client.RemoteEndPoint}");

        stream = client.GetStream();
        bytes = new Byte[256];
        data = null;

        connected = true;
    }

    public override void _Process(float delta)
    {
        if (connected == true)
        {
            int i;

            // Loop to receive all the data sent by the client.
            while((i = stream.Read(bytes, 0, bytes.Length))!=0)
            {
                // Translate data bytes to a ASCII string.
                data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                Console.WriteLine("Received: {0}", data);

                // Process the data sent by the client.
                data = data.ToUpper();

                GD.Print($"Received: {data}");

                byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                // Send back a response.
                stream.Write(msg, 0, msg.Length);
                Console.WriteLine("Sent: {0}", data);
            }
        }
    }

    // private static void TCPConnectCallback(IAsyncResult result)
    // {
    //     client = tcpListener.EndAcceptTcpClient(result);
    //     tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
    //     GD.Print($"Connect: {client.Client.RemoteEndPoint}");
    // }

    public void ServerStop()
    {
        
        tcpListener.Stop();
        //client.Close();
    }
}