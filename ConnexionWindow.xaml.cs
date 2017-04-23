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
    /// <summary>
    /// Logique d'interaction pour ConnexionWindow.xaml
    /// </summary>
    public partial class ConnexionWindow : System.Windows.Window
    {
        public string nickname;
        public string addressFirstReceiver;
        public string portFirstReceiver;

        public ConnexionWindow()
        {
            InitializeComponent();
        }

        private void buttonSubmitTopicName_Click(object sender, RoutedEventArgs e)
        {
            nickname = textBoxNickname.Text;
            addressFirstReceiver = textBoxIP.Text;
            portFirstReceiver = textBoxPort.Text;
            this.Close();
        }

        // TODO : faire validation des champs
    }
}
