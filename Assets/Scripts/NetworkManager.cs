using UnityEngine;
using NativeWebSocket;
using System;
using System.Threading.Tasks;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance;

    private WebSocket websocket;
    public bool isConnected = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void ConnectToMatchmaking()
    {
        websocket = new WebSocket("ws://10.187.96.25:8080/ws");
        websocket.OnOpen += async () =>
        {
            Debug.Log("Connection open!");

            string joinMessage = "{\"action\":\"join\"}";
            Debug.Log("Join message sent successfully!");
            await websocket.SendText(joinMessage);
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
            isConnected = false;
            if (GameManager.Instance != null && GameManager.Instance.IsOnlineMode)
            {
                GameManager.Instance.HandleTimeout();
            }
        };
        Debug.Log("Checking for messages...");
        websocket.OnMessage += (bytes) =>
        {
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("Received message: " + message);

            HandleServerMessage(message);
        };

        await websocket.Connect();
        isConnected = true;
    }

    private void Update()
    {
    #if !UNITY_WEBGL || UNITY_EDITOR
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    #endif
    }


    private void HandleServerMessage(string message)
    {
        Debug.Log("=== RAW SERVER MESSAGE ===");
        Debug.Log(message);
        try
        {
            ServerMessage msg = JsonUtility.FromJson<ServerMessage>(message);

            if (msg.action == "move")
            {
                Debug.Log($"Opponent moved at ({msg.row}, {msg.col})");
                GameManager.Instance.ReceiveOpponentMove(msg.row, msg.col);
                GameManager.Instance.SetBoardInteractable(true);
            }
            else if (msg.action == "start")
            {
                Debug.Log("Match started! My mark is: " + msg.mark);
                GameManager.Instance.SetPlayerMark(msg.mark);
                GameManager.Instance.StartOnlineGameAfterOpponentJoins();
            }
            else if (msg.action == "timeout")
            {
                Debug.Log("Game ended due to timeout.");
                GameManager.Instance.HandleTimeout(); 
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse server message: " + e.Message);
        }
    }

    public async void SendMove(int row, int col)
    {
        if (websocket == null || websocket.State != WebSocketState.Open)
        {
            Debug.LogWarning("Cannot send move, not connected.");
            return;
        }

        string moveMessage = $"{{\"action\":\"move\", \"row\":{row}, \"col\":{col}}}";
        await websocket.SendText(moveMessage);
        Debug.Log($"Sent move: {moveMessage}");
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }

}
