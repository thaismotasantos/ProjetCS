using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    class Goodbye
    {
        private readonly string _type = "GOODBYE";

        public Goodbye(string addr)
        {
            this.addr = addr;
        }

        public string type
        {
            get
            {
                return _type;
            }
        }
        public string addr { get; set; }
    }
}
