using NAudio.Wave;
using NAudio;
using NAudio.Wave.SampleProviders;
using System.Reflection.PortableExecutable;

namespace AudioStreamer
{
    internal class AudioPipeline
    {

        public string WorstQuality(string pathToFile)
        {
            return AudioResampler(pathToFile, "_worst", 8000);
        }

        public string MidQuality(string pathToFile)
        {
            return AudioResampler(pathToFile, "_mid", 16000);
        }

        public string HighQuality(string pathToFile)
        {
            return AudioResampler(pathToFile, "_high", 24000);
        }
        private string AudioResampler(string pathToFile, string postfix, int rate)
        {
            string newFilePath = Runtime.appPath + "/cache/audio/" + Path.GetFileNameWithoutExtension(pathToFile) + postfix + ".wav";
            var source = new Pcm24BitToSampleProvider(new WaveFileReader(pathToFile));
            WdlResamplingSampleProvider resampler = new WdlResamplingSampleProvider(source, rate);
            WaveFileWriter.CreateWaveFile(newFilePath, resampler.ToWaveProvider());
            return newFilePath;
        }

        public void SplitFile(string inputFile, int chunkSize, string path)
        {
            const int BUFFER_SIZE = 20 * 1024;
            byte[] buffer = new byte[BUFFER_SIZE];

            using (Stream input = File.OpenRead(inputFile))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    using (Stream output = File.Create(path + "\\" + index))
                    {
                        int remaining = chunkSize, bytesRead;
                        while (remaining > 0 && (bytesRead = input.Read(buffer, 0,
                                Math.Min(remaining, BUFFER_SIZE))) > 0)
                        {
                            output.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }
                    }
                    index++;
                    Thread.Sleep(500); // experimental; perhaps try it
                }
            }
        }
    }
}
