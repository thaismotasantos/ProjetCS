using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    class Hello
    {
        private readonly string _type = "HELLO";

        public Hello(string addr_source, string port_source, List<Peer> pairs)
        {
            this.addr_source = addr_source;
            this.port_source = addr_source;
            this.pairs = pairs;
        }

        public string type
        {
            get
            {
                return _type;
            }
        }
        public string addr_source { get; set; }
        public string port_source { get; set; }
        public List<Peer> pairs { get; set; }
    }
}
