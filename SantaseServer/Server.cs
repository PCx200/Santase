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

        OSCDispatcher dispatcher;

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
                UpdateConnections();

                System.Threading.Thread.Sleep(10);
            }
        }

        private void AcceptNewConnections()
        {
            while (!listener.Pending())
            {
                TcpClient client = listener.AcceptTcpClient();
                TcpNetworkConnection connection = new TcpNetworkConnection(client);
                connections.Add(connection);
                Console.WriteLine($"New Connection from {connection.Remote}");
            }
        }

        private void UpdateConnections()
        {
            for (int i = 0; i < connections.Count; i++)
            {
                while (connections[i].Available() > 0)
                {
                    HandlePacket(connections[i].GetPacket(), connections[i].Remote);
                }
            }
        }
        private void HandlePacket(byte[] packet, IPEndPoint remote)
        {
            OSCMessageIn msg = new OSCMessageIn(packet);
            Console.WriteLine($"Message arrives on server: {msg}");

            dispatcher.HandlePacket(packet, remote);
        }
    }
}
