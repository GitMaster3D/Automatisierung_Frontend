using Newtonsoft.Json;

namespace Automatisierung_Frontend
{
    public class FileManager
    {
        static string PersistenFilePath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "StoredData";
            }
        }


        public static void Delete(string name, string folderName = "")
        {
            string path = PersistenFilePath + $"{(folderName != "" ? "/" + folderName : "")}/{name}";
            File.Delete(path);
        }


        public static void StoreAsJson(object obj, string name, string folderName = "")
        {
            using (FileStream fs = new FileStream(PersistenFilePath + $"{(folderName != "" ? "/" + folderName : "")}/{name}", FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(JsonConvert.SerializeObject(obj));

                writer.Close();
            }
        }


        public static string[] GetAllFilenames(string folderName)
        {
            return Directory.GetFiles(PersistenFilePath + $"/{folderName}").Select(x => x.Split("\\").Last().ToString()).ToArray();
        }


        public static object GetObject<T>(string name, string folderName = "")
        {
            try
            {
                using (FileStream fs = new FileStream(PersistenFilePath + $"{(folderName != "" ? "/" + folderName : "")}/{name}", FileMode.OpenOrCreate))
                {

                    StreamReader reader = new StreamReader(fs);
                    string json = reader.ReadToEnd();

                    return JsonConvert.DeserializeObject<T>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }
    }
}
