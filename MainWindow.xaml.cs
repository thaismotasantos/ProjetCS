using Projet.modele;
using Projet.udp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class MainWindow : System.Windows.Window
    {
        public string nickname;
        public string addressFirstReceiver;
        public string portFirstReceiver;

        public ObservableCollection<Chatroom> chatrooms = new ObservableCollection<Chatroom>();
        public ObservableCollection<Message> messagesOfSelectedChatroom;
        public ObservableCollection<string> peers = new ObservableCollection<string>();

        public ChatUDPController chatUDPController;

        public MainWindow(string nickname, string addressFirstReceiver, string portFirstReceiver, ChatUDPController chatUDPController)
        {
            InitializeComponent();

            this.nickname = nickname;
            this.addressFirstReceiver = addressFirstReceiver;
            this.portFirstReceiver = portFirstReceiver;

            //populateChatrooms();
            messagesOfSelectedChatroom = new ObservableCollection<Message>();

            Chatroom c1 = new Chatroom("");
            chatrooms.Add(c1);

            //peers.Add(nickname);

            listBoxChatrooms.ItemsSource = chatrooms;
            listBoxChatrooms.SelectedItem = chatrooms[0];
            /*messagesOfSelectedChatroom = chatrooms[0].messages;*/
            listViewMessages.ItemsSource = messagesOfSelectedChatroom;

            listBoxParticipants.ItemsSource = peers;
            this.chatUDPController = chatUDPController; //new ChatUDPController();
        }

        /*private void populateChatrooms()
        {
            /*Chatroom c1 = new Chatroom("Real world");
            Chatroom c2 = new Chatroom("Hogwarts");
            Chatroom c3 = new Chatroom("Middle Earth");

            chatrooms.Add(c1);
            chatrooms.Add(c2);
            chatrooms.Add(c3);

            c1.messages.Add(new Message("thais", "Coucou tout le monde", "")); // , ""
            c1.messages.Add(new Message("dragos", "Coucou toi", ""));
            c1.messages.Add(new Message("aurore", "Hey", ""));

            c2.messages.Add(new Message("harry", "Expecto patronum", ""));
            c2.messages.Add(new Message("ron", "Wingardium Leviosa", ""));
            c2.messages.Add(new Message("hermione", "Alohomora", ""));

            c3.messages.Add(new Message("frodo", "My precious", ""));
            c3.messages.Add(new Message("sam", "No, Mister Frodo", ""));
            c3.messages.Add(new Message("gandalf", "You shall not pass", ""));

            peers.Add("thais");
            peers.Add("dragos");
            peers.Add("aurore");
            peers.Add("harry");
            peers.Add("ron");
            peers.Add("hermione");
            peers.Add("frodo");
            peers.Add("sam");
            peers.Add("gandalf");*
        }*/

        private void buttonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            string textMsg = textBoxChat.Text;

            if(!String.IsNullOrEmpty(textMsg))
            {
                Chatroom selectedChatroom = (Chatroom)listBoxChatrooms.SelectedItem;

                Message msg = new Message(chatUDPController.myNickname, textMsg, selectedChatroom.name/*, chatUDPController.myNickname*/);
                chatUDPController.sendMessage(msg);
                //messagesOfSelectedChatroom.Add(msg);

                selectedChatroom.messages.Add(msg);

                messagesOfSelectedChatroom.Clear();
                selectedChatroom.messages.ToList().ForEach(m => messagesOfSelectedChatroom.Add(m));

                this.textBoxChat.Text = "";
            }
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
        
        private void listBoxChatrooms_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(listBoxChatrooms, e.OriginalSource as DependencyObject) as ListBoxItem;
            if (item != null)
            {
                MessageBox.Show(item.ToString());
            }
        }

        private void listBoxChatrooms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Chatroom selectedChatroom = (Chatroom) listBoxChatrooms.SelectedItem;
            messagesOfSelectedChatroom.Clear();
            selectedChatroom.messages.ToList().ForEach(m => messagesOfSelectedChatroom.Add(m));
            //messagesOfSelectedChatroom.Concat(selectedChatroom.messages);
            //messagesOfSelectedChatroom = selectedChatroom.messages;
            //listViewMessages.ItemsSource = selectedChatroom.messages;
            //listViewMessages.ItemsSource = selectedChatroom.messages;
        }

        private void buttonAddTopic_Click(object sender, RoutedEventArgs e)
        {
            TopicNameForm tnf = new TopicNameForm(this);
            tnf.Show();
        }

        public void addMessageToChatroom(Message message, string chatName)
        {
            Chatroom chatroom;

            if (chatrooms.Any(c => c.name.Equals(chatName)))
            {
                chatroom = chatrooms.ToList().FindAll(c => c.name.Equals(chatName)).First();
            }
            else
            {
                chatroom = new Chatroom(chatName);
                chatrooms.Add(chatroom);
            }

            listBoxChatrooms.SelectedItem = chatroom;

            chatroom.messages.Add(message);
            //messagesOfSelectedChatroom = chatroom.messages;
            messagesOfSelectedChatroom.Clear();
            chatroom.messages.ToList().ForEach(m => messagesOfSelectedChatroom.Add(m));
            //messagesOfSelectedChatroom.Add( Concat(chatroom.messages);
        }

        public void addParticipant(string nickname)
        {
            peers.Add(nickname);
        }

        public void removeParticipant(string nickname)
        {
            peers.Remove(nickname);
        }

        public void changeChatroom(Chatroom chatroom)
        {
            if (!chatrooms.Any(c => c.name.Equals(chatroom.name)))
            {
                chatrooms.Add(chatroom);
            }
            listBoxChatrooms.SelectedItem = chatroom;
            //messagesOfSelectedChatroom = chatroom.messages;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            chatUDPController.stop();
            //envoyer message goodbye
            chatUDPController.sendGoodbye();
        }
    }
}
