using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;


namespace YARDT.Classes
{
    class Updater
    {
        private static readonly string APILatestURL = "https://api.github.com/repos/Sebski123/YARDT/releases/latest";

        private static Update LatestUpdate;
        private static Version CurrentVersion;
        private static Version LatestVersion;
        private static bool NeedsUpdate = false;
        public static string ExePath = Process.GetCurrentProcess().MainModule.FileName;

        private static string NewExe = Path.Combine(Path.GetDirectoryName(ExePath), "YARDT.exe");

        public static async Task<bool> CheckForUpdate()
        {
            var resp = await HttpClient.GetAsync(APILatestURL);
            var body = await resp.Content.ReadAsStringAsync();
            LatestUpdate = JsonSerializer.Deserialize<Update>(body);

            LatestVersion = new Version(LatestUpdate.tag_name.Substring(1));
            CurrentVersion = new Version(App.Version);

            return (LatestVersion > CurrentVersion);
        }

        public static async Task Run()
        {
            if (Path.GetFileName(ExePath).Equals("YARDT.old.exe")) RunNew();
            try
            {
                NeedsUpdate = await CheckForUpdate();
            }
            catch
            {
                Console.WriteLine("Updater:CheckFailed");
            }

            if (NeedsUpdate) await StartUpdate();
        }

        public static async Task StartUpdate()
        {
            string OldExe = Path.Combine(Path.GetDirectoryName(ExePath), "YARDT.old.exe");
            string DownloadLink = null;

            foreach (Update.Asset asset in LatestUpdate.assets)
            {
                if (asset.name == "YARDT.exe")
                {
                    DownloadLink = asset.browser_download_url;
                }
            }

            if (string.IsNullOrEmpty(DownloadLink))
            {
                Console.WriteLine("Updater:DownloadFailed");
            }
            else
            {
                if (File.Exists(OldExe))
                {
                    File.Delete(OldExe);
                }

                File.Move(ExePath, OldExe);

                await Download(DownloadLink, NewExe);
                RunNew();
            }
        }

        public static void RunNew()
        {
            Process.Start(NewExe);
            Application.Current.Dispatcher.Invoke(() => { Application.Current.Shutdown(); });
        }

        private static System.Net.Http.HttpClient _client = null;
        public static System.Net.Http.HttpClient HttpClient
        {
            get
            {
                if (_client != null) return _client;

                var handler = new System.Net.Http.HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                };

                _client = new System.Net.Http.HttpClient(handler)
                {
                    Timeout = TimeSpan.FromSeconds(30),
                };

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                _client.DefaultRequestHeaders.Add("User-Agent", "ModAssistant/" + App.Version);

                return _client;
            }
        }

        public static JavaScriptSerializer JsonSerializer = new JavaScriptSerializer()
        {
            MaxJsonLength = int.MaxValue,
        };

        public static async Task Download(string link, string output)
        {
            var resp = await HttpClient.GetAsync(link);
            using (var stream = await resp.Content.ReadAsStreamAsync())
            using (var fs = new FileStream(output, FileMode.OpenOrCreate, FileAccess.Write))
            {
                await stream.CopyToAsync(fs);
            }
        }
    }


    public class Update
    {
        public string url;
        public string assets_url;
        public string upload_url;
        public string html_url;
        public int id;
        public string node_id;
        public string tag_name;
        public string target_commitish;
        public string name;
        public bool draft;
        public User author;
        public bool prerelease;
        public string created_at;
        public string published_at;
        public Asset[] assets;
        public string tarball_url;
        public string zipball_url;
        public string body;

        public class Asset
        {
            public string url;
            public int id;
            public string node_id;
            public string name;
            public string label;
            public User uploader;
            public string content_type;
            public string state;
            public int size;
            public string created_at;
            public string updated_at;
            public string browser_download_url;
        }

        public class User
        {
            public string login;
            public int id;
            public string node_id;
            public string avatar_url;
            public string gravatar_id;
            public string url;
            public string html_url;
            public string followers_url;
            public string following_url;
            public string gists_url;
            public string starred_url;
            public string subscriptions_url;
            public string organizations_url;
            public string repos_url;
            public string events_url;
            public string received_events_url;
            public string type;
            public bool site_admin;

        }
    }
}
