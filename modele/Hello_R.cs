using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class Hello_R : Hello
    {
        public Hello_R(string addr_source, Int32 port_source, List<Peer> pairs): base(addr_source, port_source, pairs, ECommunicationType.HELLO_R) { }
    }
}
