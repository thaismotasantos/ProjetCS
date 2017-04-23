using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    [DataContract]
    public class Message : CommunicationType
    {

        public Message(string nickname, string msg, string destinataire/*, string rootedby*/): base(ECommunicationType.MESSAGE)
        {
            this.nickname = nickname;
            this.msg = msg;
            this.timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            this.destinataire = destinataire;
            this.rootedby = ""; // rootedby;
        }

        [DataMember]
        public string nickname { get; set; } // ”LeB0G0sDu06”
        [DataMember]
        public string msg { get; set; }
        [DataMember]
        public long timestamp { get; set; } // ”131231231231”
        [DataMember]
        public string destinataire { get; set; } // vide si chan principal sinon nickname de la personne
        [DataMember]
        public string hash
        {
            get
            {
                StringBuilder sb = new StringBuilder()
                    .Append(type).Append(nickname).Append(msg).Append(timestamp).Append(destinataire);
                return sha256_hash(sb.ToString());
            }
            set
            {
            }
        }
        [DataMember]
        public string rootedby { get; set; } // ”mathieu,jean,jacques” - liste de nicknames des gens auxquels le message a été déjà delivré

        public bool addToRootedBy(string nickname)
        {
            List<string> nicknames;
            if (!String.IsNullOrEmpty(rootedby))
            {
                nicknames = rootedby.Split(',').ToList<string>();
            } else
            {
                nicknames = new List<string>();
            }            

            if(!nicknames.Any())
            {
                rootedby += nickname;
                return true;
            }

            if(!nicknames.Contains(nickname))
            {
                rootedby += "," + nickname;
                return true;
            }

            return false;
        }

        private string sha256_hash(string value)
        {
            StringBuilder sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    sb.Append(b.ToString("x2"));
            }

            return sb.ToString();
        }
    }

}
