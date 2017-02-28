using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    public class Chatroom
    {
        public string name { get; set; }
        public ObservableCollection<Message> messages { get; set; }

        public Chatroom(string name)
        {
            messages = new ObservableCollection<Message>();
            this.name = name;
        }
    }
}
