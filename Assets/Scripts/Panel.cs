using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Panel : MonoBehaviour
{
    public InputField sendMsg;
    public Button sendBtn;
    public Button loginBtn;
    public Text loginBtnTxt;
    public Text chatMsg;
    private SocketClient socketClient = new SocketClient();

    private void Start()
    {
        chatMsg.text = "";

        //登入登出
        loginBtn.onClick.AddListener(() =>
        {
            if (socketClient.connected)
            {
                socketClient.CloseSocket();
                loginBtnTxt.text = "登入";
            }
            else
            {
                socketClient.ConnectToServer();
                loginBtnTxt.text = "登出";
            }
        });

        //發送訊息
        sendBtn.onClick.AddListener(() =>
        {
            socketClient.SendChatMessage(sendMsg.text);
            sendMsg.text = "";
        });
    }

    private void Update()
    {
        if (socketClient.connected)
        {
            socketClient.BeginReceived();
        }
        var msg = socketClient.GetMsgFromQueue();
        if (!string.IsNullOrEmpty(msg))
        {
            chatMsg.SetAllDirty();
            chatMsg.text += msg + "\n";
        }
    }
}
