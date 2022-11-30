using System.Net.Sockets;
using System.Text;

namespace AudioStreamer
{
    internal class TestFileBuilder
    {
        public static void build(string path)
        {
            List<string> songDirectory = Directory.GetFiles(path).ToList();

            using (FileStream f = File.Open("built_file.wav", FileMode.OpenOrCreate))
            {
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
                        foreach (byte bufferElement in binContent)
                        {
                            f.WriteByte(bufferElement);
                        }


                        Console.WriteLine("[File] Debug. File end.");
                    }

                }

            }
        }

    }
}
