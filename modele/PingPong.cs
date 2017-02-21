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

        public string type
        {
            get
            {
                return _type;
            }
        }
    }
}
