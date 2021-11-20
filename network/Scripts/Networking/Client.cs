using Godot;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
public class Client : Node
{
    private static int port;
    private static TcpClient client;

    public static int bufferSize = 4096;

    private static Byte[] bytes;
    private static String data;

    private static NetworkStream stream;
    private static bool connected = false;
    public override void _Ready()
    {
        // port = 42069;

        // client = new TcpClient();

        // client.Connect("127.0.0.1", port);

        // string message = "pog! yoooooo"; 

        // bytes = Encoding.ASCII.GetBytes(message);

        // stream = client.GetStream();

        // // Send the message to the connected TcpServer.
        // stream.Write(bytes, 0, bytes.Length);

        // Console.WriteLine($"Sent: {message}");
    }

    private void ConnectCallback(IAsyncResult _result)
    {
        client.EndConnect(_result);

        if (!client.Connected)
        {
            return;
        }

        stream = client.GetStream();

        //receivedData = new Packet();

        stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
    }

    private void ReceiveCallback(IAsyncResult _result)
    {
        try
        {
            int byteLength = stream.EndRead(_result);
            if (byteLength <= 0)
            {
                Disconnect();
                return;
            }

            byte[] _data = new byte[byteLength];
            Array.Copy(bytes, _data, byteLength);

            //receivedPacket.Reset(HandleData(_data));
            //stream.BeginRead(bytes, 0, bufferSize, ReceiveCallback, null);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"Error receiving TCP data: {ex}");
            Disconnect();
        }
    }

    private void Disconnect()
    {
        client.Close();

        GD.Print("Disconnected from server.");
    }

    public override void _Process(float delta)
    {
        
    }
}
