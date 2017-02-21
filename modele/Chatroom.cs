using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    public class Chatroom
    {
        public string name { get; set; }
        public List<Message> messages { get; set; }

        public Chatroom(string name)
        {
            this.name = name;
        }
    }
}
