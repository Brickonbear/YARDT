using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YARDT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        public static int port = 21338;

        bool gameIsRunning = false;
        bool inGame = false;
        bool setLoaded = false;
        bool gotDeck = false;
        bool direction = false;
        bool sorted = false;
        bool mulligan = true;
        bool isMinimized = false;
        double prevHeight = 0;
        JObject deck = new JObject();
        List<string> toDelete = new List<string>();
        List<string> manaCostOrder = new List<string>();
        JArray set = new JArray();
        JArray cardsInPlay = new JArray();
        JArray cardsInPlayCopy = new JArray();
        Dictionary<string, JObject> playerCards = new Dictionary<string, JObject>();
        Dictionary<string, JObject> purgatory = new Dictionary<string, JObject>();



        DispatcherTimer aTimer = new DispatcherTimer();
        DispatcherTimer bTimer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();

            aTimer.Interval = TimeSpan.FromMilliseconds(2000);
            bTimer.Interval = TimeSpan.FromMilliseconds(1000);

            async void UpdateCardsInPlay(object source, EventArgs e)
            {
                try
                {
                    JObject responseString = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync($"http://localhost:{port}/positional-rectangles"));
                    if (responseString["GameState"].ToString() == "Menus")
                    {

                        ResetVars();
                        Console.WriteLine("Not in game, stopping timer");
                    }
                    else
                    {
                        cardsInPlay = responseString["Rectangles"].ToObject<JArray>();
                    }
                }
                catch
                {
                    Console.WriteLine("Game closed, stopping timer");
                    aTimer.IsEnabled = false;
                    gameIsRunning = false;
                }
            }
            void runChecks(object source, EventArgs e)
            {

                while (!inGame || !gameIsRunning)
                {
                    try
                    {
                        JObject responseString = JsonConvert.DeserializeObject<JObject>(httpReq($"http://localhost:{port}/positional-rectangles"));

                        Console.WriteLine(responseString);

                        gameIsRunning = true;
                        if (responseString["GameState"].ToString() == "InProgress")
                        {
                            inGame = true;
                            Console.WriteLine("Starting timer");
                            aTimer.IsEnabled = true;
                            if (!gotDeck)
                            {
                                gotDeck = true;
                                deck = JsonConvert.DeserializeObject<JObject>(httpReq($"http://localhost:{port}/static-decklist"));
                                manaCostOrder.Clear();
                                foreach (JToken card in deck["CardsInDeck"])
                                {
                                    JProperty cardProperty = card.ToObject<JProperty>();
                                    manaCostOrder.Add(cardProperty.Name);
                                }
                                sorted = false;
                                Console.WriteLine("Got deck");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nNot currently in game, stopping timer");
                            aTimer.IsEnabled = false;
                            inGame = false;
                        }
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine("\nCould not connect to game!");
                        Console.WriteLine("Message :{0} ", err.Message);
                        gameIsRunning = false;
                    }
                }

                //Load set from json
                if (!setLoaded)
                {
                    set = LoadJson();
                    setLoaded = true;
                    Console.WriteLine("Loaded set");
                }

                if (!sorted && setLoaded)
                {
                    Console.WriteLine("Sorting deck");
                    manaCostOrder.Sort((x, y) =>
                    {
                        int xManaCost = -1, yManaCost = -1;
                        foreach (var item in set)
                        {
                            if (item.Value<string>("cardCode") == x)
                            {
                                xManaCost = item.Value<int>("cost");
                            }
                            else if (item.Value<string>("cardCode") == y)
                            {
                                yManaCost = item.Value<int>("cost");
                            }
                            if (xManaCost >= 0 && yManaCost >= 0) break;

                        }
                        return xManaCost.CompareTo(yManaCost);
                    });

                    sorted = true;
                    Console.WriteLine("Sorted deck");
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
                                    Console.WriteLine("Adding card: " + card.Value<string>("CardID") + "to playerCards");
                                    playerCards.Add(card.Value<string>("CardID"), card.ToObject<JObject>());
                                }
                            }
                        }
                    }

                    if (mulligan && playerCards.Count > 4)
                    {
                        playerCards.Clear();
                        mulligan = false;
                        Console.WriteLine("No longer in mulligan phase");
                    }

                    if (!mulligan && deck.Count > 0)
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
                            Console.WriteLine("Deleting cards from deck");
                            foreach (var name in toDelete)
                            {
                                deck["CardsInDeck"][name] = deck["CardsInDeck"].Value<int>(name) - 1;
                                //deck.CardsInDeck.Remove(name);
                                Console.Write("Decremented item: ");
                                Console.WriteLine(name);
                            }
                            toDelete.Clear();
                            printDeckList(deck, set, manaCostOrder);
                        }
                    }
                }
            }

            aTimer.Tick += new EventHandler(UpdateCardsInPlay);
            bTimer.Tick += new EventHandler(runChecks);

            Console.WriteLine("Heyo fuckface its ya boi LEGIIIIIIIIIIIT FOOD REVIEWS");
            bTimer.IsEnabled = true;
        }

        public static JArray LoadJson()
        {
            string json = Properties.Resources.set1_en_us;
            return JsonConvert.DeserializeObject<JArray>(json);
        }

        public void printDeckList(JObject deck, JArray set, List<string> order)
        {
            double top = 5;
            ClearControls();
            foreach (string cardCode in order)
            {
                string amount = deck["CardsInDeck"].Value<string>(cardCode);
                foreach (var item in set)
                {
                    if (item.Value<string>("cardCode") == cardCode)
                    {
                        // create button
                        Button button = new Button();
                        button.HorizontalAlignment = HorizontalAlignment.Left;
                        button.Margin = new Thickness(0,3,0,0);
                        button.FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#RomanSerif");
                        button.Width = this.Width - 5;
                        button.Height = 30;
                        button.Content = string.Format("{0,-3}{1,-25}{2}", item.Value<string>("cost"), item.Value<string>("name"), amount);
                        CreateButton(button);
                        top += button.Height + 2;
                        Console.WriteLine(string.Format("{0,-3}{1,-25}{2}", item.Value<string>("cost"), item.Value<string>("name"), amount));
                        break;
                    }
                }
            }
        }

        public static string httpReq(string URL)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync(URL).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        // by calling .Result you are synchronously reading the result
                        string responseString = responseContent.ReadAsStringAsync().Result;

                        return responseString;
                    }

                    return string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }

        private void CreateButton(Button button) //Create button
        {
            sp.Children.Add(button);
        }

        private void ClearControls() //Clear buttons
        {
            sp.Children.Clear();
        }

        private void ResetVars()
        {
            gameIsRunning = false;
            inGame = false;
            setLoaded = false;
            gotDeck = false;
            sorted = false;
            mulligan = true;
            deck = new JObject();
            toDelete = new List<string>();
            manaCostOrder = new List<string>();
            set = new JArray();
            cardsInPlay = new JArray();
            cardsInPlayCopy = new JArray();
            playerCards = new Dictionary<string, JObject>();
            purgatory = new Dictionary<string, JObject>();
            aTimer.IsEnabled = false;
        }

        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void CollapseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CollapseButton.Source = new BitmapImage(new Uri(@"/Resources/CollapseButtonClick.bmp", UriKind.Relative));
            if (!isMinimized)
            {
                prevHeight = Height;
                MaxHeight = MinHeight;
                isMinimized = true;
            }
            else
            {
                MaxHeight = 1080;
                Height = prevHeight;
                isMinimized = false;
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void CollapseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CollapseButton.Source = new BitmapImage(new Uri(@"/Resources/CollapseButtonHover.bmp", UriKind.Relative));
        }

        private void CollapseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseButton.Source = new BitmapImage(new Uri(@"/Resources/CollapseButton.bmp", UriKind.Relative));
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButtonClick.bmp", UriKind.Relative));
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButton.bmp", UriKind.Relative));
        }

        private void OptionsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButtonClick.bmp", UriKind.Relative));
            if (direction)
            {
                Height++;
               
            }
            else
            {
                Height--;
            }
            direction = !direction;
        }

        private void OptionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButtonHover.bmp", UriKind.Relative));
        }

        private void OptionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButton.bmp", UriKind.Relative));
        }
    }

}
