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
        private TcpListener? listener;
        private List<TcpNetworkConnection> connections = new();

        // roomName -> RoomInfo
        private Dictionary<string, RoomInfo> rooms = new();

        // connections that are NOT in a room yet
        private HashSet<TcpNetworkConnection> lobby = new();

        // dispatcher for each lobby connection
        private Dictionary<TcpNetworkConnection, OSCDispatcher> lobbyDispatchers = new();

        public void Start(int port)
        {
            try
            {
                Console.WriteLine($"Starting Santase server on port {port}...");
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Server.Start: {ex}");
            }
        }

        public void Update()
        {
            while (true)
            {
                try
                {
                    AcceptNewConnections();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Server.Update -> AcceptNewConnections: {ex}");
                }

                try
                {
                    UpdateLobby();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Server.Update -> UpdateLobby: {ex}");
                }

                try
                {
                    UpdateRooms();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Server.Update -> UpdateRooms: {ex}");
                }

                System.Threading.Thread.Sleep(10);
            }
        }

        private void AcceptNewConnections()
        {
            try
            {
                while (listener!.Pending())
                {
                    TcpClient client = listener.AcceptTcpClient();
                    TcpNetworkConnection conn = new TcpNetworkConnection(client);

                    connections.Add(conn);
                    lobby.Add(conn);

                    Console.WriteLine($"New Connection from {conn.Remote}");

                    // Create dispatcher for this connection
                    var dispatcher = new OSCDispatcher();
                    dispatcher.ShowIncomingMessages = false;

                    try
                    {
                        dispatcher.AddListener("/CreateRoom", (msg, remote) => OnCreateRoom(conn, msg), OSCUtil.STRING, OSCUtil.STRING);

                        dispatcher.AddListener("/JoinRoom", (msg, remote) => OnJoinRoom(conn, msg), OSCUtil.STRING, OSCUtil.STRING);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] Registering lobby listeners for {conn.Remote}: {ex}");
                    }

                    lobbyDispatchers[conn] = dispatcher;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] AcceptNewConnections: {ex}");
            }
            
        }

        private void OnCreateRoom(TcpNetworkConnection conn, OSCMessageIn msg)
        {
            try
            {
                string name = msg.ReadString();
                string pass = msg.ReadString();

                if (rooms.ContainsKey(name))
                {
                    SendRoomCreatedFailed(conn, "Room already exists");
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

                room.TryAddPlayer(conn, out int playerID);

                SendRoomCreated(conn, room.ID);

                SendRoomJoinSuccess(conn, room.ID, playerID);

                RemoveFromLobby(conn);

                Console.WriteLine($"Room '{name}' created.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] /CreateRoom from {conn.Remote}: {ex}");
            }
        }

        private void OnJoinRoom(TcpNetworkConnection conn, OSCMessageIn msg)
        {
            try
            {
                string name = msg.ReadString();
                string pass = msg.ReadString();

                if (!rooms.ContainsKey(name))
                {
                    SendRoomJoinFailed(conn, "Room not found");
                    return;
                }

                var info = rooms[name];

                if (info.Password != pass)
                {
                    SendRoomJoinFailed(conn, "Wrong password");
                    return;
                }

                if (!info.Room.TryAddPlayer(conn, out int playerID))
                {
                    SendRoomJoinFailed(conn, "Room full");
                    return;
                }

                SendRoomJoinSuccess(conn, info.Room.ID, playerID);

                Console.WriteLine($"Player {playerID} joined room '{name}'");

                RemoveFromLobby(conn);

                if (info.Room.IsFull)
                {
                    Console.WriteLine($"Room '{name}' is full. Starting game...");
                    info.Room.Model.StartGame();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] /JoinRoom from {conn.Remote}: {ex}");
            }
        }

        private void SendRoomCreatedFailed(TcpNetworkConnection conn, string reason)
        {
            try
            {
                conn.Send(new OSCMessageOut("/RoomCreatedFailed")
                    .AddString(reason)
                    .GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendRoomCreatedFailed to {conn.Remote}: {ex}");
            }
        }

        private void SendRoomJoinFailed(TcpNetworkConnection conn, string reason)
        {
            try
            {
                conn.Send(new OSCMessageOut("/RoomJoinFailed")
                    .AddString(reason)
                    .GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendRoomJoinFailed to {conn.Remote}: {ex}");
            }
        }

        private void SendRoomCreated(TcpNetworkConnection conn, int roomID)
        {
            try
            {
                conn.Send(new OSCMessageOut("/RoomCreated")
                    .AddInt(roomID)
                    .GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendRoomCreated to {conn.Remote}: {ex}");
            }
        }

        private void SendRoomJoinSuccess(TcpNetworkConnection conn, int roomID, int playerID)
        {
            try
            {
                conn.Send(new OSCMessageOut("/RoomJoinSuccess")
                    .AddInt(roomID)
                    .AddInt(playerID)
                    .GetBytes());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] SendRoomJoinSuccess to {conn.Remote}: {ex}");
            }
        }

        private void RemoveFromLobby(TcpNetworkConnection conn)
        {
            try
            {
                lobby.Remove(conn);
                lobbyDispatchers.Remove(conn);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] RemoveFromLobby {conn.Remote}: {ex}");
            }
        }

        private void UpdateLobby()
        {
            try
            {
                foreach (var conn in new List<TcpNetworkConnection>(lobby))
                {
                    try
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] UpdateLobby connection {conn.Remote}: {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateLobby outer: {ex}");
            }
        }

        private void UpdateRooms()
        {
            try
            {
                foreach (var kvp in rooms)
                {
                    try
                    {
                        kvp.Value.Room.Update();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[ERROR] UpdateRooms room '{kvp.Key}': {ex}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] UpdateRooms outer: {ex}");
            }
        }
    }
}
