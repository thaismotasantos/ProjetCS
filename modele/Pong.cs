﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class Pong : CommunicationType
    {
        public Pong(string addr_source, Int32 port_source, long timestamp): base(ECommunicationType.PONG)
        {
            this.addr_source = addr_source;
            this.port_source = port_source;
            this.timestamp = timestamp;
        }

        [DataMember]
        public string addr_source { get; set; }
        [DataMember]
        public Int32 port_source { get; set; }
        [DataMember]
        public long timestamp { get; set; }
    }
}
