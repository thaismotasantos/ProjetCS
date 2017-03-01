using Projet.modele;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Threading;
using System.Timers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        private Dictionary<Peer, DateTime> helloSenders; // liste de hello déjà reçus
        private List<string> sentAndReceivedMessages; // liste de hash des messages

        private List<string> participants;

        private System.Timers.Timer cleanHelloTimer;

        public ChatController()
        {
            adresse = "10.154.127.235";
            porte = 2323;
            nickname = "thais";
            adressePremierDestinataire = "10.154.127.235";
            portePremierDestinataire = 2323;

            noeuds = new List<Peer>();
            chatrooms = new List<Chatroom>();
            helloSenders = new Dictionary<Peer, DateTime>();
            sentAndReceivedMessages = new List<string>();
            participants = new List<string>();

            noeuds.Add(new Peer(adressePremierDestinataire, portePremierDestinataire)); // 1 pair en dur
            chatrooms.Add(new Chatroom("")); // canal principal

            ThreadStart threadDelegate = new ThreadStart(listen);
            Thread thread = new Thread(threadDelegate);
            thread.Start();

            /*ThreadStart threadDelegateCleanHello = new ThreadStart(cleanHelloSendersLits);
            Thread threadCleanHello = new Thread(threadDelegateCleanHello);
            threadCleanHello.Start();*/
            InitTimer();

            startup();
        }

        private void InitTimer()
        {
            cleanHelloTimer = new System.Timers.Timer();
            cleanHelloTimer.Elapsed += new ElapsedEventHandler(cleanHelloTimer_Tick);
            cleanHelloTimer.Interval = 10000;
            cleanHelloTimer.Enabled = true;

            //cleanHelloTimer.Start();
            /*var aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 1000;
            aTimer.Enabled = true;*/
        }

        private void cleanHelloTimer_Tick(object sender, ElapsedEventArgs e)
        {
            helloSenders = helloSenders.Where(s => ((DateTime.Now - s.Value).TotalSeconds < 10)).ToDictionary(s => s.Key, s => s.Value);
        }

        private void listen()
        {
            // recuperer ip de la source

            // Initialisation
            UdpClient listener = new UdpClient(porte);

            while (true)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, porte);
                // Reception
                Debug.WriteLine("#############################");
                Debug.WriteLine("EP : " + ep);
                byte[] bytes = listener.Receive(ref ep);

                // Conversion
                String msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Debug.WriteLine("#############################");
                Debug.WriteLine("MGS : " + msg);

                incomingMessage(msg);
            }
        }

        public static MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private void incomingMessage(string msg)
        {
            

            var data = (JObject)JsonConvert.DeserializeObject(msg);
            Console.WriteLine(data["type"]);
            string type = data["type"].Value<string>();

            MemoryStream stream = GenerateStreamFromString(msg);
            stream.Position = 0;

            // vérifier type message
            if (type.Contains(ECommunicationType.HELLO))
            {
                Hello receivedHello = new Hello();
                if (type == ECommunicationType.HELLO_A)
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello_A));
                    receivedHello = (Hello_A)ser.ReadObject(stream);
                }
                else if (type == ECommunicationType.HELLO_R)
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello_R));
                    receivedHello = (Hello_R)ser.ReadObject(stream);
                }

                // si hello qui quelqu'un m'a envoyé, vérifier qu'il n'a pas été déjà reçu à moins de 10 sec
                // si non, répondre le hello reçu avec la liste noeuds
                List<Peer> liste = helloSenders.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList();
                if (liste.Count == 0)
                {
                    List<Peer> listeNoeudsVoisins = receivedHello.pairs;
                    if (receivedHello.GetType() == typeof(Hello_A))
                    {
                        helloSenders.Add(new Peer(receivedHello.addr_source, receivedHello.port_source), DateTime.Now);

                        Hello_R hello_r = new Hello_R(adresse, porte, noeuds);
                        Peer peer_dest = new Peer(receivedHello.addr_source, receivedHello.port_source);
                        sendHello(hello_r, peer_dest);

                        // A TRAITER : Sauf si un noeuds fait un HELLO avec une liste de paire vide (isolé) auquel cas on le rajoute automatiquement
                        // si source contient que moi dans sa liste de noeuds, l'ajouter à ma liste de noeuds (même que ça dépasse 4)
                        // si aucun noeud voisin ou 1 seul et moi même
                        if (listeNoeudsVoisins.Count == 0 
                            || (listeNoeudsVoisins.Count == 1 && listeNoeudsVoisins.Where(p => (p.addr == adresse)).ToList().Count > 0))
                        {
                            noeuds.Add(peer_dest);
                        }
                    }

                    // récuperer sa liste de noeuds et ajouter à la sienne
                    fillNeighbours(listeNoeudsVoisins);
                }
            }
            else if (msg.Contains(ECommunicationType.MESSAGE))
            {
                // vérifier si message pas encore reçu
                /*if(sentAndReceivedMessages.Where(m => m == ((Message)mc).hash).ToList().Count == 0)
                {
                    sendMessage((Message)mc);
                    sentAndReceivedMessages.Add(((Message)mc).hash);
                }*/
            }
            else
            {
                Console.WriteLine("Default case");
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

            Hello_A hello = new Hello_A(adresse, porte, noeuds);

            // convert into json
            foreach(Peer p in noeuds)
            {
                sendHello(hello, p);
                //sendMessage(serialize(hello), p.addr, p.port);
                //helloSenders.Add(p);
            }
        }

        private void sendHello(Hello hello, Peer peer)
        {
            sendCommunication(serialize(hello), peer.addr, peer.port);
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
            DataContractJsonSerializer ser;

            if(comm.GetType() == typeof(Hello))
            {
                ser = new DataContractJsonSerializer(typeof(Hello));
            }
            else if (comm.GetType() == typeof(Hello_A))
            {
                ser = new DataContractJsonSerializer(typeof(Hello_A));
            }
            else if (comm.GetType() == typeof(Hello_R))
            {
                ser = new DataContractJsonSerializer(typeof(Hello_R));
            }
            else if(comm.GetType() == typeof(Message))
            {
                ser = new DataContractJsonSerializer(typeof(Message));
            }
            else if (comm.GetType() == typeof(PingPong))
            {
                ser = new DataContractJsonSerializer(typeof(PingPong));
            }
            else if (comm.GetType() == typeof(Goodbye))
            {
                ser = new DataContractJsonSerializer(typeof(Goodbye));
            }
            else
            {
                ser = new DataContractJsonSerializer(typeof(Goodbye));
            }

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
                List<Peer> listeNoeudsPasEncoreEnvoyes = noeuds.Except(helloSenders.Select(p => p.Key)).ToList();
                if (listeNoeudsPasEncoreEnvoyes.Count > 0)
                {
                    startup();
                }
                else
                {
                    // attendre 10 sec et envoyer hello
                    // reinitialiser liste helloSenders
                }
            }
        }
    }
}
