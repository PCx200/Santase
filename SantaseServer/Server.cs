using NetworkConnections;
using OSCTools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace networkingLayer
{
    public class Server
    {
        private TcpListener listener;
        private List<TcpNetworkConnection> connections = new();
        private List<Room> rooms = new();

        public void Start(int port)
        {
            Console.WriteLine($"Starting Santase server on port {port}...");
            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();

            CreateRoom();
        }

        public void Update()
        {
            while (true)
            {
                AcceptNewConnections();
                UpdateRooms();

                System.Threading.Thread.Sleep(10);
            }
        }

        private void AcceptNewConnections()
        {
            while (listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                TcpNetworkConnection connection = new TcpNetworkConnection(client);
                connections.Add(connection);
                Console.WriteLine($"New Connection from {connection.Remote}");

                Room currentRoom = rooms[rooms.Count - 1];

                if (currentRoom.TryAddPlayer(connection, out int playerID))
                {
                    Console.WriteLine($"Player {playerID} joined Room {currentRoom.ID}");
                    if (currentRoom.IsFull)
                    {
                        Console.WriteLine($"Room {currentRoom.ID} is full. Starting game...");
                        currentRoom.Model.StartGame();

                        CreateRoom();

                        Console.WriteLine($"Created new Room {rooms.Count - 1}");
                    }
                }
            }
        }

        private void UpdateRooms()
        {
            for (int i = 0; i < rooms.Count; i++)
            {
                try
                {
                    rooms[i].Update();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ROOM {rooms[i].ID} ERROR] {ex}");
                }
            }
        }

        private void CreateRoom()
        {
            int seed = new Random().Next(int.MinValue, int.MaxValue);
            Room room = new Room(rooms.Count, seed);
            rooms.Add(room);

            Console.WriteLine($"Created new Room {room.ID}");
        }
    }
}
