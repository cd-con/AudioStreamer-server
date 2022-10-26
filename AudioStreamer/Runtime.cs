using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

// Сторонние библиотеки
// NAudio - работа со звуком, конвертация
// TagLib - чтение метадаты аудио

namespace AudioStreamer
{
    internal class Runtime
    {
        public static String appPath = "/";
        private static ContentFolderTasks CFT = new ContentFolderTasks();
        public static DataInterface dataInterface = new DataInterface();
        public static void Main()
        {
            appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (IsFirstStartup()) 
            {
                Console.WriteLine("Detected first startup. Creating folders... [1/2]");
                InitFolders();

                Console.WriteLine("Creating database file... [2/2]");
                InitDatabase();

                Console.WriteLine("Done!");
            }

            // Запускаем службу слежки за папкой контента
            Console.WriteLine("Starting Content Folder watchdog...");
            CFT.ContentFolderWatchdog(appPath + "/content/");

            Console.WriteLine("Starting Database...");
            dataInterface.OpenDatabase(appPath + "/cache/data.json");


            Console.WriteLine("Starting Web Server...");
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

        static void InitDatabase()
        {
            // В БД хранится путь до файлов кэша, отправляемые сервером клиентом
            //File.Create(appPath + "/cache/data.json");
        }
    }
}
