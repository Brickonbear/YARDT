using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace YARDT
{
    class FileUtils
    {

        public static void DownloadToDir(string directory)
        {
            Console.WriteLine("Begining Data Dragon download");

            DownloadFile("https://dd.b.pvp.net/datadragon-set1-en_us.zip", directory + "/datadragon-set1-en_us.zip");

            Console.WriteLine("Finished download");
        }

        public static void DownloadFile(string address, string destination)
        {
            ManualResetEvent Waiter = new ManualResetEvent(false);

            Uri uri = new Uri(address);
            WebClient wc = new WebClient();

            new Thread(() =>
            {
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(HandleDownloadProgress);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(HandleDownloadComplete);
                wc.DownloadFileAsync(uri, destination, Waiter);

            }).Start();

            Waiter.WaitOne();
        }
        

        public static void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {

            ControlUtils.ChangeMainWindowTitle("YARDT");

            Console.WriteLine();
            ManualResetEvent Waiter = e.UserState as ManualResetEvent;

            Waiter.Set();
        }

        public static void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {

            ControlUtils.ChangeMainWindowTitle("Downloading data, " + e.ProgressPercentage + "% complete...");

            Console.Write("\rDownloaded {0} of {1} bytes. {2} % complete...      ",
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }

        public static void DeleteFromDir(string directory)
        {
            DirectoryInfo di = new DirectoryInfo(directory);

            if (di.Exists)
            {
                foreach (FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
                foreach (DirectoryInfo dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
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
