using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SocketClient
{
    private TcpClient socketConnection;
    private NetworkStream stream;
    private byte[] buffer = new byte[1024];
    private Queue<string> msgQueue = new Queue<string>();
    public string serverIP = "127.0.0.1";
    public int serverPort = 8888;
    public bool connected = false;

    ///<summary>
    ///連線到伺服器
    ///</summary>
    public void ConnectToServer()
    {
        try
        {
            socketConnection = new TcpClient(serverIP, serverPort);
            stream = socketConnection.GetStream();

            BeginReceived();

            connected = true;
        }
        catch (Exception error)
        {
            Debug.Log("Connection error: " + error.Message);
        }
    }

    ///<summary>
    ///開始接收訊息
    ///</summary>
    public void BeginReceived()
    {
        stream.BeginRead(buffer, 0, buffer.Length, ReceiveMessage, null);
    }

    ///<summary>
    ///接收到來自伺服器的訊息時，會調用這個方法
    ///</summary>
    private void ReceiveMessage(IAsyncResult ar)
    {
        try
        {
            int bytesRead = stream.EndRead(ar);
            string messageReceived = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            //將接收到的訊息放入佇列中
            msgQueue.Enqueue(messageReceived);

            Debug.Log("Received message: " + messageReceived);
        }
        catch (Exception error)
        {
            Debug.Log("Error receiving message: " + error.Message);
        }
    }

    ///<summary>
    ///從訊息佇列取出訊息
    ///</summary>
    public string GetMsgFromQueue()
    {
        if (msgQueue.Count > 0)
            return msgQueue.Dequeue();
        return null;
    }

    ///<summary>
    ///向伺服器發送訊息時調用
    ///</summary>
    private void SendMessageToServer(string message)
    {
        try
        {
            byte[] bytesToSend = Encoding.UTF8.GetBytes(message);
            stream.Write(bytesToSend, 0, bytesToSend.Length);
        }
        catch (Exception error)
        {
            Debug.Log("Error sending message: " + error.Message);
        }
    }

    public void SendChatMessage(string message)
    {
        SendMessageToServer(message);
    }

    ///<summary>
    ///關閉Socket
    ///</summary>
    public void CloseSocket()
    {
        Debug.Log("Close socket");
        try
        {
            if (socketConnection != null)
                socketConnection.Close();
        }
        catch (Exception error)
        {
            Debug.Log(error);
        }
        finally
        {
            socketConnection = null;
            connected = false;
        }
    }
}
