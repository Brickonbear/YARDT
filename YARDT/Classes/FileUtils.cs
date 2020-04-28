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
        /// <summary>
        /// Class for passing to DownloadFileAsync for later use
        /// </summary>
        public class userState
        {
            public ManualResetEvent waiter;
            public TextBlock title;
        }

        static int numOfSets = 2;

        /// <summary>
        /// Download file to specified location
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="windowTitle"></param>
        public static void DownloadToDir(string directory, TextBlock windowTitle)
        {
            Console.WriteLine("Begining Data Dragon download");

            for (int i = 1; i <= numOfSets; i++)
            {
                Console.WriteLine("Downloading set " + i.ToString() + " of " + numOfSets.ToString());
                DownloadFile("https://dd.b.pvp.net/latest/set" + i.ToString() + "-" + Properties.Settings.Default.Language + ".zip", directory + "/datadragon-set" + i.ToString() + "-" + Properties.Settings.Default.Language + ".zip", windowTitle);
            }

            Console.WriteLine("Finished download");
        }

        /// <summary>
        /// Download file to specified location synchronously
        /// </summary>
        /// <param name="address"></param>
        /// <param name="destination"></param>
        /// <param name="windowTitle"></param>
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
        
        /// <summary>
        /// Runs when download finishes; allows main thread to continue
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void HandleDownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            userState usr = e.UserState as userState;

            ControlUtils.ChangeMainWindowTitle(usr.title, "YARDT");

            Console.WriteLine();
            

            usr.waiter.Set();
        }

        /// <summary>
        /// Runs every time bytes have been downloaded; shows download progress in window title
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void HandleDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            userState usr = e.UserState as userState;
            ControlUtils.ChangeMainWindowTitle(usr.title, "Downloading data, " + e.ProgressPercentage + "% complete...");

            Console.Write("\rDownloaded {0} of {1} bytes. {2} % complete...      ",
                e.BytesReceived,
                e.TotalBytesToReceive,
                e.ProgressPercentage);
        }

        /// <summary>
        /// Deletes all files from specified directory
        /// </summary>
        /// <param name="directory"></param>
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

        /// <summary>
        /// Loads relevant set from json file
        /// </summary>
        /// <param name="mainDirName"></param>
        /// <returns></returns>
        public static JArray LoadJson(string mainDirName)
        {
            string json = "";
            for(int i = 1; i <= numOfSets; i++)
            {
                using (StreamReader r = new StreamReader(mainDirName + "set" + i.ToString() + "-" + Properties.Settings.Default.Language + ".json"))
                {
                    json += r.ReadToEnd();
                }
            }

            json = json.Replace("][", ",");
            System.IO.File.WriteAllText(@"C:\Users\sebth\Desktop\WriteText.txt", json);
            return JsonConvert.DeserializeObject<JArray>(json);
        }

    }
}
