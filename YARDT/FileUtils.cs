using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace YARDT
{
    class FileUtils
    {
        public static void DownloadToDir(string directory)
        {
            Console.WriteLine("Begining Data Dragon download");
            using (var client = new WebClient())
            {
                client.DownloadFile("https://dd.b.pvp.net/datadragon-set1-en_us.zip", directory + "/datadragon-set1-en_us.zip");
            }
            Console.WriteLine("Finished download");
        }

        public static void DeleteFromDir(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        public static JArray LoadJson(string mainDirName)
        {
            using (StreamReader r = new StreamReader(mainDirName + "set1-en_us.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<JArray>(json);
            }
        }

    }
}
