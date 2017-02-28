using Projet.modele;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Linq;

namespace Projet
{
    public class ChatController
    {
        private List<Peer> noeuds; // max 4
        private List<Chatroom> chatrooms;

        private string adresse;
        private Int32 porte;
        private string adressePremierDestinataire;
        private Int32 portePremierDestinataire;

        private List<Peer> helloReceivers;

        public ChatController()
        {
            adresse = "";
            porte = 2323;
            adressePremierDestinataire = "";
            portePremierDestinataire = 2323;

            noeuds.Add(new Peer(adressePremierDestinataire, portePremierDestinataire)); // 1 pair en dur

            ThreadStart threadDelegate = new ThreadStart(listen);
            Thread thread = new Thread(threadDelegate);
            thread.Start();

            startup();
        }

        private void listen()
        {
            //Initialisation
            UdpClient listener = new UdpClient(porte);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, porte);

            //Reception
            byte[] bytes = listener.Receive(ref ep);

            //Conversion
            String msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

            incomingMessage(msg);
        }

        private void incomingMessage(string msg)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CommunicationType));
            MemoryStream stream = new MemoryStream();
            stream.Position = 0;
            CommunicationType mc = (CommunicationType)ser.ReadObject(stream);

            // verifier type message
            /*switch(mc.type)
            {
                case CommunicationType.HELLO:
                    // si hello, regarder si source du message est dans la liste de helloReceivers
                    // si oui, ne pas répondre et récupere sa liste de noeuds et ajouter à la sienne
                    List<Peer> liste = helloReceivers.Where(p => (p.addr == ((Hello)mc).addr_source)).ToList();
                    if (liste.Count == 0)
                    {
                        List<Peer> listeNoeudsVoisins = ((Hello)mc).pairs;
                        fillNeighbours(listeNoeudsVoisins);
                    }

                    // sinon, répondre le hello reçu avec la liste noeuds
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }*/
        }

        private void startup()
        {
            // envoie hello et attend liste de noeuds voisins. si toujours moins que 4 renvoie un autre hello à un des nouveaux noeuds.
            // si toujours moins que 4 attendre 10 sec

            /*Hello hello = new Hello(adresse, porte, noeuds);

            // convert into json
            foreach(Peer p in noeuds)
            {
                sendMessage(serialize(hello), p.addr, p.port);
                helloReceivers.Add(p);
            }*/
        }

        private void sendMessage(string message, string adresse_destinataire, Int32 porte_destinataire)
        {
            //Initialisation
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPAddress target = IPAddress.Parse(adresse_destinataire);
            IPEndPoint ep = new IPEndPoint(target, porte_destinataire);

            //Conversion
            byte[] msg = Encoding.ASCII.GetBytes(message);

            //Envoi
            s.SendTo(msg, ep);
        }

        private string serialize(Hello hello) // substituer par classe mère
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello));
            MemoryStream stream = new MemoryStream();
            ser.WriteObject(stream, hello);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        private void fillNeighbours(List<Peer> listeNoeudsVoisins)
        {
            // comparer pairs existants et reçus et ajouter aux existants les nouveaux jusqu'à 4
            List<Peer> listeNouveauxNoeuds = listeNoeudsVoisins.Except(noeuds).ToList();
            if(listeNouveauxNoeuds.Count > 0)
            {
                foreach(Peer p in listeNouveauxNoeuds)
                {
                    if(noeuds.Count < 4)
                    {
                        noeuds.Add(p);
                    }
                    else
                    {
                        return;
                    }
                }
            }

            // si quantité toujours plus petite que 4 envoyer un nouveau 
            if(noeuds.Count < 4)
            {
                List<Peer> listeNoeudsPasEncoreEnvoyes = noeuds.Except(helloReceivers).ToList();
                if (listeNoeudsPasEncoreEnvoyes.Count > 0)
                {
                    startup();
                }
                else
                {
                    // attendre 10 sec et envoyer hello
                    // reinitialiser liste helloReceivers
                }
            }
        }
    }
}
