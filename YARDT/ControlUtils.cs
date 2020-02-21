using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace YARDT
{
    class ControlUtils
    {

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

                    Grid grid = sp.Children.OfType<Label>().Where(label => label.Name == StringUtils.SanitizeString(item.Value<string>("name"))).First<Label>().Content as Grid;
                    TextBlock cardAmount = grid.Children.OfType<TextBlock>().Last();
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

                }
            });
        }

        public static void ClearControls(Dispatcher dispatcher, StackPanel sp) //Clear buttons
        {
            dispatcher.Invoke(() =>
            {
                sp.Children.Clear();
            });
        }

    }
}
