using NetworkConnections;
using OSCTools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace networkingLayer
{
    struct RoomInfo
    {
        public string Name;
        public string Password;
        public Room Room;
    }

    public class Server
    {
        private TcpListener listener;
        private List<TcpNetworkConnection> connections = new();

        // roomName -> RoomInfo
        private Dictionary<string, RoomInfo> rooms = new();

        // connections that are NOT in a room yet
        private HashSet<TcpNetworkConnection> lobby = new();

        // dispatcher for each lobby connection
        private Dictionary<TcpNetworkConnection, OSCDispatcher> lobbyDispatchers = new();

        public void Start(int port)
        {
            Console.WriteLine($"Starting Santase server on port {port}...");
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public void Update()
        {
            while (true)
            {
                AcceptNewConnections();
                UpdateLobby();
                UpdateRooms();

                System.Threading.Thread.Sleep(10);
            }
        }

        private void AcceptNewConnections()
        {
            while (listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                TcpNetworkConnection conn = new TcpNetworkConnection(client);

                connections.Add(conn);
                lobby.Add(conn);

                Console.WriteLine($"New Connection from {conn.Remote}");

                // Create dispatcher for this connection
                var dispatcher = new OSCDispatcher();
                dispatcher.ShowIncomingMessages = false;

                // CREATE ROOM
                dispatcher.AddListener("/CreateRoom", (msg, remote) =>
                {
                    string name = msg.ReadString();
                    string pass = msg.ReadString();

                    if (rooms.ContainsKey(name))
                    {
                        conn.Send(new OSCMessageOut("/RoomCreatedFailed")
                            .AddString("Room already exists")
                            .GetBytes());
                        return;
                    }

                    int seed = new Random().Next();
                    Room room = new Room(rooms.Count, seed);

                    rooms[name] = new RoomInfo
                    {
                        Name = name,
                        Password = pass,
                        Room = room
                    };

                    conn.Send(new OSCMessageOut("/RoomCreated")
                        .AddInt(room.ID)
                        .GetBytes());

                    Console.WriteLine($"Room '{name}' created.");
                }, OSCUtil.STRING, OSCUtil.STRING);

                // JOIN ROOM
                dispatcher.AddListener("/JoinRoom", (msg, remote) =>
                {
                    string name = msg.ReadString();
                    string pass = msg.ReadString();

                    if (!rooms.ContainsKey(name))
                    {
                        conn.Send(new OSCMessageOut("/RoomJoinFailed")
                            .AddString("Room not found")
                            .GetBytes());
                        return;
                    }

                    var info = rooms[name];

                    if (info.Password != pass)
                    {
                        conn.Send(new OSCMessageOut("/RoomJoinFailed")
                            .AddString("Wrong password")
                            .GetBytes());
                        return;
                    }

                    if (!info.Room.TryAddPlayer(conn, out int playerID))
                    {
                        conn.Send(new OSCMessageOut("/RoomJoinFailed")
                            .AddString("Room full")
                            .GetBytes());
                        return;
                    }

                    conn.Send(new OSCMessageOut("/RoomJoinSuccess")
                        .AddInt(info.Room.ID)
                        .AddInt(playerID)
                        .GetBytes());

                    Console.WriteLine($"Player {playerID} joined room '{name}'");

                    // Remove from lobby
                    lobby.Remove(conn);
                    lobbyDispatchers.Remove(conn);

                    if (info.Room.IsFull)
                    {
                        Console.WriteLine($"Room '{name}' is full. Starting game...");
                        info.Room.Model.StartGame();
                    }
                }, OSCUtil.STRING, OSCUtil.STRING);

                lobbyDispatchers[conn] = dispatcher;
            }
        }

        private void UpdateLobby()
        {
            foreach (var conn in new List<TcpNetworkConnection>(lobby))
            {
                while (conn.Available() > 0)
                {
                    var packet = conn.GetPacket();
                    if (packet != null)
                    {
                        lobbyDispatchers[conn].HandlePacket(packet, conn.Remote);
                    }
                }
            }
        }

        private void UpdateRooms()
        {
            foreach (var kvp in rooms)
            {
                kvp.Value.Room.Update();
            }
        }
    }
}
