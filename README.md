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
- 接到請求後避免阻塞為其開一個Thread，並循環監聽連接的client處理訊息。

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
由於連線請求可能不只一個，因此為每個Client開一個Thread個別處理，防止堵塞。

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

使用`stream.Read()`
