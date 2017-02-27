using Projet.modele;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Projet
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // lancer le listener
            // remplir liste de noeuds voisins
            List<Chatroom> chatrooms = new List<Chatroom>();
            Chatroom c1 = new Chatroom("c1");
            Chatroom c2 = new Chatroom("c2");
            Chatroom c3 = new Chatroom("c3");
            chatrooms.Add(c1);
            chatrooms.Add(c2);
            chatrooms.Add(c3);

            listBoxChatrooms.ItemsSource = chatrooms;

            List<string> participants = new List<string>();
            participants.Add("thais");
            participants.Add("dragos");

            listBoxParticipants.ItemsSource = participants;
        }



        private void buttonSendMessage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void listBoxParticipants_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            object item = null;
            item = GetElementFromPoint(listBoxParticipants, e.GetPosition(listBoxParticipants));
            if (item != null)
            {
                PrivateChatWindow pcw = new PrivateChatWindow(item.ToString());
                pcw.Show();
            }
        }

        private object GetElementFromPoint(ListBox listBox, Point point)
        {
            UIElement element = (UIElement)listBox.InputHitTest(point);
            while (true)
            {
                if (element == listBox)
                {
                    return null;
                }
                object item = listBox.ItemContainerGenerator.ItemFromContainer(element);
                bool itemFound = !(item.Equals(DependencyProperty.UnsetValue));
                if (itemFound)
                {
                    return item;
                }
                element = (UIElement)VisualTreeHelper.GetParent(element);
            }
        }
        
    }
}
