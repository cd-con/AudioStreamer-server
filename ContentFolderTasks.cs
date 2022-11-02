namespace AudioStreamer
{
    internal class ContentFolderTasks
    {
        private static AudioPipeline pipeline = new();

        private bool creationDaemonStarted = false;
        private static Queue<string> genTasks = new();
        FileSystemWatcher watchdog;
        public void ContentFolderWatchdog(string pathToContentFolder)
        {
            watchdog = new(pathToContentFolder);
            watchdog.Created += OnFileCreated;
            watchdog.Deleted += OnFileDeleted;
            watchdog.Error += new ErrorEventHandler(OnError);

            watchdog.EnableRaisingEvents = true;
            watchdog.IncludeSubdirectories = true;
        }

        private void OnError(object source, ErrorEventArgs e)
        {
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                Console.WriteLine("Error: File System Watcher internal buffer overflow at " + DateTime.Now + "\r\n");
            }
            else
            {
                Console.WriteLine("Error: Watched directory not accessible at " + DateTime.Now + "\r\n");
            }
            Console.WriteLine(e.GetException().Message);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine($"[ContentProcessingThread] creationDaemonStarted = {creationDaemonStarted}");
            if (new DirectoryInfo(e.FullPath).Extension != "")
            {
                Console.WriteLine("A new one file was found in content folder!");
                if (!creationDaemonStarted)
                {
                    Console.WriteLine("[ContentProcessingThread] Starting creation daemon...");
                    Thread pThread = new(ContentProcessingThread);
                    pThread.Start();
                    creationDaemonStarted = true;
                }
                genTasks.Enqueue(e.FullPath);
            }
        }

        private void ContentProcessingThread()
        {
            while (genTasks.Count > 0)
            {
                Console.WriteLine($"[ContentProcessingThread] Queue length = {genTasks.Count}");
                string fullPath = genTasks.Dequeue();
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

                // Чистим вилочкой
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
            creationDaemonStarted = false;
            
            GC.Collect(); // НУЖНО БОООООООЛЬШЕ ПАМЯТИ
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
