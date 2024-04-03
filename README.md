# SocketChatRoom

之前一直都是做單機的作品，沒怎麼接觸過連線相關的東西，想試著弄個簡易聊天室邊做邊學，並作筆記方便日後回顧。

## 為什麼用WebSocket

這邊就得先提WebSocket的幾個特點

- 雙向通訊：WebSocket可以讓client與server間相互通信，這表示client可以向server發送訊息，而server也能向client發送資料，藉此達成「雙向通信」。
- 持久連接：WebSocket在經過一次握手建立連結後保持打開狀態（keep-alive），不需在每次通訊重新建立連結，進而減少了延遲。

![](https://i.imgur.com/S3Mhxau.png)

因此可以使用WebSocket來實作如：在線聊天室、共同編輯、股票交易等，實時性要求高的應用，而不需使用輪詢（Polling）等技術。
