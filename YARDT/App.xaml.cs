using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using YARDT.Classes;

namespace YARDT
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            if (YARDT.Properties.Settings.Default.UpgradeRequired)
            {
                YARDT.Properties.Settings.Default.Upgrade();
                YARDT.Properties.Settings.Default.UpgradeRequired = false;
                YARDT.Properties.Settings.Default.Save();
            }

            await Task.Run(async () => await Updater.Run());

            if (!YARDT.Properties.Settings.Default.LanguageChosen)
            {
                ChooseLanguage chooseLanguage = new ChooseLanguage();
                chooseLanguage.ShowDialog();
                Updater.RunNew();
            }

            MainWindow window = new MainWindow();
            window.Show();
        }
    }
}
