using Projet.modele;
using Projet.udp;
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
        /*private MainWindow mainWindow;
        private ChatUDPController chatUDPController;
        private LinkedList<Chatroom> chatrooms;*/

        /*public App()
        {
            chatrooms = new LinkedList<Chatroom>();

            Chatroom defaultChannelChatroom = new Chatroom("");
            chatrooms.AddLast(defaultChannelChatroom);

            chatUDPController = new ChatUDPController();
        }*/

        private void ApplicationStart(object sender, StartupEventArgs e)
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            /*var connexionWindow = new ConnexionWindow();

            if (connexionWindow.ShowDialog() == false)
            {
                ChatUDPController chatUDPController = new ChatUDPController(connexionWindow.nickname, connexionWindow.addressFirstReceiver,
                                            connexionWindow.portFirstReceiver);
                var mainWindow = new MainWindow(connexionWindow.nickname, connexionWindow.addressFirstReceiver,
                                            connexionWindow.portFirstReceiver, chatUDPController);*/

                ChatUDPController chatUDPController = new ChatUDPController("thais", "192.168.56.1", "2323");
                var mainWindow = new MainWindow("thais", "192.168.56.1", "2323", chatUDPController);
                chatUDPController.mainWindow = mainWindow;
                Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
                Current.MainWindow = mainWindow;
                mainWindow.Show();
            /*}
            else
            {
                MessageBox.Show("Unable to load data.", "Error", MessageBoxButton.OK);
                Current.Shutdown(-1);
            }*/
        }
    }
}
