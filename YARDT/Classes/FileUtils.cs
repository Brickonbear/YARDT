using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Controls;

namespace YARDT
{
    class FileUtils
    {
        public class userState
        {
            public ManualResetEvent waiter;
            public TextBlock title;
        }

        public static void DownloadToDir(string directory, TextBlock windowTitle)
        {
            Console.WriteLine("Begining Data Dragon download");

            DownloadFile("https://dd.b.pvp.net/latest/set1-" + Properties.Settings.Default.Language + ".zip", directory + "/datadragon-set1-" + Properties.Settings.Default.Language + ".zip", windowTitle);

            Console.WriteLine("Finished download");
        }

        public static void DownloadFile(string address, string destination, TextBlock windowTitle)
        {

            userState usr = new userState();
            usr.waiter = new ManualResetEvent(false);
            usr.title = windowTitle;

            Uri uri = new Uri(address);
            WebClient wc = new WebClient();

            new Thread(() =>
            {
                wc.DownloadProgressChanged += new DownloadProgressChangedEventHandler(HandleDownloadProgress);
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(HandleDownloadComplete);
                wc.DownloadFileAsync(uri, destination, usr);

            }).Start();

            usr.waiter.WaitOne();
        }
        

        public static void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            userState usr = e.UserState as userState;

            ControlUtils.ChangeMainWindowTitle(usr.title, "YARDT");

            Console.WriteLine();
            

            usr.waiter.Set();
        }

        public static void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            userState usr = e.UserState as userState;
            ControlUtils.ChangeMainWindowTitle(usr.title, "Downloading data, " + e.ProgressPercentage + "% complete...");

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
            using (StreamReader r = new StreamReader(mainDirName + "set1-" + Properties.Settings.Default.Language + ".json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<JArray>(json);
            }
        }

    }
}
