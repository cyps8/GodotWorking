using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
public class Client : Node
{
    private static int port;
    private static TcpClient client;

    private static Byte[] bytes;
    private static String data;

    private static NetworkStream stream;
    private static bool connected = false;
    public override void _Ready()
    {
        port = 42069;

        client = new TcpClient();

        client.Connect("127.0.0.1", port);

        string message = "pog! yoooooo"; 

        bytes = Encoding.ASCII.GetBytes(message);

        stream = client.GetStream();

        // Send the message to the connected TcpServer.
        stream.Write(bytes, 0, bytes.Length);

        Console.WriteLine("Sent: {0}", message);
    }


}
