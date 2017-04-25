using Projet.modele;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Projet.udp
{
    public class UDPSender
    {
        private string myNickname;
        private string myAddress;
        private Int32 myPort;
        //private static Mutex mutex = new Mutex();
        private ChatUDPController chatUDPController;

        public UDPSender(string myNickname, string myAddress, Int32 myPort, ChatUDPController chatUDPController)
        {
            this.myNickname = myNickname;
            this.myAddress = myAddress;
            this.myPort = myPort;
            this.chatUDPController = chatUDPController;

            //startup();
        }

        public void startup()
        {
            // envoie hello et attend liste de noeuds voisins. si toujours moins que 4 renvoie un autre hello à un des nouveaux noeuds.
            // si toujours moins que 4 attendre 10 sec

            Hello_A hello = new Hello_A(myAddress, myPort, chatUDPController.MyNodes);

            // convert into json
            foreach (Peer p in chatUDPController.MyNodes)
            {
                sendHello(hello, p);
                //sendMessage(serialize(hello), p.addr, p.port);
                //helloSenders.Add(p);
            }
        }

        public void sendHello(Hello hello, Peer peer)
        {
            sendCommunication(serialize(hello), peer.addr, peer.port);
        }

        // méthode appelée lors du clique de l'utilisateur sur bouton send
        /*public void sendMessageFromUser(Message msg)
        {
            // créer message et ajouter son nickname au rootedby du message
            // sendMessage(message);
            sendMessage(msg);
        }*/

        public void sendMessage(Message message)
        {
            broadcastMessage(message);
            // vérifier le destinataire
            // si destinataire vide, canal principal => broadcast
            /*if (message.destinataire.Equals(String.Empty) || )
            {
                /*message.addToRootedBy(nickname);
                string msg = serialize(message);
                foreach (Peer p in noeuds)
                {
                    sendCommunication(msg, p.addr, p.port);
                }*
                broadcastMessage(message);
            }
            else
            {
                // si destinataire n'est pas vide et contient @, donc message privé
                if (message.destinataire[0] == '@')
                {
                    // si nickname est le sien (message pour moi) => pas de broadcast
                    if (message.destinataire.Substring(1, message.destinataire.Length - 1).Equals(myNickname))
                    {
                        // vérifie si chatroom privé avec le nickname source existe déjà
                        // si non, crée un chatroom nommé "@nickname"
                        if (!chatUDPController.doesChatroomExist(message.nickname) /*.chatrooms.Where(c => c.name == ("@" + message.nickname)).ToList().Count == 0*)
                        {
                            chatUDPController.chatrooms.Add(new Chatroom("@" + message.nickname));
                        }
                        // ajoute le message à liste du chatroom
                        chatUDPController.AddMessageToChatroom("@" + message.nickname, message); /*chatrooms.Find(c => c.name == ("@" + message.nickname)).messages.Add(message);*
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
                    if (!chatUDPController.doesChatroomExist(message.destinataire) /*chatrooms.Where(c => c.name == (message.destinataire)).ToList().Count == 0*)
                    {
                        chatUDPController.chatrooms.Add(new Chatroom(message.destinataire));
                    }
                    // ajoute le message à liste du chatroom
                    chatUDPController.AddMessageToChatroom(message.destinataire, message);
                    // chatrooms.Find(c => c.name == (message.destinataire)).messages.Add(message);

                    broadcastMessage(message);
                }
            }

            // si nickname source n'est pas dans liste nicknames (participants), l'ajouter
            //chatUDPController.addParticipant(message.nickname);
            /*if (chatUDPController.participants.Count == 0 || chatUDPController.participants.Find(p => p == message.nickname).ToList().Count == 0)
            {
                chatUDPController.participants.Add(message.nickname);
            }*/
        }

        public void sendPing(Ping ping, Peer peer)
        {
            string comm = serialize(ping);
            sendCommunication(comm, peer.addr, peer.port);
            /*chatUDPController.MyNodes.ForEach(p => {  /*chatUDPController.sentPings.Add(p.addr + " " + p.port, false);* });
            chatUDPController.ticktockPong();*/
        }

        public void sendPong(Pong pong, string addr_source, Int32 port_source)
        {
            string comm = serialize(pong);
            sendCommunication(comm, addr_source, port_source);
        }

        public void sendGoodbye(Goodbye goodbye)
        {
            string gb = serialize(goodbye);
            chatUDPController.MyNodes.ForEach(p => sendCommunication(gb, p.addr, p.port));
            /*foreach (Peer p in chatUDPController.myNodes)
            {
                sendCommunication(gb, p.addr, p.port);
            }*/
        }

        private void broadcastMessage(Message message)
        {
            // ajouter son nom au message (rootedby)
            //message.addToRootedBy(myNickname);

            string msg = serialize(message);
            foreach (Peer p in chatUDPController.MyNodes)
            {
                // A FAIRE : NE PAS ENVOYER A L'EXPEDITEUR S'IL FAIT PARTIE DE MA LISTE DE NOEUDS
                //if(p.addr == message.a)
                sendCommunication(msg, p.addr, p.port);
            }
        }

        private string serialize(CommunicationType comm)
        {
            DataContractJsonSerializer ser;

            if (comm.GetType() == typeof(Hello))
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
            else if (comm.GetType() == typeof(Message))
            {
                ser = new DataContractJsonSerializer(typeof(Message));
            }
            else if (comm.GetType() == typeof(Ping))
            {
                ser = new DataContractJsonSerializer(typeof(Ping));
            }
            else if (comm.GetType() == typeof(Pong))
            {
                ser = new DataContractJsonSerializer(typeof(Pong));
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
    }
}
