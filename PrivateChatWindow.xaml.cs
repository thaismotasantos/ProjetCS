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

    public partial class PrivateChatWindow : Window
    {
        public string PrivateChatTitle { get; private set; }

        public PrivateChatWindow(string username)
        {
            InitializeComponent();
            DataContext = this;
            PrivateChatTitle = username;
        }

        
    }
}
