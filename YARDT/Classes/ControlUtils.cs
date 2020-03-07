using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YARDT
{
    class ControlUtils
    {
        static Dictionary<string, bool> isGreyed = new Dictionary<string, bool>();

        /// <summary>
        /// Create label with mana cost and card amount; if label already exists, just update card amount
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <param name="reset"></param>
        /// <param name="mainDirName"></param>
        public static void CreateLabel(StackPanel sp, JToken item, string amount, bool reset, string mainDirName) //Create button
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (reset)
                {
                    Label label = new Label
                    {
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 3, 0, 0),
                        FontFamily = new FontFamily(new Uri("pack://application:,,,/"), "./Resources/#RomanSerif"),
                        Width = 250 - 5,
                        Height = 30
                    };

                    Grid grid = new Grid();
                    grid.Margin = new Thickness(-5, 0, 0, 0);

                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col1_5 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    ColumnDefinition col3 = new ColumnDefinition();

                    col1.Width = new GridLength(25);
                    col1_5.Width = new GridLength(9);
                    col2.Width = new GridLength(176);
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

                    label.Content = grid;
                    string fileName = mainDirName + "full/" + item.Value<string>("cardCode") + "-full.png";
                    label.Background = new ImageBrush(new BitmapImage(new Uri(string.Join("", fileName), UriKind.Relative)));

                    label.Name = StringUtils.SanitizeString(item.Value<string>("name"));


                    //ToolTip stuff
                    //Get Image
                    System.IO.FileInfo file = new System.IO.FileInfo(mainDirName + "cards/" + item.Value<string>("cardCode").ToUpper()+".png");
                    Image myImage3 = new Image();
                    BitmapImage bi3 = new BitmapImage(new Uri(file.FullName, UriKind.Absolute));
                    myImage3.Stretch = Stretch.Fill;
                    myImage3.Source = bi3;

                    //Create ControlTemplate
                    ControlTemplate controlTemplate = new ControlTemplate(typeof(ToolTip));
                    FrameworkElementFactory contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));
                    controlTemplate.VisualTree = contentPresenter;

                    //Create ToolTip with Image and ControlTemplate
                    ToolTip tt = new ToolTip
                    {
                        Template = controlTemplate,
                        Content = myImage3
                    };

                    label.ToolTip = tt;

                    //Finally add label to Window
                    sp.Children.Add(label);

                    isGreyed.Add(label.Name, false);
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

                    Label label = sp.Children.OfType<Label>().Where(lbl => lbl.Name == StringUtils.SanitizeString(item.Value<string>("name"))).First<Label>();
                    Grid grid = label.Content as Grid;
                    TextBlock cardAmount = grid.Children.OfType<TextBlock>().Last();
                    
                    if (grid != null)
                    {
                        int column = Grid.GetColumn(cardAmount);
                        int row = Grid.GetRow(cardAmount);
                        int colSpan = Grid.GetColumnSpan(cardAmount);
                        int rowSpan = Grid.GetRowSpan(cardAmount);
                        grid.Children.Remove(cardAmount); //remove old canvas
                        grid.Children.Add(cardsLeft);//add new canvas
                        Grid.SetColumn(cardsLeft, column);
                        Grid.SetRow(cardsLeft, row);
                        Grid.SetColumnSpan(cardsLeft, colSpan);
                        Grid.SetRowSpan(cardsLeft, rowSpan);
                    }

                    if (!isGreyed[label.Name] && amount == "0")
                    {
                        greyOutLabel(label);
                    }

                }
            });
        }

        private static void greyOutLabel(Label label)
        {
            ImageBrush b = (ImageBrush)label.Background;
            BitmapSource src = (BitmapSource)b.ImageSource;
            System.Drawing.Bitmap bmp = ImageUtils.BitmapFromSource(src);
            
            bmp = ImageUtils.AddGradient(bmp, "xxxx");
            label.Background = new ImageBrush(ImageUtils.SourceFromBitmap(bmp));
            isGreyed[label.Name] = true;
        }

        /// <summary>
        /// Delete all labels and text from window
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="cardDrawPercentage1"></param>
        /// <param name="cardDrawPercentage2"></param>
        /// <param name="cardDrawPercentage3"></param>
        /// <param name="cardsInHandText"></param>
        public static void ClearControls(StackPanel sp, TextBlock cardDrawPercentage1, TextBlock cardDrawPercentage2, TextBlock cardDrawPercentage3, TextBlock cardsInHandText) //Clear buttons
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                sp.Children.Clear();
                cardDrawPercentage1.Text = "";
                cardDrawPercentage2.Text = "";
                cardDrawPercentage3.Text = "";
                cardsInHandText.Text = "";
            });
        }

        /// <summary>
        /// Change the title of the window
        /// </summary>
        /// <param name="windowTitle"></param>
        /// <param name="newTitle"></param>
        public static void ChangeMainWindowTitle(TextBlock windowTitle, string newTitle)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                windowTitle.Text = newTitle;
            });  
        }

        /// <summary>
        /// Create textBox in main window, mostly for debugging
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="content"></param>
        public static void CreateTextBox(StackPanel sp, string content)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                TextBox textBox = new TextBox
                {
                    Text = content,
                    Background = Brushes.Transparent,
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent
                };

                sp.Children.Add(textBox);
            });
        }

        /// <summary>
        /// Update amount of cards in deck, in the window
        /// </summary>
        /// <param name="cardDrawPercentage1"></param>
        /// <param name="cardDrawPercentage2"></param>
        /// <param name="cardDrawPercentage3"></param>
        /// <param name="cardsLeftText"></param>
        /// <param name="cardsLeftInDeck"></param>
        public static void UpdateCardsLeftInDeck(TextBlock cardDrawPercentage1, TextBlock cardDrawPercentage2, TextBlock cardDrawPercentage3,TextBlock cardsLeftText, int cardsLeftInDeck)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                cardsLeftText.Text = cardsLeftInDeck.ToString();
                cardDrawPercentage1.Text = Math.Round(100f / cardsLeftInDeck, 1).ToString("0.0");
                cardDrawPercentage2.Text = Math.Round(200f / cardsLeftInDeck, 1).ToString("0.0");
                cardDrawPercentage3.Text = Math.Round(300f / cardsLeftInDeck, 1).ToString("0.0");
            });
        }

        /// <summary>
        /// Update amount of cards in hand, in the window
        /// </summary>
        /// <param name="cardsInHandText"></param>
        /// <param name="cardsInHand"></param>
        public static void UpdateCardsInHand(TextBlock cardsInHandText, int cardsInHand)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                cardsInHandText.Text = cardsInHand.ToString();
            });
        }
    }
}
