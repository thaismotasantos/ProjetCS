using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Projet.modele
{
    public class Message
    {
        private readonly string _type = "MESSAGE";

        public Message(string nickname, string msg, string destinataire, string hash, string rootedby)
        {
            this.nickname = nickname;
            this.nickname = msg;
            this.timestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            this.nickname = destinataire;
            this.nickname = hash;
            this.nickname = rootedby;
        }

        public string type
        {
            get
            {
                return _type;
            }
        }
        public string nickname { get; set; } // ”LeB0G0sDu06”
        public string msg { get; set; }
        public long timestamp { get; set; } // ”131231231231”
        public string destinataire { get; set; } // vide si chan principal sinon nickname de la personne
        public string hash
        {
            get
            {
                StringBuilder sb = new StringBuilder()
                    .Append(type).Append(nickname).Append(msg).Append(timestamp).Append(destinataire);
                return sha256_hash(sb.ToString());
            }
        }
        public string rootedby { get; set; } // ”mathieu,jean,jacques” - liste de nicknames des gens auxquels le message a été déjà delivré

        public bool addToRootedBy(string nickname)
        {
            List<string> nicknames = rootedby.Split(',').ToList<string>();

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
