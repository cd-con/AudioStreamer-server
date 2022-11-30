using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
            // TODO Добавить шифрование
            // networkKey = Encoding.ASCII.GetBytes("Chaplya4422");
            Console.WriteLine("[Web] WebServer started!");
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
                    List<string> userRequest = Read(cSocket).Split(" ").ToList();
                    //Security.Decrypt(msg, );

                    // Работаем над нашим запросом
                    Console.WriteLine(userRequest.First());
                    if (userRequest.Count == 0)
                    {
                        Console.WriteLine("[Web] Request error - no command present in request!");
                    }
                    else
                    {
                        if (userRequest.First() == "ping")
                        {
                            Console.WriteLine("[Web] Pong");
                            SendMessage(cSocket, "pong");
                        }
                        if (userRequest.First() == "get_song_by_id")
                        {
                            userRequest.RemoveAt(0);
                            SongStruct? sStruct = DataInterface.FindSongByID(int.Parse(userRequest[0]));
                            if (sStruct == null)
                            {
                                Console.WriteLine($"[Web] Request error - Song with ID {userRequest[0]} does not exists");
                                SendMessage(cSocket, "transmission_end");
                            }
                            else
                            {
                                Console.WriteLine("[Web] Processing song...");
                                try
                                {
                                    switch (userRequest[1])
                                    {
                                        case "lq":
                                            SendSongToClient(cSocket, sStruct.PathToWorstQualityDirectory);
                                            break;
                                        case "hq":
                                            SendSongToClient(cSocket, sStruct.PathToHighQualityDirectory);
                                            break;
                                        default:
                                            SendSongToClient(cSocket, sStruct.PathToMediumQualityDirectory);
                                            break;
                                    }
                                }
                                catch (ArgumentOutOfRangeException)
                                {
                                    SendSongToClient(cSocket, sStruct.PathToMediumQualityDirectory);
                                }
                               
                                Console.WriteLine("[Web] Done");
                            }
                        }

                    }


                }
                catch (SocketException sException)
                {
                    Console.WriteLine($"[Web] Client disconnected. Cause: {sException.Message}");
                    break;
                }

            }
        }
        private static void SendMessage(Socket handler, string message)
        {
            StringBuilder builder = new StringBuilder();
            byte[] data = Encoding.Unicode.GetBytes(message);
            handler.Send(data);
        }

        private static void SendRaw(Socket handler, byte[] rawData)
        {
            handler.Send(rawData);
        }

        private static string Read(Socket handler)
        {
            byte[] msg = new byte[1024];
            int size = handler.Receive(msg);
            return Encoding.Unicode.GetString(msg, 0, size);
        }

        private static void SendSongToClient(Socket cSocket, string pathToFolder)
        {
            short transmissionCode = 0; // 0 - success; 1 - transmission error

            List<string> songDirectory = Directory.GetFiles(pathToFolder).ToList();
            Console.WriteLine($"[Web] Processing {songDirectory.Count} files in folder");

            foreach (string pathToFile in songDirectory)
            {
                int packetID = songDirectory.IndexOf(pathToFile);

                using (FileStream fsSource = new FileStream(pathToFile, FileMode.Open, FileAccess.Read))
                {
                    int numBytesToRead = (int)fsSource.Length;
                    int numBytesRead = 0;
                    byte[] binContent = new byte[fsSource.Length];
                    

                    while (numBytesToRead > 0)
                    {
                        int n = fsSource.Read(binContent, numBytesRead, numBytesToRead);
                        numBytesRead += n;
                        numBytesToRead -= n;
                    }
                    numBytesToRead = binContent.Length;
                    SendRaw(cSocket, binContent);


                    Console.WriteLine("[Web] Debug. File end.");


                    // Ждём сообщение с чек-суммой
                    /*string[] checksumMessage = Read(cSocket).Split();
                    if (checksumMessage[0] == "chklength" && int.Parse(checksumMessage[1]) != numBytesRead)
                    {
                        Console.WriteLine($"[Web] Packet with ID {packetID} is invalid! Transmission stopped.");
                        transmissionCode = 1;
                        break;
                    }
                    */
                }
                
            }
            TestFileBuilder.build(pathToFolder);
            Thread.Sleep(1000);
            SendMessage(cSocket, $"transmission_end {transmissionCode}");
            GC.Collect();
        }
    }
}
