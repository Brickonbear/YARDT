using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Timers;
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

            bool gameIsRunning = false;
            bool inGame = false;
            bool setLoaded = false;
            bool test = true;
            dynamic deck = new JArray();
            List<string> toDelete = new List<string>();
            dynamic set = new JArray();
            dynamic cardsInPlay = new JArray();
            dynamic cardsInPlayCopy = new JArray();
            dynamic playerCards = new JArray();
            Dictionary<int,JObject> purgatory = new Dictionary<int, JObject>();
            var overlay = new StickyOverlay();

            Timer aTimer = new Timer();
            aTimer.Interval = 2000;


            async void UpdateCardsInPlay(object source, ElapsedEventArgs e)
            {
                try
                {
                    dynamic responseString = JsonConvert.DeserializeObject(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));
                    if (responseString["GameState"].ToString() == "Menus")
                    {
                        Console.WriteLine("Not in game, stopping timer");
                        aTimer.Enabled = false;
                        inGame = false;
                    }
                    else
                    {
                        cardsInPlay = responseString.Rectangles;
                    }
                }
                catch
                {
                    Console.WriteLine("Game closed, stopping timer");
                    aTimer.Enabled = false;
                    gameIsRunning = false;
                }
            }

            aTimer.Elapsed += new ElapsedEventHandler(UpdateCardsInPlay);
            


            Console.WriteLine("Heyo fuckface its ya boi LEGIIIIIIIIIIIT FOOD REVIEWS");


            overlay.Initialize();

            overlay.Run();


            while (true)
            {
                while (!inGame || !gameIsRunning)
                {
                    try
                    {
                        dynamic responseString = JsonConvert.DeserializeObject(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));
                        gameIsRunning = true;
                        if (responseString["GameState"].ToString() == "InProgress")
                        {
                            inGame = true;
                            Console.WriteLine("Starting timer");
                            aTimer.Enabled = true;
                            deck = JsonConvert.DeserializeObject(await client.GetStringAsync($"http://localhost:{port}/static-decklist"));
                        }
                        else
                        {
                            Console.WriteLine("\nNot currently in game, stopping timer");
                            aTimer.Enabled = false;
                            inGame = false;
                        }
                    }
                    catch (HttpRequestException e)
                    {
                        Console.WriteLine("\nCould not connect to game!");
                        Console.WriteLine("Message :{0} ", e.Message);
                        gameIsRunning = false;
                    }
                }
                

                //Load set from json
                if (!setLoaded)
                {
                    set = LoadJson();
                    setLoaded = true;
                }

                if (cardsInPlay is JArray && cardsInPlay != cardsInPlayCopy)
                {
                    Console.WriteLine("Cards are diffrent");
                    cardsInPlayCopy = cardsInPlay;
                    foreach (var card in cardsInPlayCopy)
                    {
                        if (card.LocalPlayer == true)
                        {
                            if (card.CardCode != "face")
                            {
                                playerCards.Add(card);
                            }
                        }
                    }

                    foreach (var card in playerCards)
                    {
                        if (!purgatory.ContainsKey((int)card.CardID))
                        {
                            purgatory.Add((int)card.CardID, card);
                            foreach (var item in deck.CardsInDeck)
                            {
                                if (item.Name.ToString() == (string)card.CardCode && item.Value > 0)
                                {
                                    toDelete.Add(item.Name);
                                }
                            }
                        }
                    }
                    foreach (var name in toDelete)
                    {
                        deck.CardsInDeck[name] = (int)deck.CardsInDeck.GetValue(name) - 1;
                        //deck.CardsInDeck.Remove(name);
                        Console.Write("Decremented item: ");
                        Console.WriteLine(name);
                    }
                    toDelete.Clear();
                }
                if (cardsInPlay is JArray)
                {
                    if (cardsInPlay.Count > 0)
                    {
                        while (test)
                        {
                            //Console.WriteLine(cardsInPlay);
                            test = false;
                        }
                    }
                }



               /* foreach (JProperty property in deck.CardsInDeck.Properties())
                {
                    foreach (var item in set)
                    {
                        if (item.cardCode == property.Name)
                        {
                            Console.WriteLine(item.name + "\t\t" + property.Value);
                        }
                    }
                }*/
            }

        }

        public static dynamic LoadJson()
        {
            string json = Resources.set1_en_us;
            return JsonConvert.DeserializeObject(json);
        }
    }
}
