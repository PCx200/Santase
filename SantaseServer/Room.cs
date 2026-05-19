using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using NetworkConnections;
using OSCTools;

namespace networkingLayer
{
    public class Room
    {
        public int ID { get; }
        public GameModel Model { get; }

        public TcpNetworkConnection? Player1 { get; private set; }
        public TcpNetworkConnection? Player2 { get; private set; }

        public bool IsFull => Player1 != null && Player2 != null;

        private OSCDispatcher dispatcher;

        public Room(int id, int seed)
        {
            ID = id;
            Model = new GameModel(seed);

            dispatcher = new OSCDispatcher();
            dispatcher.ShowIncomingMessages = false;

            try
            {
                SubscribeModelEvents();
                RegisterRpcHandlers();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} constructor: {ex}");
            }

        }

        public bool TryAddPlayer(TcpNetworkConnection player, out int playerID)
        {
            try
            {
                if (Player1 == null)
                {
                    Player1 = player;
                    playerID = 0;
                    SendPlayerInfo(player, playerID);
                    SendRoomInfo(player);
                    return true;
                }
                if (Player2 == null)
                {
                    Player2 = player;
                    playerID = 1;
                    SendPlayerInfo(player, playerID);
                    SendRoomInfo(player);
                    return true;
                }
                playerID = -1;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} TryAddPlayer: {ex}");
                playerID = -1;
                return false;
            }

        }

        public void Update()
        {
            try
            {
                if (Player1 != null)
                {
                    ProcessConnection(Player1);
                }
                if (Player2 != null)
                {
                    ProcessConnection(Player2);
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"[ROOM {ID}] Error: {ex}");
            }
        }

        private void ProcessConnection(TcpNetworkConnection connection)
        {
            try
            {
                while (connection.Available() > 0)
                {
                    var packet = connection.GetPacket();
                    if (packet == null)
                    {

                        HandleDisconnect(connection);
                        return;
                    }

                    HandlePacket(packet, connection.Remote);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} ProcessConnection {connection.Remote}: {ex}");
                HandleDisconnect(connection);
            }
        }

        public void HandleDisconnect(TcpNetworkConnection connection)
        {
            Console.WriteLine($"[INFO] Player disconnected: {connection.Remote}");

            // Remove from room
            DisconnectPlayer(connection);

            // Close socket
            try { connection.Close(); } catch { }

            try
            {
                var msg = new OSCMessageOut("/PlayerDisconnected");
                Broadcast(msg.GetBytes());
            }
            catch { }
        }
        public void DisconnectPlayer(TcpNetworkConnection conn)
        {
            if (Player1 == conn) Player1 = null;
            if (Player2 == conn) Player2 = null;
        }

        private void HandlePacket(byte[] packet, IPEndPoint remote)
        {
            try
            {
                dispatcher.HandlePacket(packet, remote);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} HandlePacket from {remote}: {ex}");
            }
        }

        private void SendPlayerInfo(TcpNetworkConnection player, int playerID)
        {
            try
            {
                var msg = new OSCMessageOut("/PlayerInfo").AddInt(playerID);
                player.Send(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} SendPlayerInfo: {ex}");
            }
        }

        private void SendRoomInfo(TcpNetworkConnection player)
        {
            try
            {
                var msg = new OSCMessageOut("/RoomInfo")
                .AddInt(ID);
                player.Send(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} SendRoomInfo: {ex}");
            }
        }

        private void Broadcast(byte[] packet)
        {
            try
            {
                if (Player1 != null) Player1.Send(packet);
                if (Player2 != null) Player2.Send(packet);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} Broadcast: {ex}");
            }
        }

        private void SubscribeModelEvents()
        {
            // server sends full card data
            Model.OnHandChanged += OnHandChanged;
            Model.OnKozChanged += OnKozChanged;
            Model.OnScoreChanged += OnScoreChanged;
            Model.OnTurnChanged += OnTurnChanged;
            Model.OnCardPlayed += OnCardPlayed;
            Model.OnTrickEnded += OnTrickEnded;
            Model.OnRoundOver += OnRoundOver;
            Model.OnDeckClosed += OnDeckClosed;
            Model.OnNotification += OnNotification;
            Model.OnStateChanged += OnStateChanged;
            Model.OnMatchOver += OnMatchOver;
        }

        private void OnHandChanged(int playerID, List<Card> hand)
        {
            try
            {
                var ownerMsg = new OSCMessageOut("/HandChanged")
                .AddInt(playerID)
                .AddInt(hand.Count);

                foreach (var card in hand)
                {
                    ownerMsg.AddString(card.GetName());
                    ownerMsg.AddString(card.GetSuit());
                    ownerMsg.AddInt(card.GetPoints());
                }

                var opponentMsg = new OSCMessageOut("/HandChanged")
                    .AddInt(playerID)
                    .AddInt(hand.Count);

                if (playerID == 0)
                {
                    Player1?.Send(ownerMsg.GetBytes());
                    Player2?.Send(opponentMsg.GetBytes());
                }
                else
                {
                    Player2?.Send(ownerMsg.GetBytes());
                    Player1?.Send(opponentMsg.GetBytes());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnHandChanged: {ex}");
            }
        }
        private void OnKozChanged(Card koz)
        {
            try
            {
                var msg = new OSCMessageOut("/KozChanged")
                .AddString(koz.GetName())
                .AddString(koz.GetSuit());

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnKozChanged: {ex}");
            }
        }
        private void OnScoreChanged(int p1, int p2)
        {
            try
            {
                var msg = new OSCMessageOut("/ScoreChanged")
                .AddInt(p1)
                .AddInt(p2);

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnScoreChanged: {ex}");
            }

        }
        private void OnTurnChanged(int playerID)
        {
            try
            {
                var msg = new OSCMessageOut("/TurnChanged")
                .AddInt(playerID);

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnTurnChanged: {ex}");
            }

        }
        private void OnCardPlayed(int playerID, Card card)
        {
            try
            {
                var msg = new OSCMessageOut("/CardPlayed")
                .AddInt(playerID)
                .AddString(card.GetName())
                .AddString(card.GetSuit());

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnCardPlayed: {ex}");
            }

        }
        private void OnTrickEnded()
        {
            try
            {
                var msg = new OSCMessageOut("/TrickEnded");
                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnTrickEnded: {ex}");
            }

        }
        private void OnRoundOver(int winnerID, (int, int) gamePoints)
        {
            try
            {
                var msg = new OSCMessageOut("/RoundOver")
                    .AddInt(winnerID)
                    .AddInt(gamePoints.Item1)
                    .AddInt(gamePoints.Item2);

                Broadcast(msg.GetBytes());

                Task.Delay(2000).ContinueWith(_ => Model.RestartRound());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnRoundOver: {ex}");
            }

        }
        private void OnDeckClosed(int playerID)
        {
            try
            {
                var msg = new OSCMessageOut("/DeckClosed")
                    .AddInt(playerID);

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnDeckClosed: {ex}");
            }

        }
        private void OnNotification(string txt)
        {
            try
            {
                var msg = new OSCMessageOut("/Notification")
                    .AddString(txt);

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnNotification: {ex}");
            }

        }
        private void OnStateChanged()
        {
            try
            {
                var msg = new OSCMessageOut("/StateChanged");

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnStateChanged: {ex}");
            }

        }
        private void OnMatchOver(int winnerID)
        {
            try
            {
                var msg = new OSCMessageOut("/MatchOver")
                    .AddInt(winnerID);

                Broadcast(msg.GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} OnMatchOver: {ex}");
            }

        }

        private void RegisterRpcHandlers()
        {
            try
            {
                // /PlayCard int playerID, int cardIndex
                dispatcher.AddListener("/PlayCard", OnPlayCard, OSCUtil.INT, OSCUtil.INT);

                // /CloseDeck int playerID
                dispatcher.AddListener("/CloseDeck", OnCloseDeck, OSCUtil.INT);

                // /ExchangeKoz int playerID
                dispatcher.AddListener("/ExchangeKoz", OnExchangeKoz, OSCUtil.INT);

                // /HelloClient string version
                dispatcher.AddListener("/HelloClient", OnHelloClient, OSCUtil.STRING);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} RegisterRpcHandlers: {ex}");
            }

        }

        private void OnPlayCard(OSCMessageIn msg, IPEndPoint remote)
        {
            try
            {
                int playerID = msg.ReadInt();
                int cardIndex = msg.ReadInt();
                Model.RequestPlayCard(playerID, cardIndex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} /PlayCard from {remote}: {ex}");
            }

        }

        private void OnCloseDeck(OSCMessageIn msg, IPEndPoint remote)
        {
            try
            {
                int playerID = msg.ReadInt();
                Model.RequestCloseDeck(playerID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} /CloseDeck from {remote}: {ex}");
            }

        }

        private void OnExchangeKoz(OSCMessageIn msg, IPEndPoint remote)
        {
            try
            {
                int playerID = msg.ReadInt();
                Model.RequestExchangeKoz(playerID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} /ExchangeKoz from {remote}: {ex}");
            }

        }
        private void OnHelloClient(OSCMessageIn msg, IPEndPoint remote)
        {
            try
            {
                string version = msg.ReadString();
                Console.WriteLine($"Client {remote} connected with version {version}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Room {ID} /HelloClient from {remote}: {ex}");
            }

        }
    }
}

