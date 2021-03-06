﻿using Projet.modele;
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

namespace Projet
{
    public partial class TopicNameForm : System.Windows.Window
    {
        MainWindow win;

        public TopicNameForm(MainWindow win)
        {
            InitializeComponent();
            this.win = win;
        }

        private void buttonSubmitTopicName_Click(object sender, RoutedEventArgs e)
        {
            Chatroom newChatroom = new Chatroom(textBoxTopicName.Text);
            win.changeChatroom(newChatroom);
            this.Close();
        }
    }
}
