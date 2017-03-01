using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract(Name ="HELLO")]
    public class Hello : CommunicationType
    {
        public Hello()
        {

        }

        public Hello(string addr_source, Int32 port_source, List<Peer> pairs, string type): base(type)
        {
            this.addr_source = addr_source;
            this.port_source = port_source;
            this.pairs = pairs;
        }
        
        [DataMember]
        public string addr_source { get; set; }
        [DataMember]
        public Int32 port_source { get; set; }
        [DataMember]
        public List<Peer> pairs { get; set; }
    }
}
