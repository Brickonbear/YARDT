using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.IO.Compression;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Documents;
using System.Security.Cryptography;

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
        bool verified = false;
        bool labelsDrawn = false;
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

        public MainWindow()
        {
            InitializeComponent();

            if (!Directory.Exists("YARDTData"))
            {
                Directory.CreateDirectory("YARDTData");
                Console.WriteLine("created folder YARDTData");
            }
            else
            {
                Console.WriteLine("folder exists");
            }
            if (!Directory.Exists("YARDTTempData"))
            {
                Directory.CreateDirectory("YARDTTempData");
                Console.WriteLine("created folder YARDTTempData");
            }
            else
            {
                Console.WriteLine("folder exists");
            }

            Console.WriteLine("Verifying Data");
            for(int i = 5; i > 0; i--)
            {
                Console.WriteLine("Attempting to verify " + i + " tries left");
                verified = VerifyData(false);
                if (verified) break;

            }
            Console.WriteLine("Succesfully verified Data");
            
            aTimer.Interval = TimeSpan.FromMilliseconds(2000);

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
            
            aTimer.Tick += new EventHandler(UpdateCardsInPlay);

            Console.WriteLine("Heyo fuckface its ya boi LEGIIIIIIIIIIIT FOOD REVIEWS");
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(t => Main());
        }

        public void Main()
        {
            while (true)
            {
                while (!inGame || !gameIsRunning)
                {
                    try
                    {
                        JObject responseString = JsonConvert.DeserializeObject<JObject>(httpReq($"http://localhost:{port}/positional-rectangles"));


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
                        else if(inGame || aTimer.IsEnabled)
                        {
                            Console.WriteLine("\nNot currently in game, stopping timer");
                            aTimer.IsEnabled = false;
                            inGame = false;
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("\nCould not connect to game!");
                        Console.WriteLine("Trying again in 5 sec");
                        //Console.WriteLine("Message :{0} ", err.Message);
                        gameIsRunning = false;
                        Thread.Sleep(5000);
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

                if (cardsInPlay is JArray && !JToken.DeepEquals(cardsInPlay, cardsInPlayCopy))
                {
                    Console.WriteLine("Cards are different");
                    cardsInPlayCopy = cardsInPlay;
                    foreach (var card in cardsInPlayCopy)
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
                                Console.Write("Decremented item: ");
                                Console.WriteLine(name);
                            }
                            toDelete.Clear();
                            printDeckList(deck, set, manaCostOrder);
                        }
                    }
                }
            }
        }

        public bool VerifyData(bool downloaded)
        {
            string hash = "";
            string correctHash = "904e7678a42f5893424534df9941b96b";
            if(File.Exists(mainDirName + "set1-en_us.json"))
            {
                hash = CalculateMD5(mainDirName + "set1-en_us.json");         
            }

            Console.WriteLine(hash);

            if (!downloaded)
            {
                if (hash != correctHash)
                {
                    Console.WriteLine("Hashes don't match, deleting content of " + mainDirName);

                    DeleteFromDir(mainDirName);

                    DownloadToDir(tempDirName);

                    //Unzip File
                    ZipFile.ExtractToDirectory(tempDirName+"/datadragon-set1-en_us.zip", tempDirName+"/datadragon-set1-en_us");

                    var dir = new DirectoryInfo(tempDirName+"/datadragon-set1-en_us/en_us/img/cards");

                    var data = new FileInfo(tempDirName+"/datadragon-set1-en_us/en_us/data/set1-en_us.json");

                    data.MoveTo(mainDirName+ "/set1-en_us.json");

                    foreach (var file in dir.EnumerateFiles("*-alt*.png"))
                    {
                        file.Delete();
                    }
                    
                    Directory.CreateDirectory(mainDirName+"/full");
                    Directory.CreateDirectory(mainDirName+"/cards");
                    Console.WriteLine("created folder "+ mainDirName);

                    foreach (var file in dir.EnumerateFiles("*-full.png"))
                    {
                        string[] filename = { mainDirName+"/full/", file.Name , "_"};
                        file.MoveTo(string.Join("", filename));
                    }

                    foreach (var file in dir.EnumerateFiles())
                    {
                        string[] filename = { mainDirName+"/cards/", file.Name };
                        file.MoveTo(string.Join("", filename));
                    }

                    dir = new DirectoryInfo(mainDirName+"/full");

                    foreach (var file in dir.EnumerateFiles("*.png_"))
                    {
                        Bitmap image;
                        Bitmap img = new Bitmap(file.FullName);
                        if (img.Width == 1024)
                        {
                            image = ResizeImage(img, 250, 250);
                            CropImage(image, file.FullName, 25, 110, 200, 30);
                        }
                        else
                        {
                            image = ResizeImage(img, 200, 100);
                            CropImage(image, file.FullName, 0, 30, 200, 30);
                        }
                        img.Dispose();
                        file.Delete();
                    }
                    bool verified = VerifyData(true);
                    if (verified)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return hash == correctHash;
        }

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public void CropImage(Bitmap image, string name, int x, int y, int width, int height)
        {
            Bitmap croppedImage;

            // Here we capture the resource - image file.
            using (image)
            {
                Rectangle crop = new Rectangle(x, y, width, height);

                // Here we capture another resource.
                croppedImage = image.Clone(crop, image.PixelFormat);

            } // Here we release the original resource - bitmap in memory and file on disk.

            croppedImage = AddGradient(croppedImage, name);

            // At this point the file on disk already free - you can record to the same path.
            croppedImage.Save(name.TrimEnd('_'), ImageFormat.Png);

            // It is desirable release this resource too.
            croppedImage.Dispose();
        }
        public Bitmap AddGradient (Bitmap image, string name)
        {
            Bitmap gradient;
            //Console.WriteLine(name.Split('\\').Last<string>().Substring(2, 2).ToLower());
            switch (name.Split('\\').Last<string>().Substring(2,2).ToLower())
            {
                case "de":
                    gradient = new Bitmap(Properties.Resources.GradientDemacia);
                    break;
                case "fr":
                    gradient = new Bitmap(Properties.Resources.GradientFreljord);
                    break;
                case "io":
                    gradient = new Bitmap(Properties.Resources.GradientIonia);
                    break;
                case "nx":
                    gradient = new Bitmap(Properties.Resources.GradientNoxus);
                    break;
                case "pz":
                    gradient = new Bitmap(Properties.Resources.GradientPiltoverZaun);
                    break;
                case "si":
                    gradient = new Bitmap(Properties.Resources.GradientShadowIsles);
                    break;
                default:
                    gradient = new Bitmap(250, 30);
                    break;
            }

            var target = new Bitmap(250, 30, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            graphics.CompositingMode = CompositingMode.SourceOver; // this is the default, but just to be clear

            graphics.DrawImage(image, 50, 0);
            graphics.DrawImage(gradient, 0, 0);

            return target;
        }
        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }
            return destImage;
        }

        public void DownloadToDir(string directory)
        {
            Console.WriteLine("Begining Data Dragon download");
            using (var client = new WebClient())
            {
                client.DownloadFile("https://dd.b.pvp.net/datadragon-set1-en_us.zip", directory + "/datadragon-set1-en_us.zip");
            }
            Console.WriteLine("Finished download");
        }
        
        public void DeleteFromDir(string directory)
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

        public static JArray LoadJson()
        {
            using (StreamReader r = new StreamReader(mainDirName + "set1-en_us.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<JArray>(json);
            }
        }

        public void printDeckList(JObject deck, JArray set, List<string> order)
        {
            //ClearControls();
            foreach (string cardCode in order)
            {
                string amount = deck["CardsInDeck"].Value<string>(cardCode);
                foreach (var item in set)
                {
                    if (item.Value<string>("cardCode") == cardCode)
                    {
                        //Create button
                        CreateButton(item, amount, !labelsDrawn);
                        //top += button.Height + 2;
                        Console.WriteLine(string.Format("{0,-3}{1,-25}{2}", item.Value<string>("cost"), item.Value<string>("name"), amount));
                        break;
                    }
                }
            }
            labelsDrawn = true;
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

                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        private void CreateButton(JToken item, string amount, bool reset) //Create button
        {
            Dispatcher.Invoke(() =>
            {
                if (reset)
                {
                    Label label = new Label
                    {
                        Foreground = System.Windows.Media.Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 3, 0, 0),
                        FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/"), "./Resources/#RomanSerif"),
                        Width = Width - 5,
                        Height = 30
                    };

                    Console.WriteLine("This should only happen once");

                    Grid grid = new Grid();

                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col1_5 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    ColumnDefinition col3 = new ColumnDefinition();

                    col1.Width = new GridLength(16);
                    col1_5.Width = new GridLength(14);
                    col2.Width = new GridLength(180);
                    col3.Width = new GridLength(40);

                    grid.ColumnDefinitions.Add(col1);
                    grid.ColumnDefinitions.Add(col1_5);
                    grid.ColumnDefinitions.Add(col2);
                    grid.ColumnDefinitions.Add(col3);

                    TextBlock manaCost = new TextBlock(new Run(item.Value<string>("cost")));
                    TextBlock name = new TextBlock(new Run(item.Value<string>("name")));
                    TextBlock cardsLeft = new TextBlock(new Run("x" + amount))
                    {
                        Name = "cardAmount"
                    };

                    manaCost.FontSize = 22;
                    name.FontSize = 16;
                    cardsLeft.FontSize = 22;

                    manaCost.FontWeight = FontWeights.Bold;
                    name.FontWeight = FontWeights.Bold;
                    cardsLeft.FontWeight = FontWeights.Bold;

                    manaCost.VerticalAlignment = VerticalAlignment.Center;
                    name.VerticalAlignment = VerticalAlignment.Center;
                    cardsLeft.VerticalAlignment = VerticalAlignment.Center;

                    manaCost.HorizontalAlignment = HorizontalAlignment.Center;

                    Grid.SetColumn(manaCost, 0);
                    Grid.SetColumn(name, 2);
                    Grid.SetColumn(cardsLeft, 3);

                    grid.Children.Add(manaCost);
                    grid.Children.Add(name);
                    grid.Children.Add(cardsLeft);

                    label.Content = grid;//string.Format("{0,-3}{1,-25}{2}", item.Value<string>("cost"), item.Value<string>("name"), amount);
                    string[] fileName = { mainDirName + "full/", item.Value<string>("cardCode"), "-full.png" };
                    //Console.WriteLine(string.Join("", fileName));
                    //var img = CropAtRect(new BitmapImage(new Uri(string.Join("", fileName), UriKind.Relative)), new Rectangle(500, 250, 250, 30))

                    label.Background = new ImageBrush(new BitmapImage(new Uri(string.Join("", fileName), UriKind.Relative)));
                    label.Name = item.Value<string>("name").Replace(" ", "");
                    sp.Children.Add(label);
                }
                else
                {
                    TextBlock cardsLeft = new TextBlock(new Run("x" + amount))
                    {
                        Name = "cardAmount"
                    };
                    cardsLeft.FontSize = 22;
                    cardsLeft.FontWeight = FontWeights.Bold;
                    cardsLeft.VerticalAlignment = VerticalAlignment.Center;

                    Grid grid = sp.Children.OfType<Label>().Where(label => label.Name == item.Value<string>("name").Replace(" ", "")).First<Label>().Content as Grid;
                    TextBlock cardAmount = grid.Children.OfType<TextBlock>().Last();
                    Console.WriteLine(cardAmount.Text);
                    if (grid != null)
                    {
                        var column = Grid.GetColumn(cardAmount);
                        var row = Grid.GetRow(cardAmount);
                        var colSpan = Grid.GetColumnSpan(cardAmount);
                        var rowSpan = Grid.GetRowSpan(cardAmount);
                        grid.Children.Remove(cardAmount); //remove old canvas
                        grid.Children.Add(cardsLeft);//add new canvas
                        Grid.SetColumn(cardsLeft, column);
                        Grid.SetRow(cardsLeft, row);
                        Grid.SetColumnSpan(cardsLeft, colSpan);
                        Grid.SetRowSpan(cardsLeft, rowSpan);
                    }

                    Console.WriteLine(item.Value<string>("name"));
                }
            });
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
            Application.Current.Shutdown();
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
