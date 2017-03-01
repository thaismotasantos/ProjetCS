using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract(Name ="pair")]
    public class Peer
    {

        public Peer(){} // constructeur par défaut

        [DataMember]
        public string addr { get; set; }
        [DataMember]
        public Int32 port { get; set; }

        public Peer(string addr, Int32 port)
        {
            this.addr = addr;
            this.port = port;
        }
    }
}
