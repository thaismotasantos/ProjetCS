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
        private string nickname;
        private string adressePremierDestinataire;
        private Int32 portePremierDestinataire;

        private List<Peer> helloReceivers; // liste de hello déjà reçus
        private List<string> sentAndReceivedMessages; // liste de hash des messages

        private List<string> participants;

        public ChatController()
        {
            adresse = "192.168.56.1"; // 10.154.127.235
            porte = 2323;
            nickname = "";
            adressePremierDestinataire = "";
            portePremierDestinataire = 2323;

            noeuds = new List<Peer>();
            chatrooms = new List<Chatroom>();
            helloReceivers = new List<Peer>();
            sentAndReceivedMessages = new List<string>();
            participants = new List<string>();

            noeuds.Add(new Peer(adressePremierDestinataire, portePremierDestinataire)); // 1 pair en dur
            chatrooms.Add(new Chatroom("")); // canal principal

            ThreadStart threadDelegate = new ThreadStart(listen);
            Thread thread = new Thread(threadDelegate);
            thread.Start();

            startup();
        }

        private void listen()
        {
            // recuperer ip de la source

            // Initialisation
            UdpClient listener = new UdpClient(porte);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, porte);

            while (true)
            {
                // Reception
                byte[] bytes = listener.Receive(ref ep);

                // Conversion
                String msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);

                incomingMessage(msg);
            }
        }

        private void incomingMessage(string msg)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CommunicationType));
            MemoryStream stream = new MemoryStream();
            stream.Position = 0;
            CommunicationType mc = (CommunicationType)ser.ReadObject(stream);

            // verifier type message
            switch(mc.type)
            {
                case ECommunicationType.HELLO:
                    // si hello, regarder si source du message est dans la liste de helloReceivers
                    // si oui, ne pas répondre et récuperer sa liste de noeuds et ajouter à la sienne
                    List<Peer> liste = helloReceivers.Where(p => (p.addr == ((Hello)mc).addr_source)).ToList();
                    if (liste.Count > 0)
                    {
                        List<Peer> listeNoeudsVoisins = ((Hello)mc).pairs;
                        fillNeighbours(listeNoeudsVoisins);
                    } // sinon, répondre le hello reçu avec la liste noeuds
                    else
                    {
                        // A TRAITER : Sauf si un noeuds fait un HELLO avec une liste de paire vide (isolé) auquel cas on le rajoute automatiquement
                        // si source contient que moi dans sa liste de noeuds, l'ajouter à ma liste de noeuds (même que ça dépasse 4)
                        Hello hello = new Hello(adresse, porte, noeuds);
                        Peer peer_dest = new Peer(((Hello)mc).addr_source, ((Hello)mc).port_source);
                        sendHello(hello, peer_dest);
                    }
                    break;
                case ECommunicationType.MESSAGE:
                    // vérifier si message pas encore reçu
                    if(sentAndReceivedMessages.Where(m => m == ((Message)mc).hash).ToList().Count == 0)
                    {
                        sendMessage((Message)mc);
                        sentAndReceivedMessages.Add(((Message)mc).hash);
                    }
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        private void broadcastMessage(Message message)
        {
            // ajouter son nom au message (rootedby)
            message.addToRootedBy(nickname);

            string msg = serialize(message);
            foreach (Peer p in noeuds)
            {
                // A FAIRE : NE PAS ENVOYER A L'EXPEDITEUR S'IL FAIT PARTIE DE MA LISTE DE NOEUDS
                sendCommunication(msg, p.addr, p.port);
            }
        }

        // méthode appelée lors du clique de l'utilisateur sur bouton send
        private void sendMessageFromUser()
        {
            // créer message et ajouter son nickname au rootedby du message
            // sendMessage(message);
            Message message = new Message(nickname, "", "", nickname);
            sentAndReceivedMessages.Add(message.hash);
        }

        private void sendMessage(Message message)
        {
            // vérifier le destinataire
            // si destinataire vide, canal principal => broadcast
            if (message.destinataire == String.Empty)
            {
                /*message.addToRootedBy(nickname);
                string msg = serialize(message);
                foreach (Peer p in noeuds)
                {
                    sendCommunication(msg, p.addr, p.port);
                }*/
                broadcastMessage(message);
            }
            else
            {
                // si destinataire n'est pas vide et contient @, donc message privé
                if (message.destinataire[0] == '@')
                {
                    // si nickname est le sien => pas de broadcast
                    if (message.nickname == nickname)
                    {
                        // vérifie si chatroom privé avec le nickname source existe déjà
                        // si non, crée un chatroom nommé "@nickname"
                        if (chatrooms.Where(c => c.name == ("@" + message.nickname)).ToList().Count == 0) {
                            chatrooms.Add(new Chatroom("@" + message.nickname));
                        }
                        // ajoute le message à liste du chatroom
                        chatrooms.Find(c => c.name == ("@" + message.nickname)).messages.Add(message);
                    } // si message privé pour quelqu'un d'autre
                      // broadcast
                    else
                    {
                        broadcastMessage(message);
                    }
                } // si destinataire n'est pas vide mais pas @, donc chatroom => broadcast
                else
                {
                    // si chatroom n'existe pas encore, créer
                    if (chatrooms.Where(c => c.name == (message.destinataire)).ToList().Count == 0)
                    {
                        chatrooms.Add(new Chatroom(message.destinataire));
                    }
                    // ajoute le message à liste du chatroom
                    chatrooms.Find(c => c.name == (message.destinataire)).messages.Add(message);

                    broadcastMessage(message);
                }
            }

            // si nickname source n'est pas dans liste nicknames (participants), l'ajouter
            if (participants.Find(p => p == message.nickname).ToList().Count == 0)
            {
                participants.Add(message.nickname);
            }
        }

        private void startup()
        {
            // envoie hello et attend liste de noeuds voisins. si toujours moins que 4 renvoie un autre hello à un des nouveaux noeuds.
            // si toujours moins que 4 attendre 10 sec

            Hello hello = new Hello(adresse, porte, noeuds);

            // convert into json
            foreach(Peer p in noeuds)
            {
                sendHello(hello, p);
                //sendMessage(serialize(hello), p.addr, p.port);
                //helloReceivers.Add(p);
            }
        }

        private void sendHello(Hello hello, Peer peer)
        {
            sendCommunication(serialize(hello), peer.addr, peer.port);
            helloReceivers.Add(peer);
        }

        private void sendCommunication(string message, string adresse_destinataire, Int32 porte_destinataire)
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

        private string serialize(CommunicationType comm)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(CommunicationType));
            MemoryStream stream = new MemoryStream();
            ser.WriteObject(stream, comm);
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
