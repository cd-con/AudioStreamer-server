namespace AudioStreamer
{
    internal class ContentFolderTasks
    {
        private static AudioPipeline pipeline = new AudioPipeline();

        private bool creationDaemonStarted = false;
        private static List<string> tasks = new List<string>();
        public void ContentFolderWatchdog(string pathToContentFolder)
        {
            FileSystemWatcher watchdog = new FileSystemWatcher(pathToContentFolder);

            watchdog.Created += OnFileCreated;
            watchdog.Deleted += OnFileDeleted;

            watchdog.EnableRaisingEvents = true;
            watchdog.IncludeSubdirectories = true;
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            if (new DirectoryInfo(e.FullPath).Extension != "")
            {
                Console.WriteLine("A new one file was found in content folder!");
                if (!creationDaemonStarted)
                {
                    Thread pThread = new(ContentProcessingTread);
                    pThread.Start();
                    creationDaemonStarted = true;
                }
                tasks.Add(e.FullPath);
            }
        }

        private void ContentProcessingTread()
        {
            foreach (string fullPath in tasks)
            {
                Console.WriteLine($"[ContentProcessingThread] Processing {tasks.IndexOf(fullPath) + 1} out of {tasks.Count}");
                string compressedFolderPath = Runtime.appPath + "/cache/" + new FileInfo(fullPath).Name;

                // Делаем шакалы
                Console.WriteLine("[ContentProcessingThread] Generating audio files [1/3]");
                Directory.CreateDirectory(compressedFolderPath + "/worst/");
                pipeline.SplitFile(pipeline.WorstQuality(fullPath), 256000, compressedFolderPath + "/worst/");

                // Делаем среднячок
                Console.WriteLine("[ContentProcessingThread] Generating audio files [2/3]");
                Directory.CreateDirectory(compressedFolderPath + "/mid/");
                pipeline.SplitFile(pipeline.MidQuality(fullPath), 256000, compressedFolderPath + "/mid/");

                // Делаем суперкайф
                Console.WriteLine("[ContentProcessingThread] Generating audio files [3/3]");
                Directory.CreateDirectory(compressedFolderPath + "/hq/");
                pipeline.SplitFile(pipeline.HighQuality(fullPath), 256000, compressedFolderPath + "/hq/");

                // Читаем метадату
                TagLib.File metadata = TagLib.File.Create(fullPath.ToString());
                // Записываем в БД
                Runtime.dataInterface.AddSong(metadata.Tag.Title, metadata.Tag.Performers, compressedFolderPath + "/worst/", compressedFolderPath + "/mid/", compressedFolderPath + "/hq/");

                // Освобождаем память
                Console.WriteLine("[ContentProcessingThread] Cleaning up...");

                

                DirectoryInfo di = new DirectoryInfo(Runtime.appPath + "/cache/audio/");
                FileInfo[] files = di.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (file.Extension == ".wav")
                    {
                        file.Delete();
                    }                
                }
                Console.WriteLine("[ContentProcessingThread] Temporary files were deleted successfully");
            }
            Console.WriteLine("[ContentProcessingThread] Ended.");
            tasks.Clear();
            GC.Collect();            
            creationDaemonStarted = false;
        }

        private static void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            if (new DirectoryInfo(e.FullPath).Extension != "")
            {
                Console.WriteLine("One of the files was removed in content folder! Cleaning up...");
            }
        }
    }
}
