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
            bool mulligan = true;
            JObject deck = new JObject();
            List<string> toDelete = new List<string>();
            JArray set = new JArray();
            JArray cardsInPlay = new JArray();
            JArray cardsInPlayCopy = new JArray();
            Dictionary<string, JObject> playerCards = new Dictionary<string, JObject>();
            Dictionary<string, JObject> purgatory = new Dictionary<string, JObject>();
            var overlay = new StickyOverlay();

            Timer aTimer = new Timer();
            aTimer.Interval = 2000;


            async void UpdateCardsInPlay(object source, ElapsedEventArgs e)
            {
                try
                {
                    JObject responseString = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));
                    if (responseString["GameState"].ToString() == "Menus")
                    {

                        toDelete.Clear();
                        deck = new JObject();
                        cardsInPlay.Clear();
                        cardsInPlayCopy.Clear();
                        playerCards.Clear();
                        Console.WriteLine("Not in game, stopping timer");
                        aTimer.Enabled = false;
                        inGame = false;
                    }
                    else
                    {
                        cardsInPlay = responseString["Rectangles"].ToObject<JArray>();
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
                        JObject responseString = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));

                        gameIsRunning = true;
                        if (responseString["GameState"].ToString() == "InProgress")
                        {
                            inGame = true;
                            Console.WriteLine("Starting timer");
                            aTimer.Enabled = true;
                            deck = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync($"http://localhost:{port}/static-decklist"));
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
                    //Console.WriteLine("Cards are diffrent");
                    cardsInPlayCopy = cardsInPlay;
                    foreach (var card in cardsInPlayCopy)
                    {
                        if (!playerCards.ContainsKey(card.Value<string>("CardID")))
                        {
                            if (card.Value<bool>("LocalPlayer") == true)
                            {
                                if (card.Value<string>("CardCode") != "face")
                                {
                                    playerCards.Add(card.Value<string>("CardID"), card.ToObject<JObject>());
                                }
                            }
                        }
                    }

                    if (mulligan && playerCards.Count > 4)
                    {
                        playerCards.Clear();
                        mulligan = false;
                    }

                    if (!mulligan)
                    {
                        foreach (var card in playerCards.Keys)
                        {
                            if (!purgatory.ContainsKey(card))
                            {
                                purgatory.Add(card, playerCards[card]);
                                foreach (var item in deck["CardsInDeck"])
                                {
                                    JProperty itemProperty = item.ToObject<JProperty>();

                                    if (itemProperty.Name == (string)playerCards[card]["CardCode"] && (int)itemProperty.Value > 0)
                                    {
                                        toDelete.Add(itemProperty.Name);
                                        break;
                                    }
                                }
                            }
                        }
                        if (toDelete.Count > 0)
                        {
                            foreach (var name in toDelete)
                            {
                                deck["CardsInDeck"][name] = deck["CardsInDeck"].Value<int>(name) - 1;
                                //deck.CardsInDeck.Remove(name);
                                Console.Write("Decremented item: ");
                                Console.WriteLine(name);
                            }
                            toDelete.Clear();
                            printDeckList(deck, set);
                        }
                    }
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
            }

        }

        public static JArray LoadJson()
        {
            string json = Resources.set1_en_us;
            return JsonConvert.DeserializeObject<JArray>(json);
        }

        public static void printDeckList(JObject deck, JArray set)
        {
            foreach (JToken card in deck["CardsInDeck"])
            {
                JProperty cardProperty = card.ToObject<JProperty>();

                foreach (var item in set)
                {

                    if (item.Value<string>("cardCode") == cardProperty.Name)
                    {
                        Console.WriteLine(string.Format("{0,-25}{1}", item.Value<string>("name"), cardProperty.Value));
                        break;
                    }
                }
            }
        }
    }
}
