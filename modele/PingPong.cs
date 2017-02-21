using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    class PingPong
    {
        private readonly string _type = "PING/PONG";

        public PingPong(string addr_source, long timestamp)
        {
            this.addr_source = addr_source;
            this.timestamp = timestamp;
        }

        public string type
        {
            get
            {
                return _type;
            }
        }
        public string addr_source { get; set; }
        public long timestamp { get; set; }
    }
}
