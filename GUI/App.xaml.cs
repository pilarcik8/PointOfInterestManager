using System.Configuration;
using System.Data;
using System.Windows;

namespace GUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Vygenerovane / upravene pomocou AI - Kapitatila 69
        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Kym je otvoreny dialog, aplikacia sa nesmie ukoncit po zatvoreni posledneho okna
            var selection = new TreeSelectionWindow();
            if (selection.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var mainWindow = new MainWindow(selection.SelectedTreeType);
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }
    }

}
