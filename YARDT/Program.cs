using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using YARDT.Overlay;
using YARDT.Properties;

namespace YARDT
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        public static int port = 21337;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Heyo fuckface its ya boi LEGIT FOOD REVIEWS");
            bool gameIsRunning = false;
            bool inGame = false;
            do
            {
                do
                {
                    try
                    {
                        dynamic responseString = JsonConvert.DeserializeObject(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));
                        gameIsRunning = true;
                        if (responseString["GameState"].ToString() == "InProgress")
                        {
                            inGame = true;
                        }
                        else
                        {
                            Console.WriteLine("\nNot currently in game");
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine("\nCould not connect to game!");
                        Console.WriteLine("Message :{0} ", e.Message);
                    }
                }
                while (!gameIsRunning);
            }
            while (!inGame);

            dynamic deck = JsonConvert.DeserializeObject(await client.GetStringAsync($"http://localhost:{port}/static-decklist"));

            dynamic set = LoadJson();

            foreach (JProperty property in deck.CardsInDeck.Properties())
            {
                foreach (var item in set)
                {

                    if (item.cardCode == property.Name)
                    {
                        Console.WriteLine(item.name + "\t\t" + property.Value);
                    }
                }
            }

            var overlay = new StickyOverlay();

            overlay.Initialize();

            overlay.Run();
            Console.ReadLine();
        }

        public static dynamic LoadJson()
        {
            string json = Resources.set1_en_us;
            return JsonConvert.DeserializeObject(json);
        }
    }
}
