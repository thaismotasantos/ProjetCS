using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Projet
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void ApplicationStart(object sender, StartupEventArgs e)
    {
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var connexionWindow = new ConnexionWindow();

        if (connexionWindow.ShowDialog() == false)
        {
            var mainWindow = new MainWindow(connexionWindow.nickname, connexionWindow.address);
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            Current.MainWindow = mainWindow;
            mainWindow.Show();
        }
        else
        {
            MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
            Current.Shutdown(-1);
        }
    }
    }
}
