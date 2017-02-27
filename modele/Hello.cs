using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class Hello : CommunicationType
    {
        public Hello(string addr_source, string port_source, List<Peer> pairs): base("HELLO")
        {
            this.addr_source = addr_source;
            this.port_source = addr_source;
            this.pairs = pairs;
        }
        
        [DataMember]
        public string addr_source { get; set; }
        [DataMember]
        public string port_source { get; set; }
        [DataMember]
        public List<Peer> pairs { get; set; }
    }
}
