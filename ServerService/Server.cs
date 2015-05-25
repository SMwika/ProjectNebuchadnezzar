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

namespace ServerService
{
    class Server
    {
        private Thread _connectionThread;
        private Thread _clientServiceThread;

        private String ip = "127.0.0.1";
        private int port = 9191;

        private object ReceiveObject(Socket sock)
        {
            if (!sock.Connected) return null;
            NetworkStream stream = new NetworkStream(sock);
            IFormatter formatter = new BinaryFormatter();
            try
            {
                object o = (object)formatter.Deserialize(stream);
                return o;
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
            return null;
        }

        private void connectionThreadFunc()
        {
            //Packet pck = null;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            Console.WriteLine("Bind created");
            listener.Listen(10);
            Console.WriteLine("Listening...");

            while(true){
                try
                {
                    Socket handler;
                    handler = listener.Accept();
                    Console.WriteLine("Connected before Thread");
                    Thread clientService = new Thread(() => clientServiceThreadFunc(handler));
                    clientService.Start();
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        private void clientServiceThreadFunc(Socket s)
        {
            IPEndPoint ipep = s.RemoteEndPoint as IPEndPoint;
            String ip = ipep.Address.ToString();
            /* obsługa każdego klienta - odczyt obiektów z socketa (funkcja ReceiveObject(Socket sock) )*/
            Console.WriteLine("Connected in Thread");
            while (true)
            {
                Packet pck = null;
                pck = (Packet)ReceiveObject(s);
                if (pck == null)
                {
                    s.Close();
                    return;
                }
                Console.WriteLine(pck.getString());
                DBConnect db = new DBConnect();
                db.addPacket(pck, ip);
            }
        }
        public Server()
        {
            _connectionThread = new Thread(connectionThreadFunc);
            _connectionThread.Name = "Server connection thread";
            _connectionThread.Start();
        }


    }
}
