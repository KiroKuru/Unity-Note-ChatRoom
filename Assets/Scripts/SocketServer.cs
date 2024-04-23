using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
class SocketServer
{
    static List<TcpClient> clientList = new List<TcpClient>();
    static void Main()
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        int port = 8888;

        TcpListener server = new TcpListener(ipAddress, port);
        server.Start();
        Console.WriteLine("Server started. Listening for clients...");

        //等待連線
        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clientList.Add(client);
            Console.WriteLine("Client connected.");

            Thread clientThread = new Thread(HandleClient);
            clientThread.Start(client);
        }
    }

    /// <summary>
    /// 處理訊息
    /// </summary>
    /// <param name="clientObj"></param>
    static void HandleClient(object clientObj)
    {
        TcpClient client = (TcpClient)clientObj;
        NetworkStream stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead;

        while (true)
        {
            try
            {
                bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received: " + message);
                BroadcastMsg(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                break;
            }
        }

        client.Close();
        Console.WriteLine("Client disconnected.");
    }

    /// <summary>
    /// 將訊息廣播給每個client
    /// </summary>
    /// <param name="msg"></param>
    static void BroadcastMsg(string msg)
    {
        var notConnectedList = new List<TcpClient>();

        foreach (TcpClient client in clientList)
        {
            if (client.Connected)
            {
                NetworkStream clientStream = client.GetStream();
                byte[] messageBytes = Encoding.UTF8.GetBytes("Server received: " + msg);
                clientStream.Write(messageBytes, 0, messageBytes.Length);
                clientStream.Flush();
            }
            else
            {
                notConnectedList.Add(client);
            }
        }
        foreach (var notConnect in notConnectedList)
        {
            clientList.Remove(notConnect);
        }
    }
}
