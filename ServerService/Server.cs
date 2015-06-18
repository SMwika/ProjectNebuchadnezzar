using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SharedClasses;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.IO;
using System.Configuration;
using System.ServiceModel;

namespace ServerService
{
    /// <summary>
    /// Main server class
    /// </summary>
    class Server
    {
        private Thread _connectionThread;
        public static List<Thread> threadsList = new List<Thread>();
        public static Dictionary<Thread, ConfigPacket> configInjects = new Dictionary<Thread, ConfigPacket>();
        private System.Diagnostics.EventLog events;
        private DBConnect db;
        public static List<String> clientList = new List<String>();
        public static String[] validClients = System.Configuration.ConfigurationManager.AppSettings["validClientIPs"].Split(';');

        private String ip = ConfigurationManager.AppSettings["listenerIP"];//"127.0.0.1";
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["listenerPort"]);//9191;
        /// <summary>
        /// prepares distributed configuration injection to connected threads
        /// </summary>
        /// <param name="cp">configuraton to be injected</param>
        public static void InjectClientsConfiguration(ConfigPacket cp)
        {
            foreach (Thread t in threadsList)
            {
#if(DEBUG)
                Console.WriteLine("set cp: " + cp.ToString() + "on thread: " + t.ToString());
#endif
                configInjects.Add(t, cp);
            }
        }
        /// <summary>
        /// Serializes and sends given object via NetworkStream
        /// <seealso cref="System.Net.Sockets.NetworkStream"/>
        /// </summary>
        /// <param name="o">given object</param>
        /// <param name="s">socket to write to</param>
        private void SendObject(object o, Socket s)
        {
            IFormatter formatter = new BinaryFormatter();
            System.Net.Sockets.NetworkStream stream = new System.Net.Sockets.NetworkStream(s);
            formatter.Serialize(stream, o);
        }
        /// <summary>
        /// polls socket if endpoint is available
        /// <seealso cref="System.Net.Sockets.Socket"/>
        /// </summary>
        /// <param name="s">socket to check</param>
        /// <returns></returns>
        private bool IsSocketConnected(System.Net.Sockets.Socket s)
        {
            if (s == null) return false;
            bool poll = s.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
            bool avail = (s.Available == 0);
            if (poll && avail) return false;
            else return true;
        }
        /// <summary>
        /// receives serialized object from given NetworkStream
        /// <seealso cref="System.Net.Sockets.NetworkStream"/>
        /// </summary>
        /// <param name="sock">socket to read from</param>
        /// <returns>deserialized object</returns>
        private object ReceiveObject(Socket sock)
        {
            if (!sock.Connected) return null;
            NetworkStream stream = new NetworkStream(sock);
            IFormatter formatter = new BinaryFormatter();
            stream.ReadTimeout = 10000;
            try
            {
                object o = (object)formatter.Deserialize(stream);
                return o;
            }
            catch (SocketException)
            {
                return null;
            }
            catch (IOException e)
            {
                //if (e.InnerException is TimeoutException)
                //{
                //    Console.WriteLine("Timeout");
                //    return null;
                //}
                //if (e.InnerException is SocketException)
                //{
                //    IPEndPoint ipep = sock.RemoteEndPoint as IPEndPoint;
                //    String ip = ipep.Address.ToString();
                //    //new DBConnect().addLogs("Client " + ip + " forcibly closed connection");
                //    //events.WriteEntry("Client " + ip + " forcibly closed connection", System.Diagnostics.EventLogEntryType.Warning);
                //    return null;
                //}
            }
            return null;
        }
        /// <summary>
        /// Listener thread
        /// </summary>
        private void connectionThreadFunc()
        {
            //Packet pck = null;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
           // Console.WriteLine("Bind created");
            new DBConnect().addLogs("[" + ip + ":" + port + "] Bind created");
            listener.Listen(10);
            DBConnect db = new DBConnect();
#if(DEBUG)
            Console.WriteLine("[" + ip + ":" + port + "] Listening...");
#endif
            //db.addLogs("Listening");

            while(true){
                try
                {
                    Socket handler;
                    handler = listener.Accept();
                    String accIp = (handler.RemoteEndPoint as IPEndPoint).Address.ToString(); 
                    if (!validClients.Contains(accIp))
                    {
                        new DBConnect().addLogs("Tried to connect from restricted IP: " + accIp, 2);
                        handler.Send(new byte[] { 0xFF, 0xFE, 0xFD });
                        handler.Close();
                        continue;
                    }
                    handler.Send(new byte[] { 0xFF, 0xFF, 0xFF });
#if(DEBUG)
                    Console.WriteLine("Connected before Thread");
#endif
                    //if (isConnected) connector.SendLiverEvent("Connected");
                    Thread clientService = new Thread(() => clientServiceThreadFunc(handler));
                    clientService.Start();
                }
                catch (SocketException se)
                {
                    new DBConnect().addLogs(se.ToString());
                   // Console.WriteLine(se.ToString());
                }
            }
        }
        /// <summary>
        /// client thread from listener thread
        /// <seealso cref="connectionThreadFunc()"/>
        /// </summary>
        /// <param name="s">accepted socket from listener</param>
        private void clientServiceThreadFunc(Socket s)
        {
            threadsList.Add(Thread.CurrentThread);
            IPEndPoint ipep = s.RemoteEndPoint as IPEndPoint;
            String ip = ipep.Address.ToString();
            Packet helo = (Packet)ReceiveObject(s);
            clientList.Add(ip + " as " + helo.User);
            /* obsługa każdego klienta - odczyt obiektów z socketa (funkcja ReceiveObject(Socket sock) )*/
          //  Console.WriteLine("[" + ip + "]Connected in Thread");
            new DBConnect().addLogs("[" + ip + "]" + "Connected in Thread");
            while (true)
            {
                if (IsSocketConnected(s))
                {
                    if (configInjects.ContainsKey(Thread.CurrentThread))
                    {
#if(DEBUG)
                        Console.WriteLine("Injecting config: " + configInjects[Thread.CurrentThread]);
#endif
                        SendObject(configInjects[Thread.CurrentThread], s);
                        configInjects.Remove(Thread.CurrentThread);
                    }
                    Packet pck = null;
                    pck = (Packet)ReceiveObject(s);
                    if (pck == null)
                    {
                        continue;

                        s.Close();
                        break;
                    }
                    if (pck.IsExitPacket())
                    {
                        Console.WriteLine("Thread exited");
                        s.Close();
                        break;
                    }
#if(DEBUG)
                    Console.WriteLine(pck.getString());
#endif
                    db.addPacket(pck, ip);
                }
                else
                {
                    break;
                }
                
            }
           // Console.WriteLine("[" + ip + "]Thread Ended");
            new DBConnect().addLogs("[" + ip + "]Thread Ended");
            clientList.Remove(ip + " as " + helo.User);
            threadsList.Remove(Thread.CurrentThread);
        }

        private void WCFThreadFunc()
        {
            
        }
        public Server(System.Diagnostics.EventLog events)
        {
            this.events = events;
            Console.WriteLine(ip + ":" + port);
            db = new DBConnect();
            _connectionThread = new Thread(connectionThreadFunc);
            _connectionThread.Name = "Server connection thread";
            _connectionThread.Start();
        }


    }
}
