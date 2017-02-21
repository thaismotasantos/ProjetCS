using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    class Peer
    {
        public string addr { get; set; }
        public string port { get; set; }

        public Peer(string addr, string port)
        {
            this.addr = addr;
            this.port = port;
        }
    }
}
