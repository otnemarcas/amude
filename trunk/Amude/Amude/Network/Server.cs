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
    internal class Server
    {
        private volatile bool shutdown = false;
        private int serverPort = 9050;
        private int clientPort = 9040;
        private int broadcastPort = 9090;
        private bool broadcast = false;

        private Thread threadBroadcast;
        private Thread threadListen;

        private Socket sockBroadcast;
        private Socket sockListen;

        private AutoResetEvent resetEvent;
        private ServerMessageProcessor messageProcessor;
        private AutoResetEvent resetBroadcast;

        private System.Timers.Timer timerPing;
        private Dictionary<IPAddress, DateTime> lastPing;
        private Guid guid;
        private volatile bool accept;

        public DataArrival DataArrival;
        
        public Dictionary<IPAddress, DateTime> LastPing 
        {
            get { return lastPing; }
        }

        public Guid Guid
        {
            get { return guid; }
        }

        public Server()
        {
            guid = Guid.NewGuid();
        }

        public void Start()
        {
            resetEvent = new AutoResetEvent(false);
            messageProcessor = new ServerMessageProcessor();
            lastPing = new Dictionary<IPAddress, DateTime>();
            DataArrival += new DataArrival(messageProcessor.DataArrival);
            Listen();
            SendBroadcast();            
        }

        public void Stop()
        {
            StopSendBroadCast();
            StopPing();
            StopListen();
        }

        public void SendData(IPAddress address, AbstractMessage message)
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
                sock.Connect(address, clientPort);
                sock.Send(len);
                sock.Send(data);
                sock.Close();
            }
            catch
            {
                ConnectionLost(Controller.GetInstance().GetPlayer(address));
            }
        }

        public void SendToAll(AbstractMessage message)
        {
            foreach (Player player in Controller.GetInstance().GetPlayers())
            {
                SendData(player.IPAddress, message);
            }
        }

        private void SendBroadcast()
        {
            threadBroadcast = new Thread(new ThreadStart(StartSendBroadCast));
            threadBroadcast.Start();
        }

        private void StartSendBroadCast()
        {
            broadcast = true;
            resetBroadcast = new AutoResetEvent(false);

            sockBroadcast = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Broadcast, broadcastPort);

            string hostname = Dns.GetHostName();
            string playerName = Controller.GetInstance().GetLocalPlayer().Name;

            MemoryStream fs = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, new ServerNameMessage(hostname, playerName));
            fs.Close();

            sockBroadcast.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sockBroadcast.EnableBroadcast = true;
            try
            {
                while (broadcast)
                {
                    sockBroadcast.SendTo(fs.ToArray(), ipEndPoint);
                    Thread.Sleep(500);
                }
            }
            catch
            {
                ConnectionLost("Erro ao criar jogo, verifique as conexões de rede.");
            }
            
            resetBroadcast.Set();

        }

        public void StopSendBroadCast()
        {
            if (threadBroadcast != null && threadBroadcast.IsAlive)
            {
                broadcast = false;
                resetBroadcast.WaitOne();

                sockBroadcast.Close();
                threadBroadcast.Abort();
            }
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

            foreach (Player player in Controller.GetInstance().GetPlayers())
            {
                if (player.Name != Controller.GetInstance().GetLocalPlayer().Name)
                {
                    if (!lastPing.ContainsKey(player.IPAddress))
                    {
                        lastPing.Add(player.IPAddress, DateTime.Now);
                        try
                        {
                            SendData(player.IPAddress, pingMessage);
                        }
                        catch
                        {
                            ConnectionLost(player);
                        }
                    }
                    else
                    {
                        DateTime last = lastPing[player.IPAddress];
                        TimeSpan interval = DateTime.Now.Subtract(last);

                        if (interval.TotalSeconds > Core.PING)
                        {
                            ConnectionLost(player);
                        }
                        else
                        {
                            try
                            {
                                SendData(player.IPAddress, pingMessage);
                            }
                            catch
                            {
                                ConnectionLost(player);
                            }
                        }

                    }
                    Console.WriteLine("Ip: {0}, Hora: {1}", player.IPAddress.ToString(), lastPing[player.IPAddress]);
                }
            }
        }

        private void ConnectionLost(Player player)
        {
            lock (this)
            {
                if (timerPing != null)
                {
                    String description = String.Format("Não foi possível se conectar a {0}, verifique as conexões de rede.",
                                               player.Name);

                    ErrorScreen errorScreen = new ErrorScreen("Falha de Conexão",
                                                              description);

                    Thread connectionLost = new Thread(new ParameterizedThreadStart(ShowConnectionLost));
                    connectionLost.Start(errorScreen);

                }
            }
        }

        private void ConnectionLost(String message)
        {
            lock (this)
            {
                if (timerPing != null)
                {
                    ErrorScreen errorScreen = new ErrorScreen("Falha de Conexão", message);
                    Thread connectionLost = new Thread(new ParameterizedThreadStart(ShowConnectionLost));
                    connectionLost.Start(errorScreen);
                }
            }
        }

        private void ShowConnectionLost(Object errorScreen)
        {
            Controller.GetInstance().SupressUpdate();

            Controller.GetInstance().QuitGame(false, false);
            ScreenManager.GetInstance().AddScreen((AbstractScreen)errorScreen);
            Controller.GetInstance().StopServer();
            Controller.GetInstance().StopClient();

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

        private void StartListen()
        {
            sockListen = new Socket(AddressFamily.InterNetwork,
                                     SocketType.Stream,
                                     ProtocolType.Tcp);

            IPEndPoint iep = new IPEndPoint(IPAddress.Any, serverPort);
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

                DataArrival(address, message);

            }

            resetEvent.Set();
        }

        private void StopListen()
        {
            shutdown = true;

            if (threadListen != null && threadListen.IsAlive)
            { 
                if (accept)
                {
                    // Enviar mensagem para liberar o Sock.Accept
                    byte[] data, len;

                    MemoryStream memoryStream = new MemoryStream();
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(memoryStream, new CloseSocketMessage());
                    memoryStream.Close();

                    len = new byte[Core.RECEIVE_BUFFER_SIZE];
                    data = memoryStream.ToArray();
                    Encoding.ASCII.GetBytes(data.Length.ToString()).CopyTo(len, 0); ;

                    Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    sock.Connect(IPAddress.Loopback, serverPort);
                    sock.Send(len);
                    sock.Send(data);
                    sock.Close();

                    resetEvent.WaitOne();
                }                

                sockListen.Close();
            }
        }

    }

}