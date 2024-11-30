using Newtonsoft.Json;
using System.Xml.Linq;

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
            var path = PersistenFilePath + $"{(folderName != "" ? "/" + folderName : "")}";
            Directory.CreateDirectory(path);

            using (FileStream fs = new FileStream(path + $"/{name}", FileMode.Create))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.Write(JsonConvert.SerializeObject(obj));

                writer.Close();
            }
        }


        public static string[] GetAllFilenames(string folderName)
        {
            var path = PersistenFilePath + $"/{folderName}";
            Directory.CreateDirectory(path);

            return Directory.GetFiles(PersistenFilePath + $"/{folderName}").Select(x => x.Split("\\").Last().ToString()).ToArray();
        }


        public static object GetObject<T>(string name, string folderName = "")
        {
            var path = PersistenFilePath + $"{(folderName != "" ? "/" + folderName : "")}";
            Directory.CreateDirectory(path);

            try
            {
                using (FileStream fs = new FileStream(path + $"/{name}", FileMode.OpenOrCreate))
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
