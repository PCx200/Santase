using NetworkConnections;
using OSCTools;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class Client : MonoBehaviour
{
    public static Client Instance;
    private string serverIP = "127.0.0.1";
    private int serverPort = 50001;

    private TcpNetworkConnection connection;
    private int playerID = -1;

    private OSCDispatcher dispatcher;

    private float pingTimer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ConnectToServer(serverIP);
    }

    // Update is called once per frame
    void Update()
    {
        if (connection == null) return;

        while (connection.Available() > 0)
        {
            var packet = connection.GetPacket();
            if (packet != null)
                HandlePacket(packet);
        }

        pingTimer += Time.deltaTime;
        if (pingTimer >= 2f)
        {
            SendPing();
            pingTimer = 0f;
        }

    }
    private void OnDestroy()
    {
        SendDisconnect();
    }

    private void OnApplicationQuit()
    {
        SendDisconnect();
    }
    private void SendPing()
    {
        if (connection == null) return;

        try
        {
            var msg = new OSCMessageOut("/Ping");
            connection.Send(msg.GetBytes());
        }
        catch { }
    }


    public void SendDisconnect()
    {
        if (connection == null) return;

        try
        {
            var msg = new OSCMessageOut("/Disconnect")
                .AddInt(playerID);

            connection.Send(msg.GetBytes());
        }
        catch { }

        try { connection.Close(); } catch { }
    }

    private void HandlePacket(byte[] packet)
    {
        dispatcher.HandlePacket(packet, connection.Remote);
    }

    public void ConnectToServer(string ip)
    {
        serverIP = ip;
        Connect();
        Init();
    }

    private void Connect()
    {
        connection = new TcpNetworkConnection(serverIP, serverPort);
        Debug.Log("Connected to server");

        // Send hello handshake
        var hello = new OSCMessageOut("/HelloClient")
            .AddString("v1.0");
        connection.Send(hello.GetBytes());
    }
    private void Init()
    {
        dispatcher = new OSCDispatcher();
        dispatcher.ShowIncomingMessages = true;

        // Lobby Listeners
        dispatcher.AddListener("/RoomCreated", OnRoomCreated, OSCUtil.INT);
        dispatcher.AddListener("/RoomCreatedFailed", OnRoomCreatedFailed, OSCUtil.STRING);
        dispatcher.AddListener("/RoomJoinSuccess", OnRoomJoinSuccess, OSCUtil.INT, OSCUtil.INT);
        dispatcher.AddListener("/RoomJoinFailed", OnRoomJoinFailed, OSCUtil.STRING);

        // Gameplay Listeners
        dispatcher.AddListener("/HandChanged", OnHandChanged);
        dispatcher.AddListener("/KozChanged", OnKozChanged, OSCUtil.STRING, OSCUtil.STRING);
        dispatcher.AddListener("/ScoreChanged", OnScoreChanged, OSCUtil.INT, OSCUtil.INT);
        dispatcher.AddListener("/TurnChanged", OnTurnChanged, OSCUtil.INT);
        dispatcher.AddListener("/CardPlayed", OnCardPlayed, OSCUtil.INT, OSCUtil.STRING, OSCUtil.STRING);
        dispatcher.AddListener("/RoundOver", OnRoundOver, OSCUtil.INT, OSCUtil.INT, OSCUtil.INT);
        dispatcher.AddListener("/Notification", OnNotification, OSCUtil.STRING);
        dispatcher.AddListener("/MatchOver", OnMatchOver, OSCUtil.INT);
        dispatcher.AddListener("/TrickEnded", OnTrickEnded);
        dispatcher.AddListener("/DeckClosed", OnDeckClosed, OSCUtil.INT);
        dispatcher.AddListener("/StateChanged", OnStateChanged);
    }

    #region Lobby Phase
    public void CreateRoom(string roomName, string password)
    {
        var msg = new OSCMessageOut("/CreateRoom")
            .AddString(roomName)
            .AddString(password);

        connection.Send(msg.GetBytes());
    }

    public void JoinRoom(string roomName, string password)
    {
        var msg = new OSCMessageOut("/JoinRoom")
            .AddString(roomName)
            .AddString(password);

        connection.Send(msg.GetBytes());
    }

    private void OnRoomCreated(OSCMessageIn msg, IPEndPoint remote)
    {
        int roomID = msg.ReadInt();
        Debug.Log($"Room created with ID {roomID}");
    }

    private void OnRoomCreatedFailed(OSCMessageIn msg, IPEndPoint remote)
    {
        string reason = msg.ReadString();
        Debug.LogError("Room creation failed: " + reason);
    }

    private void OnRoomJoinSuccess(OSCMessageIn msg, IPEndPoint remote)
    {
        int roomID = msg.ReadInt();
        playerID = msg.ReadInt();

        Debug.Log($"Joined room {roomID} as Player {playerID}");

        UnityEngine.SceneManagement.SceneManager.LoadScene("GameplayScene");

        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnGameplaySceneLoaded;
    }
    private void OnGameplaySceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name != "GameplayScene") return;

        GameController.Instance.localPlayerID = playerID;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnGameplaySceneLoaded;
    }

    private void OnRoomJoinFailed(OSCMessageIn msg, IPEndPoint remote)
    {
        string reason = msg.ReadString();
        Debug.LogError("Join failed: " + reason);
    }

    #endregion

    #region Gameplay Phase
    private void OnHandChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        int playerID = msg.ReadInt();
        int count = msg.ReadInt();

        List<Card> hand = new List<Card>();

        if (playerID == this.playerID)
        {
            for (int i = 0; i < count; i++)
            {
                string name = msg.ReadString();
                string suit = msg.ReadString();
                int points = msg.ReadInt();
                hand.Add(new Card(name, suit, points));
            }
        }
        else
        {
            for (int i = 0; i < count; i++)
                hand.Add(new Card());
        }

        GameController.Instance.HandleHandChangedFromServer(playerID, hand);
    }

    private void OnKozChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        string name = msg.ReadString();
        string suit = msg.ReadString();
        GameController.Instance.HandleKozChangedFromServer(name, suit);
    }

    private void OnScoreChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        int p1 = msg.ReadInt();
        int p2 = msg.ReadInt();
        GameController.Instance.HandleScoreChangedFromServer(p1, p2);
    }

    private void OnTurnChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        int turn = msg.ReadInt();
        GameController.Instance.HandleTurnChangedFromServer(turn);
    }

    private void OnCardPlayed(OSCMessageIn msg, IPEndPoint remote)
    {
        int playerID = msg.ReadInt();
        string name = msg.ReadString();
        string suit = msg.ReadString();
        int points = msg.ReadInt();
        GameController.Instance.HandleCardPlayedFromServer(playerID, name, suit, points);
    }

    private void OnRoundOver(OSCMessageIn msg, IPEndPoint remote)
    {
        int winner = msg.ReadInt();
        int gp1 = msg.ReadInt();
        int gp2 = msg.ReadInt();
        GameController.Instance.HandleRoundOverFromServer(winner, gp1, gp2);
    }

    private void OnNotification(OSCMessageIn msg, IPEndPoint remote)
    {
        string txt = msg.ReadString();
        GameController.Instance.HandleNotificationFromServer(txt);
    }

    private void OnMatchOver(OSCMessageIn msg, IPEndPoint remote)
    {
        int winnerID = msg.ReadInt();
        GameController.Instance.HandleMatchOverFromServer(winnerID);
    }
    public void SendPlayCard(int cardIndex)
    {
        var msg = new OSCMessageOut("/PlayCard")
            .AddInt(playerID)
            .AddInt(cardIndex);

        connection.Send(msg.GetBytes());
    }

    public void SendCloseDeck()
    {
        var msg = new OSCMessageOut("/CloseDeck")
            .AddInt(playerID);

        connection.Send(msg.GetBytes());
    }

    public void SendExchangeKoz()
    {
        var msg = new OSCMessageOut("/ExchangeKoz")
            .AddInt(playerID);

        connection.Send(msg.GetBytes());
    }

    private void OnTrickEnded(OSCMessageIn msg, IPEndPoint remote)
    {
        GameController.Instance.HandleTrickEndedFromServer();
    }

    private void OnDeckClosed(OSCMessageIn msg, IPEndPoint remote)
    {
        int pid = msg.ReadInt();
        GameController.Instance.HandleDeckClosedFromServer(pid);
    }

    private void OnStateChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        GameController.Instance.HandleStateChangedFromServer();
    }
    #endregion

}
