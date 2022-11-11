using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioStreamer
{
    class SongStruct
    {
        public int SongID { get; }
        public string SongName { get; }
        public string[] SongAuthors { get; }

        public string PathToWorstQualityDirectory { get; set; }

        public string PathToMediumQualityDirectory { get; set; }

        public string PathToHighQualityDirectory { get; set; }

        public SongStruct(int SongID, string SongName, string[] SongAuthors, string PathToWorstQualityDirectory, string PathToMediumQualityDirectory, string PathToHighQualityDirectory)
        {
            this.SongID = SongID;
            this.SongName = SongName;
            this.SongAuthors = SongAuthors;
            this.PathToWorstQualityDirectory = PathToWorstQualityDirectory;
            this.PathToMediumQualityDirectory = PathToMediumQualityDirectory;
            this.PathToHighQualityDirectory = PathToHighQualityDirectory;
        }
    }

    class DataStruct
    {
        public List<SongStruct>? SongStructs { get; set; }

        public DataStruct(List<SongStruct>? songStructs)
        {
            this.SongStructs = songStructs;
        }
    }


    internal class DataInterface
    {

        private static DataStruct? ds;
        private string pathToDatabase = "";
        public void OpenDatabase(string pathToBase)
        {
            try
            {
                pathToDatabase = pathToBase;
                using FileStream fs = new(pathToBase, FileMode.OpenOrCreate);
                ds = JsonSerializer.Deserialize<DataStruct>(fs);
                fs.Close();

                // Дадим пользователю знать что БД готова к работе
                Console.WriteLine("Database ready!");
            }
            catch (JsonException)
            {
                Console.WriteLine("[DataInterface] Database not found or corrupted.");
                Runtime.ExitApp();
            }
        }

        public void InitDatabase(string pathToBase)
        {
            pathToDatabase = pathToBase;
            using FileStream fs = new(pathToBase, FileMode.OpenOrCreate);
            ds = JsonSerializer.Deserialize<DataStruct>(fs);
            fs.Close();
        }

        public void AddSong(string songName, string[] songAuthors, string pathToHQ, string pathToMidQ, string pathToWorstQ)
        {
            if (ds == null || ds.SongStructs == null)
            {
                Console.WriteLine("[DataInterface] Database after initialization can't be read (null!). Retrying...");
                OpenDatabase(pathToDatabase);
            }
            if (ds.SongStructs.Count == 0)
            {
                ds.SongStructs.Add(new SongStruct(0, songName, songAuthors, pathToWorstQ, pathToMidQ, pathToHQ));
            }
            else
            {
                ds.SongStructs.Add(new SongStruct(ds.SongStructs.Last().SongID + 1, songName, songAuthors, pathToWorstQ, pathToMidQ, pathToHQ));
            }
            try
            {
                using FileStream fs = new(pathToDatabase, FileMode.OpenOrCreate);
                JsonSerializer.Serialize(fs, ds);
                fs.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[DataInterface] An error occured while saving database. Cause: {e.Message}");
            }

        }
        /*
         * Ищет песню по ID
         * 
         * Возвращает SongStruct если найдено
         * Возвращает null если не найдено
         */
        public static SongStruct? FindSongByID(int TargetSongID)
        {
            foreach (SongStruct ss in ds.SongStructs)
            {
                if (ss.SongID == TargetSongID)
                {
                    return ss;
                }
            }
            return null;
        }
    }
}
