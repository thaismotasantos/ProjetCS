using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class PingPong : CommunicationType
    {
        public PingPong(string addr_source, long timestamp): base(ECommunicationType.PINGPONG)
        {
            this.addr_source = addr_source;
            this.timestamp = timestamp;
        }

        [DataMember]
        public string addr_source { get; set; }
        [DataMember]
        public long timestamp { get; set; }
    }
}
