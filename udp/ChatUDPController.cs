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
using System.Net.NetworkInformation;

namespace Projet.udp
{
    public class ChatUDPController
    {
        public MainWindow mainWindow { get; set; }

        public List<Peer> myNodes { get; set; } // max 4
        public List<Chatroom> chatrooms { get; set; }

        private string myAddress;
        private Int32 myPort;
        public string myNickname;
        private string addressFirstReceiver;
        private Int32 portFirstReceiver;

        private UDPListener udpListener;
        private UDPSender udpSender;

        public Dictionary<Peer, DateTime> helloSenders { get; set; } // liste de hello déjà reçus
        public Dictionary<Peer, DateTime> receivedHelloAList { get; set; } // liste de hello envoyés
        public Dictionary<Peer, DateTime> receivedHelloRList { get; set; } // liste de hello déjà reçus
        public List<string> sentAndReceivedMessages { get; set; } // liste de hash des messages
        public List<string> sentAndReceivedGoodbyes { get; set; } // liste de goodbyes envoyés et reçus
        
        public List<string> participants { get; set; }

        private System.Timers.Timer cleanHelloTimer;

        public ChatUDPController(string nickname, string addressFirstReceiver, string portFirstReceiver)
        {
            this.myAddress = getMyIPAddress(); // 192.168.1.17 10.154.126.249 
            this.myPort = 2324; //2323
            this.myNickname = nickname; //"thais";
            this.addressFirstReceiver = addressFirstReceiver; //"192.168.1.17"; // 10.154.127.244 10.154.127.235
            this.portFirstReceiver = Int32.Parse(portFirstReceiver); //2323;

            myNodes = new List<Peer>();
            chatrooms = new List<Chatroom>();
            helloSenders = new Dictionary<Peer, DateTime>();
            receivedHelloAList = new Dictionary<Peer, DateTime>();
            receivedHelloRList = new Dictionary<Peer, DateTime>();
            sentAndReceivedMessages = new List<string>();
            sentAndReceivedGoodbyes = new List<string>();
            participants = new List<string>();

            myNodes.Add(new Peer(this.addressFirstReceiver, this.portFirstReceiver)); // 1 pair en dur
            chatrooms.Add(new Chatroom("")); // canal principal

            this.udpListener = new UDPListener(this.myAddress, this.myPort, this);
            this.udpSender = new UDPSender(this.myNickname, this.myAddress, this.myPort, this);
            // send first hello communications
            startup();

            // envoie hello et attend liste de noeuds voisins. si toujours moins que 4 renvoie un autre hello à un des nouveaux noeuds.
            // si toujours moins que 4 attendre 10 sec
            /*Hello_A hello = new Hello_A(myAddress, myPort, myNodes);
            foreach (Peer p in myNodes)
            {
                sendHello(hello, p);
            }*/

            /*ThreadStart threadDelegate = new ThreadStart(listen);
            Thread thread = new Thread(threadDelegate);
            thread.Start();*/

            /*ThreadStart threadDelegateCleanHello = new ThreadStart(cleanHelloSendersLits);
            Thread threadCleanHello = new Thread(threadDelegateCleanHello);
            threadCleanHello.Start();*/
            InitTimer();

            //startup();
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
            //helloSenders = helloSenders.Where(s => ((DateTime.Now - s.Value).TotalSeconds < 10)).ToDictionary(s => s.Key, s => s.Value);
            receivedHelloAList = receivedHelloAList.Where(s => ((DateTime.Now - s.Value).TotalSeconds < 10)).ToDictionary(s => s.Key, s => s.Value);
            receivedHelloRList = receivedHelloRList.Where(s => ((DateTime.Now - s.Value).TotalSeconds < 10)).ToDictionary(s => s.Key, s => s.Value);
            startup();
            // TODO : faire ping pairs liste noeuds voisins
            sendPing();
        }

        public void sendHello(Hello hello, Peer peer)
        {
            udpSender.sendHello(hello, peer);
        }

        public void sendMessage(Message message)
        {
            // s'ajouter à la liste rootedBy
            message.addToRootedBy(myNickname);
            sentAndReceivedMessages.Add(message.hash);

            udpSender.sendMessage(message);
        }

        public void sendPing()
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            modele.Ping ping = new modele.Ping(myAddress, myPort, unixTimestamp);

            // TODO : créer dictionnaire pingpong (si un ping a sa réponse pong)
            // ajouter une nouvelle entrée dans le dict avec ping

            udpSender.sendPing(ping);
        }

        public void sendPong(string addr_source, Int32 port_source)
        {
            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            modele.Pong pong = new modele.Pong(myAddress, myPort, unixTimestamp);
            udpSender.sendPong(pong, addr_source, port_source);
        }

