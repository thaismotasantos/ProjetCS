using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class CommunicationType
    {
        [DataMember]
        public string type { get; set; }

        public CommunicationType() { }

        public CommunicationType(string type)
        {
            this.type = type;
        }
    }    
}
