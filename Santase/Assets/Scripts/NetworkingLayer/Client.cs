using NetworkConnections;
using OSCTools;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEditor.Search;
using UnityEngine;
using static UnityEngine.CullingGroup;

public class Client : MonoBehaviour
{
    public static Client Instance;
    private string serverIP = "127.0.0.1";
    private int serverPort = 50001;

    private TcpNetworkConnection connection;
    private int playerID = -1;

    private OSCDispatcher dispatcher;

    [SerializeField] private GameController gameController;

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
        Connect();
        Init();
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
    }
    private void OnDestroy()
    {
        try 
        { 
            connection?.Close(); 
        } 
        catch { }
    }

    private void HandlePacket(byte[] packet)
    {
        dispatcher.HandlePacket(packet, connection.Remote);
    }

    private void Connect()
    {
        connection = new TcpNetworkConnection(serverIP, serverPort);
        Debug.Log("Connected to server");
    }
    private void Init()
    {
        dispatcher = new OSCDispatcher();
        dispatcher.ShowIncomingMessages = true;

        dispatcher.AddListener("/PlayerInfo", OnPlayerInfo, OSCUtil.INT);
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

    private void OnPlayerInfo(OSCMessageIn msg, IPEndPoint remote)
    {
        playerID = msg.ReadInt();
        gameController.localPlayerID = playerID;
        Debug.Log($"Assigned PlayerID: {playerID}");
    }

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

        gameController.HandleHandChangedFromServer(playerID, hand);
    }

    private void OnKozChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        string name = msg.ReadString();
        string suit = msg.ReadString();
        gameController.HandleKozChangedFromServer(name, suit);
    }

    private void OnScoreChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        int p1 = msg.ReadInt();
        int p2 = msg.ReadInt();
        gameController.HandleScoreChangedFromServer(p1, p2);
    }

    private void OnTurnChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        int turn = msg.ReadInt();
        gameController.HandleTurnChangedFromServer(turn);
    }

    private void OnCardPlayed(OSCMessageIn msg, IPEndPoint remote)
    {
        int playerID = msg.ReadInt();
        string name = msg.ReadString();
        string suit = msg.ReadString();
        int points = msg.ReadInt();
        gameController.HandleCardPlayedFromServer(playerID, name, suit, points);
    }

    private void OnRoundOver(OSCMessageIn msg, IPEndPoint remote)
    {
        int winner = msg.ReadInt();
        int gp1 = msg.ReadInt();
        int gp2 = msg.ReadInt();
        gameController.HandleRoundOverFromServer(winner, gp1, gp2);
    }

    private void OnNotification(OSCMessageIn msg, IPEndPoint remote)
    {
        string txt = msg.ReadString();
        gameController.HandleNotificationFromServer(txt);
    }

    private void OnMatchOver(OSCMessageIn msg, IPEndPoint remote)
    {
        int winnerID = msg.ReadInt();
        gameController.HandleMatchOverFromServer(winnerID);
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
        gameController.HandleTrickEndedFromServer();
    }

    private void OnDeckClosed(OSCMessageIn msg, IPEndPoint remote)
    {
        int pid = msg.ReadInt();
        gameController.HandleDeckClosedFromServer(pid);
    }

    private void OnStateChanged(OSCMessageIn msg, IPEndPoint remote)
    {
        gameController.HandleStateChangedFromServer();
    }


}
