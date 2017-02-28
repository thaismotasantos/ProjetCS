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
        public Goodbye(string addr, string nickname): base(ECommunicationType.GOODBYE)
        {
            this.addr = addr;
            this.nickname = nickname;
        }
        
        [DataMember]
        public string addr { get; set; }
        [DataMember]
        public string nickname { get; set; }
    }
}
