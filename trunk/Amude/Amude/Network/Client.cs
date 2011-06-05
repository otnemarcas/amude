/* 
 FATEC - Carapicuíba

 Trabalho de conclusão de curso - ASTI Jogos

 Gabriel Sacramento do Amaral
 Leandro Roque Razori
 Renato Ibanhez
 Sergio Alberto Pasqualino
 
 Outubro/2010
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using Amude.Core;
using Amude.Core.MessageProcessors;
using Amude.Domain;
using Amude.Network.Messages;
using Amude.Screen;
using Amude.Screen.Core;

namespace Amude.Network
{
    internal class Client
    {
        private int serverPort = 9050;
        private int clientPort = 9040;
        private int broadcastPort = 9090;

        private Dictionary<IPAddress, ServerNameMessage> servers;
        private IPAddress serverAddress;

        private Socket sockFindServers;
        private Socket sockListen;

        private Thread threadListen;
        private Thread threadFindServers;
        private Boolean shutdown;
        private AutoResetEvent resetEvent;
        private ClientMessageProcessor messageProcessor;
        private System.Timers.Timer timerPing;
        private Guid guid;
        private volatile Boolean accept;

        public DataArrival DataArrival;
        public ServerDiscovered ServerDiscovered;

        public Guid Guid
        {
            get { return guid; }
        }

        public DateTime LastPing { get; set; }

        public void FindServers(ServerDiscovered callback)
        {
            lock (this)
            {
                servers = new Dictionary<IPAddress, ServerNameMessage>();
                ServerDiscovered = callback;
                threadFindServers = new Thread(new ThreadStart(StartFindServers));
                threadFindServers.Start();
            }
        }

        public void SetServerAddress(IPAddress address)
        {
            serverAddress = address;
        }

        public void SendData(AbstractMessage message)
        {
            try
            {
                byte[] data, len;

                message.Guid = guid;

                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, message);
                memoryStream.Close();

                len = new byte[Core.RECEIVE_BUFFER_SIZE];
                data = memoryStream.ToArray();
                Encoding.ASCII.GetBytes(data.Length.ToString()).CopyTo(len, 0);

                Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                sock.Connect(serverAddress, serverPort);
                sock.Send(len);
                sock.Send(data);
                sock.Close();
            }
            catch
            {
                if (Controller.GetInstance().GetPlayers().Count > 1)
                {
                    ConnectionLost(Controller.GetInstance().GetRemotePlayer().Name);
                }
                else
                {
                    ConnectionLost(" o computador remoto ");
                }
            }
        }

        public void Start()
        {
            resetEvent = new AutoResetEvent(false);
            messageProcessor = new ClientMessageProcessor();
            DataArrival += new DataArrival(messageProcessor.DataArrival);
            if (Controller.GetInstance().IsServer)
            {
                messageProcessor.InitializeLocalPlayer();
            }
            Listen();

        }

        public void Stop()
        {
            StopPing();
            StopFindServers();
            StopListen();
        }

        public void StartPing()
        {
            timerPing = new System.Timers.Timer(5000);
            timerPing.Elapsed += new System.Timers.ElapsedEventHandler(PingElapsed);
            timerPing.Enabled = true;
            timerPing.Start();
        }

        public void StopPing()
        {
            if (timerPing != null)
            {
                timerPing.Enabled = false;
                timerPing.Stop();
                timerPing.Dispose();
                timerPing = null;
            }
        }

        private void PingElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            PingMessage pingMessage = new PingMessage();

            TimeSpan interval = DateTime.Now.Subtract(LastPing);

            if (interval.TotalSeconds > Core.PING)
            {
                if (Controller.GetInstance().GetPlayers().Count > 1)
                {
                    ConnectionLost(Controller.GetInstance().GetRemotePlayer().Name);
                }
                else
                {
                    ConnectionLost("Servidor");
                }
            }
        }

        private void ConnectionLost(String playerName)
        {
            String description = String.Format("Perda de conexão com {0}, verifique as conexões de rede.",
                                               playerName);

            ErrorScreen errorScreen = new ErrorScreen("Falha de Conexão",
                                                      description);

            Thread connectionLost = new Thread(new ParameterizedThreadStart(ShowConnectionLost));
            connectionLost.Start(errorScreen);
        }

        private void ShowConnectionLost(Object errorScreen)
        {
            Controller.GetInstance().SupressUpdate();

            Controller.GetInstance().QuitGame(false, false);
            ScreenManager.GetInstance().AddScreen((AbstractScreen)errorScreen);

            Controller.GetInstance().ReleaseUpdate();
        }

        private void Listen()
        {
            if (threadListen != null && threadListen.IsAlive)
            {
                threadListen.Abort();
                threadListen = null;
            }

            threadListen = new Thread(new ThreadStart(StartListen));
            threadListen.Start();
        }

        private void StartFindServers()
        {
            sockFindServers = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, broadcastPort);
            sockFindServers.Bind(ipEndPoint);
            EndPoint ep = (EndPoint)ipEndPoint;

            while (true)
            {
                lock (this)
                {
                    byte[] data = new byte[1024];
                    int receivedBytes = sockFindServers.ReceiveFrom(data, ref ep);

                    BinaryFormatter bf = new BinaryFormatter();
                    MemoryStream stream = new MemoryStream(data);

                    AbstractMessage message = (AbstractMessage)bf.Deserialize(stream);

                    if (message is ServerNameMessage)
                    {
                        ServerNameMessage serverName = (ServerNameMessage)message;

                        serverName.ServerAddress = IPAddress.Parse(ep.ToString().Remove(ep.ToString().IndexOf(':')));

                        if (!servers.ContainsKey(serverName.ServerAddress))
                        {
                            servers.Add(serverName.ServerAddress, serverName);
                            ServerDiscovered(serverName);
                        }
                    }
                    else if (message is CloseSocketMessage)
                    {
                        break;
                    }
                }
            }


        }

        public void StopFindServers()
        {
            if (threadFindServers != null)
            {
                // Enviar mensagem para liberar o Sock.Accept
                byte[] data;
                serverAddress = IPAddress.Loopback;

                MemoryStream memoryStream = new MemoryStream();
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(memoryStream, new CloseSocketMessage());
                memoryStream.Close();

                data = memoryStream.ToArray();

                Socket sock = new Socket(AddressFamily.InterNetwork,
                                         SocketType.Dgram,
                                         ProtocolType.Udp);
                sock.Connect(IPAddress.Loopback, broadcastPort);
                sock.Send(data);
                sock.Close();

                sockFindServers.Close();

                threadFindServers.Abort();
                threadFindServers = null;
            }
        }

        private void StartListen()
        {
            sockListen = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream,
                                     ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Any, clientPort);
            sockListen.Bind(iep);

            while (!shutdown)
            {
                byte[] len, data, temp;
                int total = 0;

                sockListen.Listen(10);
                
                accept = true;
                Socket incoming = sockListen.Accept();
                accept = false;

                len = new byte[Core.RECEIVE_BUFFER_SIZE];
                incoming.Receive(len);

                data = new byte[int.Parse(Encoding.ASCII.GetString(len))];
                temp = new byte[int.Parse(Encoding.ASCII.GetString(len))];

                while (total < data.Length)
                {
                    int received = incoming.Receive(temp);
                    Console.WriteLine("Bytes Recebidos: {0}", received);
                    Array.Copy(temp, 0, data, total, received);
                    total += received;
                }

                string address = incoming.RemoteEndPoint.ToString();
                address = address.Remove(address.IndexOf(':'));
                incoming.Close();

                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream stream = new MemoryStream(data);
                AbstractMessage message = (AbstractMessage)bf.Deserialize(stream);

                Console.WriteLine("Mensagem: {0}, Tamanho: {1}", message, total);

                if (guid == Guid.Empty)
                {
                    guid = message.Guid;
                }

                DataArrival(address, message);

            }

            resetEvent.Set();
        }

        private void StopListen()
        {
            if (threadListen != null && threadListen.IsAlive)
            {
                shutdown = true;

                if (accept)
                {

                    // Enviar mensagem para liberar o Sock.Accept
                    byte[] data, len;
                    serverAddress = IPAddress.Loopback;

                    MemoryStream memoryStream = new MemoryStream();
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, new CloseSocketMessage());
                    memoryStream.Close();

                    len = new byte[Core.RECEIVE_BUFFER_SIZE];
                    data = memoryStream.ToArray();
                    Encoding.ASCII.GetBytes(data.Length.ToString()).CopyTo(len, 0);

                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(IPAddress.Loopback, clientPort);
                    sock.Send(len);
                    sock.Send(data);
                    sock.Close();

                    // Aguarda sinal para fechar socket e finalizar a thread
                    resetEvent.WaitOne();
                }

                sockListen.Close();

            }
        }

    }
}
