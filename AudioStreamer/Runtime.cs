using System.Reflection;

// Сторонние библиотеки
// NAudio - работа со звуком, конвертация
// TagLib - чтение метадаты аудио

namespace AudioStreamer
{
    internal class Runtime
    {
        public static string? appPath = "/";
        private static ContentFolderTasks CFT = new();
        public static DataInterface dataInterface = new();
        public static void Main()
        {
            appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (appPath == null)
            {
                throw new DirectoryNotFoundException("App runtime directory cannot be found.");
            }

            if (IsFirstStartup()) 
            {
                Console.WriteLine("Detected first startup. Creating folders... [1/1]");
                InitFolders();

                Console.WriteLine("Done!");
            }

            // Запускаем службу слежки за папкой контента
            Console.WriteLine("Starting Content Folder watchdog...");
            CFT.ContentFolderWatchdog(appPath + "/content/");

            Console.WriteLine("Starting Database...");
            dataInterface.OpenDatabase(appPath + "/cache/data.json");


            Console.WriteLine("Starting Web Server...");
            WebServer.Run();
            Console.WriteLine("Ready!");


            Console.ReadKey();
        }

        static bool IsFirstStartup()
        {
            // Проверяем наличие файла БД. Просто но надёжно.
            return !File.Exists(appPath + "/cache/data.json");
        }

        static void InitFolders()
        {
            // Создаём папку для хранения контента
            Directory.CreateDirectory(appPath + "/content/");

            // Создаём папку для кеша
            Directory.CreateDirectory(appPath + "/cache/");

            // Звука...
            Directory.CreateDirectory(appPath + "/cache/audio/");
        }
        public static void ExitApp()
        {
            Environment.Exit(-1);
        }
    }
}
