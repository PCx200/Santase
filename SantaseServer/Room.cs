using System;
using System.Collections.Generic;
using System.Net;
using NetworkConnections;
using OSCTools;
using UnityEngine;

namespace networkingLayer
{
    public class Room
    {
        public int ID { get; }
        public GameModel Model { get; }

        public TcpNetworkConnection Player1 { get; private set; }
        public TcpNetworkConnection Player2 { get; private set; }

        public bool IsFull => Player1 != null && Player2 != null;

        private OSCDispatcher dispatcher;

        public Room(int id, int seed)
        {
            ID = id;
            Model = new GameModel(seed);

            dispatcher = new OSCDispatcher();
            dispatcher.ShowIncomingMessages = false;

            SubscribeModelEvents();
            RegisterRpcHandlers();
        }

        public bool TryAddPlayer(TcpNetworkConnection player, out int playerID)
        {
            if (Player1 == null)
            {
                Player1 = player;
                playerID = 0;
                SendPlayerInfo(player, playerID);
                return true;
            }
            if (Player2 == null)
            {
                Player2 = player;
                playerID = 1;
                SendPlayerInfo(player, playerID);
                return true;
            }

            playerID = -1;
            return false;

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
            while (connection.Available() > 0)
            {
                var packet = connection.GetPacket();
                if (packet != null)
                {
                    HandlePacket(packet, connection.Remote);
                }
            }
        }

        private void HandlePacket(byte[] packet, IPEndPoint remote)
        {
            dispatcher.HandlePacket(packet, remote);
        }
        private void SendPlayerInfo(TcpNetworkConnection player, int playerID)
        {
            var msg = new OSCMessageOut("/PlayerInfo").AddInt(playerID);
            player.Send(msg.GetBytes());
        }
        private void Broadcast(byte[] packet)
        {
            if (Player1 != null) Player1.Send(packet);
            if (Player2 != null) Player2.Send(packet);
        }

        private void SubscribeModelEvents()
        {
            Model.OnHandChanged += (playerID, hand) =>
            {
                var msg = new OSCMessageOut("/HandChanged")
                    .AddInt(playerID)
                    .AddInt(hand.Count);
                Broadcast(msg.GetBytes());
            };

            Model.OnKozChanged += (koz) =>
            {
                var msg = new OSCMessageOut("/KozChanged")
                    .AddString(koz.GetName())
                    .AddString(koz.GetSuit());
                Broadcast(msg.GetBytes());
            };

            Model.OnScoreChanged += (p1, p2) =>
            {
                var msg = new OSCMessageOut("/ScoreChanged")
                    .AddInt(p1)
                    .AddInt(p2);
                Broadcast(msg.GetBytes());
            };

            Model.OnTurnChanged += (playerID) =>
            {
                var msg = new OSCMessageOut("/TurnChanged")
                    .AddInt(playerID);
                Broadcast(msg.GetBytes());
            };

            Model.OnCardPlayed += (playerID, card) =>
            {
                var msg = new OSCMessageOut("/CardPlayed")
                    .AddInt(playerID)
                    .AddString(card.GetName())
                    .AddString(card.GetSuit());
                Broadcast(msg.GetBytes());
            };

            Model.OnTrickEnded += () =>
            {
                var msg = new OSCMessageOut("/TrickEnded");
                Broadcast(msg.GetBytes());
            };

            Model.OnRoundOver += (winnerID, gamePoints) =>
            {
                var msg = new OSCMessageOut("/RoundOver")
                    .AddInt(winnerID)
                    .AddInt(gamePoints.Item1)
                    .AddInt(gamePoints.Item2);
                Broadcast(msg.GetBytes());
            };

            Model.OnDeckClosed += (playerID) =>
            {
                var msg = new OSCMessageOut("/DeckClosed")
                    .AddInt(playerID);
                Broadcast(msg.GetBytes());
            };

            Model.OnNotification += (txt) =>
            {
                var msg = new OSCMessageOut("/Notification")
                    .AddString(txt);
                Broadcast(msg.GetBytes());
            };

            Model.OnStateChanged += () =>
            {
                var msg = new OSCMessageOut("/StateChanged");
                Broadcast(msg.GetBytes());
            };

            Model.OnMatchOver += (winnerID) =>
            {
                var msg = new OSCMessageOut("/MatchOver")
                    .AddInt(winnerID);
                Broadcast(msg.GetBytes());
            };
        }

        private void RegisterRpcHandlers()
        {
            // /PlayCard int playerID, int cardIndex
            dispatcher.AddListener("/PlayCard", (msg, remote) =>
            {
                int playerID = msg.ReadInt();
                int cardIndex = msg.ReadInt();
                Model.RequestPlayCard(playerID, cardIndex);
            }, OSCUtil.INT, OSCUtil.INT);

            // /CloseDeck int playerID
            dispatcher.AddListener("/CloseDeck", (msg, remote) =>
            {
                int playerID = msg.ReadInt();
                Model.RequestCloseDeck(playerID);
            }, OSCUtil.INT);

            // /ExchangeKoz int playerID
            dispatcher.AddListener("/ExchangeKoz", (msg, remote) =>
            {
                int playerID = msg.ReadInt();
                Model.RequestExchangeKoz(playerID);
            }, OSCUtil.INT);
        }
    }
}

