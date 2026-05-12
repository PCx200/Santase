using System;

namespace networkingLayer
{
    public class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start(50001);
            server.Update();
        }
    }

}
