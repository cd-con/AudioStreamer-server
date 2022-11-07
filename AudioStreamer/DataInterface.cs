using System.Text.Json;
using System.Text.Json.Serialization;

namespace AudioStreamer
{
    class SongStruct
    {
        public int id { get; }
        public string songName { get; }
        public string[] songAuthors { get; }

        public string pathToWorstQ { get; set; }

        public string pathToMidQ { get; set; }

        public string pathToHQ { get; set; }

        public SongStruct(int id, string songName, string[] songAuthors, string pathToWorstQ, string pathToMidQ, string pathToHQ)
        {
            this.id = id;
            this.songName = songName;
            this.songAuthors = songAuthors;
            this.pathToWorstQ = pathToWorstQ;
            this.pathToMidQ = pathToMidQ;
            this.pathToHQ = pathToHQ;
        }
    }

    class DataStruct
    {
        public List<SongStruct>? songStructs { get; set; }

        public DataStruct(List<SongStruct>? songStructs)
        {
            this.songStructs = songStructs;
        }
    }


    internal class DataInterface
    {

        private DataStruct? ds;
        private string pathToDatabase = "";
        public void OpenDatabase(string pathToBase)
        {
            pathToDatabase = pathToBase;
            using FileStream fs = new(pathToBase, FileMode.OpenOrCreate);
            ds = JsonSerializer.Deserialize<DataStruct>(fs);
            fs.Close();
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
            if(ds == null || ds.songStructs == null)
            {
                Console.WriteLine($"[DataInterfaceDebug] ds={ds};\nds.songStructs={ds.songStructs}\n---= If you see this, seems like an error occured and app will crash soon=---");
                Console.WriteLine("[DataInterface] Database after initialization can't be read (null!). Retrying...");
                OpenDatabase(pathToDatabase);
            }
            if (ds.songStructs.Count == 0)
            {
                ds.songStructs.Add(new SongStruct(0, songName, songAuthors, pathToWorstQ, pathToMidQ, pathToHQ));
            }
            else
            {
                ds.songStructs.Add(new SongStruct(ds.songStructs.Last().id + 1, songName, songAuthors, pathToWorstQ, pathToMidQ, pathToHQ));
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
    }
}
