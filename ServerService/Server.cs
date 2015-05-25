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
            catch (SocketException se)
            {
                Console.WriteLine(se.ToString());
            }
            return null;
        }

        private void connectionThreadFunc()
        {
            Packet pck = null;
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, port);

            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            while(true){
                try
                {
                    listener.Bind(localEndPoint);
                    Console.WriteLine("Bind created");
                    listener.Listen(10);
                    Console.WriteLine("Listening...");

                    Socket handler;
                    handler = listener.Accept();
                    Thread clientService = new Thread(() => clientServiceThreadFunc(handler));
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.ToString());
                }
            }
        }

        private void clientServiceThreadFunc(Socket s)
        {
            /* obsługa każdego klienta - odczyt obiektów z socketa (funkcja ReceiveObject(Socket sock) )*/
        }
        public Server()
        {
            _connectionThread = new Thread(connectionThreadFunc);
            _connectionThread.Name = "Server connection thread";
            _connectionThread.Start();
        }


    }
}
