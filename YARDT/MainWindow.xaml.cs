using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Imaging;

namespace YARDT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        bool gameIsRunning = false;
        bool inGame = false;
        bool setLoaded = false;
        bool gotDeck = false;
        bool sorted = false;
        bool mulligan = true;
        bool isMinimized = false;
        bool labelsDrawn = false;
        bool printMenu = true;
        double prevHeight = 0;
        JObject deck = new JObject();
        List<string> toDelete = new List<string>();
        List<string> manaCostOrder = new List<string>();
        JArray set = new JArray();
        JArray cardsInPlay = new JArray();
        JArray cardsInPlayCopy = new JArray();
        Dictionary<string, JObject> playerCards = new Dictionary<string, JObject>();
        Dictionary<string, JObject> purgatory = new Dictionary<string, JObject>();
        readonly DispatcherTimer aTimer = new DispatcherTimer();
        const string mainDirName = "YARDTData/";
        const string tempDirName = "YARDTTempData/";

        Dictionary<string, string> hashTable = new Dictionary<string, string>()
        {
            {"de_de", "d19422011f99ede7490fe445b046b0a5"},
            {"en_us", "885a4f142a285bcb0c7b1dd9767c0023"},
            {"es_es", "6e9b725464203a06271c90529a242e08"},
            {"fr_fr", "123354b6b920e66f0fe97eedc58c1d84"},
            {"it_it", "222561a9c3cada90ca6a8be50374e1b0"},
            {"ja_jp", "7eea717db7e1c1b84ceca0219f9cdff9"},
            { "ko_kr", "f0d30526c42cd2d4a6d7b4d0556762c3"}
        };

        

        public MainWindow()
        {
            InitializeComponent();

            portSettingText.Text = Properties.Settings.Default.Port.ToString();

            aTimer.Interval = TimeSpan.FromMilliseconds(2000);
            aTimer.Tick += new EventHandler(UpdateCardsInPlay);

            System.Threading.Tasks.Task.Delay(50).ContinueWith(t => VerifyData(false));
        }

        public void Main()
        {
            ControlUtils.ClearControls(sp);

            while (true)
            {
                while (!inGame || !gameIsRunning)
                {
                    try
                    {
                        JObject responseString = JsonConvert.DeserializeObject<JObject>(Utils.HttpReq($"http://localhost:{Properties.Settings.Default.Port}/positional-rectangles"));


                        gameIsRunning = true;
                        if (responseString["GameState"].ToString() == "InProgress")
                        {
                            inGame = true;
                            Console.WriteLine("Starting timer");
                            ControlUtils.ChangeMainWindowTitle("YARDT");
                            aTimer.IsEnabled = true;

                            if (!gotDeck)
                            {
                                gotDeck = true;
                                
                                string resString = Utils.HttpReq($"http://localhost:{Properties.Settings.Default.Port}/static-decklist");
                                if (resString == "failure")
                                {
                                    JObject expeditionState = JsonConvert.DeserializeObject<JObject>(Utils.HttpReq($"http://localhost:{Properties.Settings.Default.Port}/expeditions-state"));
                                    deck = DeckFromExpedition(expeditionState);
                                }
                                else
                                {
                                    deck = JsonConvert.DeserializeObject<JObject>(resString);
                                }
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
                            if (printMenu)
                            {
                                Console.WriteLine("In menu, waiting for game to start");
                                ControlUtils.ChangeMainWindowTitle("Waiting for match to start");
                                printMenu = false;
                            }

                            if (inGame || aTimer.IsEnabled)
                            {
                                Console.WriteLine("Not currently in game, stopping timer");
                                aTimer.IsEnabled = false;
                                inGame = false;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Could not connect to game!");
                        Console.WriteLine("Trying again in 5 sec");
                        ControlUtils.ChangeMainWindowTitle("Waiting for game to start");
                        //Console.WriteLine("Message :{0} ", err.Message);
                        gameIsRunning = false;
                        Thread.Sleep(5000);
                    }
                }

                //Load set from json
                if (!setLoaded)
                {
                    set = FileUtils.LoadJson(mainDirName);
                    setLoaded = true;
                    Console.WriteLine("Loaded set");
                }

                if (!sorted && setLoaded)
                {
                    Console.WriteLine("Sorting deck");
                    manaCostOrder.Sort((x, y) =>
                    {
                        int xManaCost = -1, yManaCost = -1;
                        foreach (JToken item in set)
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

                if (cardsInPlay is JArray && !JToken.DeepEquals(cardsInPlay, cardsInPlayCopy))
                {
                    Console.WriteLine("Cards are different");
                    cardsInPlayCopy = cardsInPlay;
                    foreach (JToken card in cardsInPlayCopy)
                    {
                        if (!playerCards.ContainsKey(card.Value<string>("CardID")))
                        {
                            if (card.Value<bool>("LocalPlayer") == true)
                            {
                                if (card.Value<string>("CardCode") != "face")
                                {
                                    Console.WriteLine("Adding card: " + card.Value<string>("CardID") + " to playerCards");
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
                        Utils.PrintDeckList(deck, set, manaCostOrder, sp, ref labelsDrawn, mainDirName);
                    }

                    if (!mulligan && deck.Count > 0)
                    {
                        foreach (string card in playerCards.Keys)
                        {
                            if (!purgatory.ContainsKey(card))
                            {
                                purgatory.Add(card, playerCards[card]);
                                foreach (JToken item in deck["CardsInDeck"])
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
                            foreach (string name in toDelete)
                            {
                                deck["CardsInDeck"][name] = deck["CardsInDeck"].Value<int>(name) - 1;
                                Console.Write("Decremented item: ");
                                Console.WriteLine(name);
                            }
                            toDelete.Clear();
                            Utils.PrintDeckList(deck, set, manaCostOrder, sp, ref labelsDrawn, mainDirName);
                        }
                    }
                }
                Thread.Sleep(500);
            }
        }

        private JObject DeckFromExpedition(JObject expeditionState)
        {
            JObject deckList = new JObject();
            JObject cardsInDeck = new JObject();
            deckList.Add("DeckCode", "DECKCODE");

            foreach (string cardCode in expeditionState["Deck"])
            {
                if (cardsInDeck.ContainsKey(cardCode)){
                    cardsInDeck[cardCode] = (int)cardsInDeck[cardCode] + 1;
                }
                else
                {
                    cardsInDeck.Add(cardCode, 1);
                }
            }

            deckList.Add("CardsInDeck", cardsInDeck);
            return deckList;
        }

        public bool VerifyData(bool downloaded)
        {
            Console.WriteLine("Verifying Data");
            ControlUtils.CreateTextBox(sp, "Verifying Data");

            string hash = "";
            hashTable.TryGetValue(Properties.Settings.Default.Language, out string correctHash);
            if (File.Exists(mainDirName + "set1-" + Properties.Settings.Default.Language + ".json"))
            {
                hash = StringUtils.CalculateMD5(mainDirName + "set1-" + Properties.Settings.Default.Language + ".json");
            }

            if (hash == correctHash)
            {
                Console.WriteLine("Deleting Temp Data");
                ControlUtils.CreateTextBox(sp, "Deleting Temp Data");
                if (Directory.Exists(tempDirName))
                {
                    FileUtils.DeleteFromDir(tempDirName);
                    Directory.Delete(tempDirName);
                }
                Console.WriteLine("Succesfully verified Data");
                ControlUtils.CreateTextBox(sp, "Succesfully verified Data");
                System.Threading.Tasks.Task.Delay(100).ContinueWith(t => Main());
                return true;
            }

            if (!downloaded)
            {
                if (hash != correctHash)
                {
                    Console.WriteLine("Hashes don't match");
                    ControlUtils.CreateTextBox(sp, "Hashes don't match");

                    Directory.CreateDirectory(mainDirName);
                    Console.WriteLine("Created folder " + mainDirName);
                    ControlUtils.CreateTextBox(sp, "Created folder " + mainDirName);

                    Directory.CreateDirectory(tempDirName);
                    Console.WriteLine("Created folder " + tempDirName);
                    ControlUtils.CreateTextBox(sp, "Created folder " + tempDirName);

                    Console.WriteLine("Deleting content of " + mainDirName);
                    ControlUtils.CreateTextBox(sp, "Deleting content of " + mainDirName);
                    FileUtils.DeleteFromDir(mainDirName);

                    ControlUtils.CreateTextBox(sp, "Downloading DataDragon");

                    FileUtils.DownloadToDir(tempDirName);

                    //Unzip File
                    Console.WriteLine("Unziping DataDragon");
                    ControlUtils.CreateTextBox(sp, "Unziping DataDragon");
                    ZipFile.ExtractToDirectory(tempDirName + "/datadragon-set1-" + Properties.Settings.Default.Language + ".zip", tempDirName + "/datadragon-set1-" + Properties.Settings.Default.Language + "");

                    DirectoryInfo dir = new DirectoryInfo(tempDirName + "/datadragon-set1-" + Properties.Settings.Default.Language + "/" + Properties.Settings.Default.Language + "/img/cards");

                    foreach (FileInfo file in dir.EnumerateFiles("*-alt*.png"))
                    {
                        file.Delete();
                    }

                    Directory.CreateDirectory(mainDirName + "/full");
                    Directory.CreateDirectory(mainDirName + "/cards");

                    Console.WriteLine("Moving full images to " + mainDirName + "full/");
                    ControlUtils.CreateTextBox(sp, "Moving full images to " + mainDirName + "full/");
                    foreach (FileInfo file in dir.EnumerateFiles("*-full.png"))
                    {
                        string[] filename = { mainDirName + "/full/", file.Name, "_" };
                        file.MoveTo(string.Join("", filename));
                    }
                    
                    Console.WriteLine("Moving cards to " + mainDirName + "cards/");
                    ControlUtils.CreateTextBox(sp, "Moving cards to " + mainDirName + "cards/");
                    foreach (FileInfo file in dir.EnumerateFiles())
                    {
                        string[] filename = { mainDirName + "/cards/", file.Name, "_" };
                        file.MoveTo(string.Join("", filename));
                    }
                   
                    dir = new DirectoryInfo(mainDirName + "/cards");
                    Console.WriteLine("Resizing card images");
                    ControlUtils.CreateTextBox(sp, "Resizing card images");
                    foreach (FileInfo file in dir.EnumerateFiles("*.png_"))
                    {
                        Bitmap image;
                        Bitmap img = new Bitmap(file.FullName);
                        image = ImageUtils.ResizeImage(img, 340, 512);
                        image.Save(file.FullName.TrimEnd('_'), ImageFormat.Png);
                        img.Dispose();
                        file.Delete();
                    }
                    
                    dir = new DirectoryInfo(mainDirName + "/full");
                    Console.WriteLine("Cropping full images and applying gradient");
                    ControlUtils.CreateTextBox(sp, "Cropping full images and applying gradient");
                    foreach (FileInfo file in dir.EnumerateFiles("*.png_"))
                    {
                        Bitmap image;
                        Bitmap img = new Bitmap(file.FullName);
                        if (img.Width == 1024)
                        {
                            image = ImageUtils.ResizeImage(img, 250, 250);
                            image = ImageUtils.CropImage(image, 25, 110, 200, 30);
                            image = ImageUtils.AddGradient(image, file.FullName);
                            image.Save(file.FullName.TrimEnd('_'), ImageFormat.Png);
                        }
                        else
                        {
                            image = ImageUtils.ResizeImage(img, 200, 100);
                            image = ImageUtils.CropImage(image, 0, 30, 200, 30);
                            image = ImageUtils.AddGradient(image, file.FullName);
                            image.Save(file.FullName.TrimEnd('_'), ImageFormat.Png);
                        }
                        img.Dispose();
                        file.Delete();
                    }

                    FileInfo dataSetFile = new FileInfo(tempDirName + "/datadragon-set1-" + Properties.Settings.Default.Language + "/" + Properties.Settings.Default.Language + "/data/set1-" + Properties.Settings.Default.Language + ".json");
                    dataSetFile.MoveTo(mainDirName + "/set1-" + Properties.Settings.Default.Language + ".json");

                    bool verified = VerifyData(true);
                    if (!verified)
                    {
                        Console.WriteLine("Could not verify data");
                        Environment.Exit(1337);
                    }
                }
            }
            return hash == correctHash;
        }

        public async void UpdateCardsInPlay(object source, EventArgs e)
        {
            try
            {
                JObject responseString = JsonConvert.DeserializeObject<JObject>(await client.GetStringAsync($"http://localhost:{Properties.Settings.Default.Port}/positional-rectangles"));
                if (responseString["GameState"].ToString() == "Menus")
                {

                    Console.WriteLine("Not in game, stopping timer");
                    aTimer.IsEnabled = false;
                    ResetVars();
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
                ResetVars();
            }
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
            labelsDrawn = false;
            printMenu = true;
            ControlUtils.ClearControls(sp);
        }

        //Main window functions
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        //CollapseButton Functions
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

        private void CollapseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CollapseButton.Source = new BitmapImage(new Uri(@"/Resources/CollapseButtonHover.bmp", UriKind.Relative));
        }

        private void CollapseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CollapseButton.Source = new BitmapImage(new Uri(@"/Resources/CollapseButton.bmp", UriKind.Relative));
        }

        //CloseButton Functions
        private void CloseButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Environment.Exit(Environment.ExitCode);
            //Application.Current.Shutdown();
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButtonClick.bmp", UriKind.Relative));
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButton.bmp", UriKind.Relative));
        }

        //OptionsButton Functions
        private void OptionsButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButtonClick.bmp", UriKind.Relative));

            if(settingsSP.Visibility == Visibility.Collapsed)
            {
                portSettingText.Text = Properties.Settings.Default.Port.ToString();
                settingsSP.Visibility = Visibility.Visible;
            }
            else
            {
                settingsSP.Visibility = Visibility.Collapsed;
            }
        }

        private void OptionsButton_MouseEnter(object sender, MouseEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButtonHover.bmp", UriKind.Relative));
        }

        private void OptionsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            OptionsButton.Source = new BitmapImage(new Uri(@"/Resources/OptionsButton.bmp", UriKind.Relative));
        }
        
        //Settings menu Functions
         private void portApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (portSettingText.Text != Properties.Settings.Default.Port.ToString())
            {
                int number;

                bool success = int.TryParse(portSettingText.Text, out number);
                if (success)
                {
                    Properties.Settings.Default.Port = number;
                    Properties.Settings.Default.Save();
                }
            }
        }
    }
}
