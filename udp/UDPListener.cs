using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Projet.modele;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Projet.udp
{
    public class UDPListener
    {
        private string myAddress;
        private Int32 myPort;
        private Boolean isListening;
        private static Mutex mutex = new Mutex();
        private ChatUDPController chatUDPController;

        public UDPListener(string myAddress, Int32 myPort, ChatUDPController chatUDPController)
        {
            this.myAddress = myAddress;
            this.myPort = myPort;
            this.isListening = true;
            this.chatUDPController = chatUDPController;

            ThreadStart threadDelegate = new ThreadStart(listen);
            Thread thread = new Thread(threadDelegate);
            thread.Start();
        }

        private void listen()
        {
            // recuperer ip de la source

            // Initialisation
            UdpClient listener = new UdpClient(myPort);
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, myPort);

            while (this.isListening)
            {
                // Reception
                Debug.WriteLine("#############################");
                Debug.WriteLine("EP : " + ep);
                byte[] bytes = listener.Receive(ref ep);

                // Conversion
                String msg = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
                Debug.WriteLine("#############################");
                Debug.WriteLine("MGS : " + msg);

                readIncomingCommunication(msg);
            }
        }

        private void readIncomingCommunication(string msg)
        {
            mutex.WaitOne();

            var data = (JObject)JsonConvert.DeserializeObject(msg);
            Console.WriteLine(data["type"]);
            string type = data["type"].Value<string>();

            MemoryStream stream = GenerateStreamFromString(msg);
            stream.Position = 0;

            // TODO : faire vérification si communication est bien formée

            // vérifier type message
            if (type.Contains(ECommunicationType.HELLO))
            {
                Hello receivedHello = new Hello();
                List<Peer> listeNoeudsVoisins;
                if (type == ECommunicationType.HELLO_A)
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello_A));
                    receivedHello = (Hello_A)ser.ReadObject(stream);
                    listeNoeudsVoisins = receivedHello.pairs;

                    // si hello qui quelqu'un m'a envoyé, vérifier qu'il n'a pas été déjà reçu à moins de 10 sec
                    // si non, répondre le hello reçu avec la liste noeuds
                    //List<Peer> liste = helloSenders.Where(p => (p.Key.addr == receivedHello.addr_source)).Select(p => p.Key).ToList();
                    if (chatUDPController.isANewReceivedHello(receivedHello, type) /*liste.Count == 0*/)
                    {
                        //List<Peer> listeNoeudsVoisins = receivedHello.pairs;
                        /*if (receivedHello.GetType() == typeof(Hello_A))
                        {*/
                        Hello_R hello_r = new Hello_R(myAddress, myPort, chatUDPController.myNodes);
                        Peer peer_dest = new Peer(receivedHello.addr_source, receivedHello.port_source);
                        //chatUDPController.helloSenders.Add(peer_dest, DateTime.Now);
                        chatUDPController.receivedHelloAList.Add(peer_dest, DateTime.Now);
                        chatUDPController.sendHello(hello_r, peer_dest);

                        // A TRAITER : Sauf si un noeuds fait un HELLO avec une liste de paire vide (isolé) auquel cas on le rajoute automatiquement
                        // si source contient que moi dans sa liste de noeuds, l'ajouter à ma liste de noeuds (même que ça dépasse 4)
                        // si aucun noeud voisin ou 1 seul et moi même
                        chatUDPController.addPeerToCurrentNeighbors(peer_dest, listeNoeudsVoisins.Count == 0 || (listeNoeudsVoisins.Count == 1 && listeNoeudsVoisins[0].addr == myAddress));

                        /*if ((listeNoeudsVoisins.Count == 0
                            || (listeNoeudsVoisins.Count == 1 && listeNoeudsVoisins[0].addr == myAddress)) /*listeNoeudsVoisins.Where(p => (p.addr == adresse)).ToList().Count > 0)*
                            && !chatUDPController.myNodes.Contains(peer_dest))
                        {
                            chatUDPController.myNodes.Add(peer_dest);
                        }*/
                        //}

                        // récuperer sa liste de noeuds et ajouter à la sienne
                        //fillNeighbours(listeNoeudsVoisins, type);
                        listeNoeudsVoisins.ForEach(p => chatUDPController.addPeerToCurrentNeighbors(p, false));
                    }
                }
                else if (type == ECommunicationType.HELLO_R)
                {
                    if (chatUDPController.isANewReceivedHello(receivedHello, type))
                    {
                        DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Hello_R));
                        receivedHello = (Hello_R)ser.ReadObject(stream);
                        listeNoeudsVoisins = receivedHello.pairs;

                        Peer peer_dest = new Peer(receivedHello.addr_source, receivedHello.port_source);
                        chatUDPController.receivedHelloRList.Add(peer_dest, DateTime.Now);
                        //chatUDPController.addPeerToCurrentNeighbors(peer_dest, listeNoeudsVoisins.Count == 0 || (listeNoeudsVoisins.Count == 1 && listeNoeudsVoisins[0].addr == myAddress));

                        // récuperer sa liste de noeuds et ajouter à la sienne
                        //fillNeighbours(listeNoeudsVoisins, type);
                        listeNoeudsVoisins.ForEach(p => chatUDPController.addPeerToCurrentNeighbors(p, false));
                    }
                }
            }
            else if (type == ECommunicationType.MESSAGE)
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Message));
                Message message = (Message)ser.ReadObject(stream);

                // vérifier si message pas encore reçu
                if (chatUDPController.isANewReceivedMessage(message) /*sentAndReceivedMessages.Where(m => m == message.hash).ToList().Count == 0*/)
                {
                    treatReceivedMessage(message);
                }
            }
            else if (type == ECommunicationType.PING)
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Ping));
                Ping ping = (Ping)ser.ReadObject(stream);
                chatUDPController.sendPong(ping.addr_source, ping.port_source);
            }
            else if (type == ECommunicationType.PONG)
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Pong));
                Pong pong = (Pong)ser.ReadObject(stream);
                // TODO : ajouter réponse pong à ping correspondant
                // chatUDPController.
            }
            else if (type == ECommunicationType.GOODBYE)
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Goodbye));
                Goodbye goodbye = (Goodbye)ser.ReadObject(stream);

                // vérifier si goodbye pas encore reçu
                if (chatUDPController.isANewReceivedGoodbye(goodbye) /*sentAndReceivedMessages.Where(m => m == message.hash).ToList().Count == 0*/)
                {
                    chatUDPController.sentAndReceivedGoodbyes.Add(goodbye.nickname);
                    chatUDPController.sendReceivedGoodbye(goodbye);
                }
            }
            else
            {
                Console.WriteLine("Default case");
            }

            mutex.ReleaseMutex();
        }

        private void treatReceivedMessage(Message message)
        {
            chatUDPController.sentAndReceivedMessages.Add((message).hash);

            //chatUDPController.sendReceivedMessage(message);
            
            // si destinataire est canal principal ou canal pas privé
            if (message.destinataire.Equals(String.Empty)
                || !message.destinataire.Equals(String.Empty) && !message.destinataire.Contains('@'))
            {
                chatUDPController.sendReceivedMessage(message, message.destinataire);
            } // message pour moi
            else if(message.destinataire[0] == '@' && message.destinataire.Equals('@' + chatUDPController.myNickname))
            {
                chatUDPController.addMessageOnScreen(message, '@' + message.nickname);
                chatUDPController.addParticipant(message.nickname);
            } // envoyer message si destinataire different de moi
            else
            {
                chatUDPController.sendMessage(message);
                chatUDPController.addParticipant(message.nickname);
            }
        }

        /*private void fillNeighbours(List<Peer> listeNoeudsVoisins, string helloType)
        {
            // comparer pairs existants et reçus et ajouter aux existants les nouveaux jusqu'à 4
            //List<Peer> listeNouveauxNoeuds = listeNoeudsVoisins.Except(noeuds).ToList();
            List<Peer> listeNouveauxNoeuds = listeNoeudsVoisins.Where(x => !chatUDPController.myNodes.Any(y => y.addr == x.addr)).ToList();
            if (listeNouveauxNoeuds.Count > 0)
            {
                foreach (Peer p in listeNouveauxNoeuds)
                {
                    if (chatUDPController.myNodes.Count >= 4)
                    {
                        return;
                    }

                    if (p.addr != myAddress)
                    {
                        chatUDPController.myNodes.Add(p);
                    }
                }
            }

            // si quantité toujours plus petite que 4 envoyer un nouveau hello
            if (chatUDPController.myNodes.Count < 4)
            {
                List<Peer> listeNoeudsPasEncoreEnvoyes;
                // ?????????????????????????
                if (helloType == ECommunicationType.HELLO_A)
                {
                    //listeNoeudsPasEncoreEnvoyes = chatUDPController.myNodes.Except(chatUDPController.helloSenders.Select(p => p.Key)).ToList();
                    listeNoeudsPasEncoreEnvoyes = chatUDPController.myNodes.Except(chatUDPController.receivedHelloAList.Select(p => p.Key)).ToList();
                } else
                {
                    listeNoeudsPasEncoreEnvoyes = chatUDPController.myNodes.Except(chatUDPController.receivedHelloRList.Select(p => p.Key)).ToList();
                }
                //List<Peer> listeNoeudsPasEncoreEnvoyes = chatUDPController.myNodes.Except(chatUDPController.helloSenders.Select(p => p.Key)).ToList();
                if (listeNoeudsPasEncoreEnvoyes.Count > 0)
                {
                    //chatUDPController.startup();
                }
                else
                {
                    // attendre 10 sec et envoyer hello
                    // reinitialiser liste helloSenders
                }
            }
        }*/

        public static MemoryStream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public void stopListening()
        {
            this.isListening = false;
        }
    }
}