        public void sendGoodbye()
        {
            Goodbye goodbye = new Goodbye(myAddress, myNickname);
            sentAndReceivedGoodbyes.Add(goodbye.nickname);
            udpSender.sendGoodbye(goodbye);
        }

        public void sendReceivedGoodbye(Goodbye goodbye)
        {
            // retirer nickname from list participant
            if (participants.Any(p => p.Equals(goodbye.nickname)))
            {
                participants.Remove(goodbye.nickname);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindow.removeParticipant(goodbye.nickname)));
            }
            // broadcast goodbye
            udpSender.sendGoodbye(goodbye);
        }

        public void sendReceivedMessage(Message message, string chatName)
        {
            // s'ajouter à la liste rootedBy
            /*message.addToRootedBy(myNickname);

            udpSender.sendMessage(message);*/
            addMessageOnScreen(message , chatName);
            sendMessage(message);

            //System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindow.addMessageToChatroom(message)));
        }

        public void addMessageOnScreen(Message message, string chatName)
        {
            System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindow.addMessageToChatroom(message, chatName)));
        }

        public void addParticipant(string nickname)
        {
            // si nickname source n'est pas dans liste nicknames (participants), l'ajouter
            if (/*participants.Count == 0 || */!participants.Any(p => p.Equals(nickname)))
            {
                participants.Add(nickname);
                System.Windows.Application.Current.Dispatcher.Invoke((Action)(() => mainWindow.addParticipant(nickname)));
                // TODO : mettre a jour a liste de participants en mainwindow
            }
        }

        public void startup()
        {
            udpSender.startup();
        }

        private string getMyIPAddress()
        {
            /*ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            bool isWLANConnection = (InternetConnectionProfile == null) ? false : InternetConnectionProfile.IsWlanConnectionProfile;
            */
            //var profile = Windows.Networking.Connectivity.NetworkInformation.getInternetConnectionProfile();
            return GetAllLocalIPv4(NetworkInterfaceType.Wireless80211).FirstOrDefault();

            /*var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Adresse ip pas retrouvé");*/

            /*var hostName = Dns.GetHostEntry(Dns.GetHostName());
            var ipAddress = hostName.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork);
            if (ipAddress != null)
            {
                Console.WriteLine("Mon adress IP : " + ipAddress.ToString());
                return ipAddress.ToString();
            }
            throw new Exception("Adresse ip pas retrouvé");*/
        }

        private static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public Boolean isANewReceivedHello(Hello receivedHello, string helloType)
        {
            if(helloType == ECommunicationType.HELLO_A)
            {
                return receivedHelloAList.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList().Count == 0;
            } else 
            {
                return receivedHelloRList.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList().Count == 0;
            }
            //return helloSenders.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList().Count == 0;
        }

        public Boolean isANewReceivedMessage(Message message)
        {
            return !sentAndReceivedMessages.Any(m => m == message.hash); //.ToList().Count == 0;
        }

        public Boolean isANewReceivedGoodbye(Goodbye goodbye)
        {
            return !sentAndReceivedGoodbyes.Any(g => g == goodbye.nickname); //.ToList().Count == 0;
        }        

        public Boolean doesChatroomExist(string nickname)
        {
            return chatrooms.Where(c => c.name == ("@" + nickname)).ToList().Count == 0;
        }

        public void AddMessageToChatroom(string nickname, Message message)
        {
            chatrooms.Find(c => c.name == (nickname)).messages.Add(message);
        }

        public void addPeerToCurrentNeighbors(Peer peer, bool mustAdd)
        {
            // liste myNodes contient moins que 4 peers ou doit ajouter peer
            // peer n'existe pas dans la liste myNodes
            if ((this.myNodes.Count < 4 || mustAdd) && this.myNodes.FindAll(p => p.addr == peer.addr && p.port == peer.port).Count == 0)
            {
                this.myNodes.Add(peer);
                // ajouter peer dans liste mainwindow
            }
        }

        public void stop()
        {
            this.udpListener.stopListening();
        }

        /*private void listen()
        {
            // recuperer ip de la source

            // Initialisation
            UdpClient listener = new UdpClient(myPort);

            while (true)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, myPort);
                // Reception
                Debug.WriteLine("#############################");
                Debug.WriteLine("EP : " + ep);
                byte[] bytes = listener.Receive(ref ep);

                // Conversion
                String msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Debug.WriteLine("#############################");
                Debug.WriteLine("MGS : " + msg);

                incomingCommunication(msg);
            }
        }*/

        /*private void startup()
        {
            // envoie hello et attend liste de noeuds voisins. si toujours moins que 4 renvoie un autre hello à un des nouveaux noeuds.
            // si toujours moins que 4 attendre 10 sec

            Hello_A hello = new Hello_A(myAddress, myPort, myNodes);

            // convert into json
            foreach (Peer p in myNodes)
            {
                sendHello(hello, p);
                //sendMessage(serialize(hello), p.addr, p.port);
                //helloSenders.Add(p);
            }
        }*/

                /*private void sendHello(Hello hello, Peer peer)
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

                private void incomingCommunication(string msg)
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

                            // si hello qui quelqu'un m'a envoyé, vérifier qu'il n'a pas été déjà reçu à moins de 10 sec
                            // si non, répondre le hello reçu avec la liste noeuds
                            List<Peer> liste = helloSenders.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList();
                            if (liste.Count == 0)
                            {
                                List<Peer> listeNoeudsVoisins = receivedHello.pairs;
                                /*if (receivedHello.GetType() == typeof(Hello_A))
                                {
                                Hello_R hello_r = new Hello_R(myAddress, myPort, myNodes);
                                Peer peer_dest = new Peer(receivedHello.addr_source, receivedHello.port_source);
                                helloSenders.Add(peer_dest, DateTime.Now);
                                sendHello(hello_r, peer_dest);

                                // A TRAITER : Sauf si un noeuds fait un HELLO avec une liste de paire vide (isolé) auquel cas on le rajoute automatiquement
                                // si source contient que moi dans sa liste de noeuds, l'ajouter à ma liste de noeuds (même que ça dépasse 4)
                                // si aucun noeud voisin ou 1 seul et moi même
                                if ((listeNoeudsVoisins.Count == 0
                                    || (listeNoeudsVoisins.Count == 1 && listeNoeudsVoisins[0].addr == myAddress)) /*listeNoeudsVoisins.Where(p => (p.addr == adresse)).ToList().Count > 0)
                                    && !myNodes.Contains(peer_dest))
                                {
                                    myNodes.Add(peer_dest);
                                }
                                //}

                                // récuperer sa liste de noeuds et ajouter à la sienne
                                fillNeighbours(listeNoeudsVoisins);
                            }
                        }
                        else if (type == ECommunicationType.HELLO_R)
                        {
                            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello_R));
                            receivedHello = (Hello_R)ser.ReadObject(stream);
                        }
                    }
                    else if (type == ECommunicationType.MESSAGE)
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Message));
                        Message message = (Message)ser.ReadObject(stream);

                        // vérifier si message pas encore reçu
                        if (sentAndReceivedMessages.Where(m => m == message.hash).ToList().Count == 0)
                        {
                            sendMessage(message);
                            sentAndReceivedMessages.Add((message).hash);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Default case");
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

                private void broadcastMessage(Message message)
                {
                    // ajouter son nom au message (rootedby)
                    message.addToRootedBy(myNickname);

                    string msg = serialize(message);
                    foreach (Peer p in myNodes)
                    {
                        // A FAIRE : NE PAS ENVOYER A L'EXPEDITEUR S'IL FAIT PARTIE DE MA LISTE DE NOEUDS
                        sendCommunication(msg, p.addr, p.port);
                    }
                }

                // méthode appelée lors du clique de l'utilisateur sur bouton send
                public void sendMessageFromUser(Message msg)
                {
                    // créer message et ajouter son nickname au rootedby du message
                    // sendMessage(message);
                    sendMessage(msg);
                    sentAndReceivedMessages.Add(msg.hash);
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
                        }
                        broadcastMessage(message);
                    }
                    else
                    {
                        // si destinataire n'est pas vide et contient @, donc message privé
                        if (message.destinataire[0] == '@')
                        {
                            // si nickname est le sien => pas de broadcast
                            if (message.nickname == myNickname)
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
                    if (participants.Count == 0 || participants.Find(p => p == message.nickname).ToList().Count == 0)
                    {
                        participants.Add(message.nickname);
                    }
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
                    //List<Peer> listeNouveauxNoeuds = listeNoeudsVoisins.Except(noeuds).ToList();
                    List<Peer> listeNouveauxNoeuds = listeNoeudsVoisins.Where(x => !myNodes.Any(y => y.addr == x.addr)).ToList();
                    if (listeNouveauxNoeuds.Count > 0)
                    {
                        foreach(Peer p in listeNouveauxNoeuds)
                        {
                            if(myNodes.Count >= 4)
                            {
                                return;
                            }

                            if(p.addr != myAddress)
                            {
                                myNodes.Add(p);
                            }
                        }
                    }

                    // si quantité toujours plus petite que 4 envoyer un nouveau hello
                    if(myNodes.Count < 4)
                    {
                        // ?????????????????????????
                        List<Peer> listeNoeudsPasEncoreEnvoyes = myNodes.Except(helloSenders.Select(p => p.Key)).ToList();
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
                }*/
    }
}
