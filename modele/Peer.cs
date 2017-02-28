using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    public class Peer
    {
        public string addr { get; set; }
        public Int32 port { get; set; }

        public Peer(string addr, Int32 port)
        {
            this.addr = addr;
            this.port = port;
        }
    }
}
