using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace YARDT
{
    /// <summary>
    /// Interaction logic for ChooseLanguage.xaml
    /// </summary>
    public partial class ChooseLanguage : Window
    {
        private double prevHeight;
        private bool isMinimized;

        public ChooseLanguage()
        {
            InitializeComponent();
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
        }

        private void CloseButton_MouseEnter(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButtonClick.bmp", UriKind.Relative));
        }

        private void CloseButton_MouseLeave(object sender, MouseEventArgs e)
        {
            CloseButton.Source = new BitmapImage(new Uri(@"/Resources/CloseButton.bmp", UriKind.Relative));
        }

        private void portApplyButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Language = ((ComboBoxItem)choosenLanguage.SelectedItem).Content.ToString();
            Properties.Settings.Default.LanguageChosen = true;
            Properties.Settings.Default.Save();
            Close();

        }
    }
}
