using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class Goodbye : CommunicationType
    {
        public Goodbye(string addr): base("GOODBYE")
        {
            this.addr = addr;
        }
        
        [DataMember]
        public string addr { get; set; }
    }
}
