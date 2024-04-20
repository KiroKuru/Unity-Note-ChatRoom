# SocketChatRoom

之前一直都是做單機的作品，沒怎麼接觸過連線相關的東西，想試著弄個簡易聊天室邊做邊學，並作筆記方便日後回顧。

## 為什麼用WebSocket

這邊就得先提WebSocket的幾個特點

- 雙向通訊：WebSocket可以讓client與server間相互通信，這表示client可以向server發送訊息，而server也能向client發送資料，藉此達成「雙向通信」。
- 持久連接：WebSocket在經過一次握手建立連結後保持打開狀態（keep-alive），不需在每次通訊重新建立連結，進而減少了延遲。

![](https://i.imgur.com/S3Mhxau.png)

因此可以使用WebSocket來實作如：在線聊天室、共同編輯、股票交易等，實時性要求高的應用，而不需使用輪詢（Polling）等技術。

## 如何開始WebSocket

主要會分成Server與Client兩邊進行。

#### *Server*

- 創建一個Socket並綁定Server的IP與Port。
- 開始監聽有無client請求連線。
- 接到請求後為其開一個Thread，並循環監聽連接的client處理訊息。

#### *Client*

- 設置一個要連過去Server的Socket並綁好Server的IP與Port。
- 向伺服器發起連線請求。
- 連接後開始處理要發送與接收的訊息。
- 設置中斷連接的方法。

## Server實作

```c#
IPAddress ipAddress = IPAddress.Parse("Your IP");
int port = 8888; //Your port

TcpListener server = new TcpListener(ipAddress, port);
server.Start();
Console.WriteLine("Server started. Listening for clients...");
```

首先指定好Server的IP與監聽的Port，再創建監聽的Socket`TcpListener`並綁定IP與Port，然後開始監聽等待連線。

```c#
while (true)
{
    TcpClient client = server.AcceptTcpClient();
    clientList.Add(client);
    Console.WriteLine("Client connected.");

    Thread clientThread = new Thread(HandleClient);
    clientThread.Start(client);
}
```

當監聽到Client的連線請求後，接受`AcceptTcpClient()`並放入`clientList`連接池中。
由於連線請求可能不只一個，因此為每個Client開一個Thread個別處理。

```c#
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
```

下一步要來處理訊息，建立連接後可以使用`TcpClient`的`GetStream()`方法取得與客戶端通訊的`NetworkStream`。

使用`stream.Read()`從客戶端接收資料，並將資料讀入`buffer`中。接著使用`Encoding.UTF8.GetString()`方法將`buffer`中的資料解碼為UTF-8字串。

由於聊天室內會同時有多個客戶端，需要再呼叫`BroadcastMsg`方法將訊息廣播給其他客戶端，這樣訊息才會同步顯示在每個人的聊天室裡。

```c#
static void BroadcastMsg(string msg)
{
    var notConnectedList = newList<TcpClient>();

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
```

最後一部分是把訊息廣播給所有已連接的客戶端的方法。

首先遍歷`clientList`將已連接與未連接的客戶端分開處理，檢查客戶端的連接狀態`client.Connected`，如果仍然連接，則取得客戶端的`NetworkStream`，將要廣播的訊息`msg`轉換成字節陣列`messageBytes`，然後使用`clientStream.Write()`方法將訊息發送給客戶端。

最後使用`clientStream.Flush()`來確保所有資料都被立即發送。

而未連接的客戶端，創建一個List`notConnectedList`來暫時存儲，最後遍歷一次List將其從`clientList`內移除。

## Client實作

```c#
private TcpClient socketConnection;
private NetworkStream stream;
private byte[] buffer=new byte[1024];
private Queue<string> msgQueue=new Queue<string>();
public string serverIP="Your IP";
public int serverPort=8888 //Your port;
public bool connected=false;
```

首先定義成員變數和屬性：

- `TcpClient socketConnection`：用於代表客戶端的 TcpClient 連接。
- `NetworkStream stream`：用於從`TcpClient`中獲取資料流，進行訊息的讀取和寫入。
- `byte[] buffer`：用於暫存從伺服器接收到的資料。
- `Queue<string> msgQueue`：用於存儲從伺服器接收到的訊息。
- `string serverIP`：伺服器的IP地址。
- `int serverPort`：伺服器的Port。
- `bool connected`：表示客戶端是否已連接到伺服器。

```c#
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
```

`ConnectToServer()`是連接到伺服器的方法，與伺服器很像一開始先建立`TcpClient`並連接到指定的伺服器IP和端口，並獲取 NetworkStream 以進行資料讀取和寫入。

然後使用`BeginRead()`方法開始從伺服器接收資料，並指定當有資料可讀取時要調用的回調方法`ReceiveMessage`。

設置`connected`為true，表示客戶端已連接成功。

```c#
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
```

當從伺服器接收到資料時調用`ReceiveMessage()`，讀取從`NetworkStream`中接收到的資料，轉換成字串。

將接收到的訊息放入`msgQueue`佇列中。

```c#
public string GetMsgFromQueue()
{
    if (msgQueue.Count > 0)
        return msgQueue.Dequeue();
    return null;
}
```

從`msgQueue`佇列中取出一條訊息，如果佇列不為空則返回訊息，否則返回null。

```c#
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
```

將要發送的訊息轉換成字節陣列，然後使用`NetworkStream`的`Write()`方法向伺服器發送資料。

```c#
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
```

關閉客戶端的`TcpClient`連接，並將`connected`設置為false。

至此一個簡易的聊天室便搭建完成了，最後只需要在Unity內簡單弄個聊天室UI，並將Func綁上去就完成了。


