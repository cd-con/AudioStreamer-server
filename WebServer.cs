using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AudioStreamer
{
    internal class WebServer
    {
        public static int port = 8080;
        public static string ip = "0.0.0.0";
        private static byte[] networkKey;
        private static Dictionary<string, TcpClient> dictionary = new Dictionary<string, TcpClient>();

        public static void Run()
        {
            Thread sThread = new(ServerThread);

            networkKey = Encoding.ASCII.GetBytes("Chaplya4422");
            sThread.Start();
        }

        private static void ServerThread()
        {
            Socket sListener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new(IPAddress.Parse(ip), port);

            sListener.Bind(endPoint);
            sListener.Listen(100);

            Socket cSocket = default;

            int cCounter = 0;

            while (true)
            {
                cSocket = sListener.Accept();
                cCounter++;
                Thread uThread = new(new ThreadStart(() => ClientHandler(cSocket)));
                uThread.Start();
            }

        }

        private static void ClientHandler(Socket cSocket)
        {
            while (cSocket.Connected)
            {
                try
                {
                    byte[] msg = new byte[1024];
                    int size = cSocket.Receive(msg);
                    string message = Encoding.UTF8.GetString(msg, 0, size);
                    //Security.Decrypt(msg, );
                }
                catch (SocketException sException)
                {
                    Console.WriteLine($"[Web] Client disconnected. Cause: {sException.Message}");
                    break;
                }
                
            }
        }
    }
}
