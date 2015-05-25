using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.Serialization.Formatters.Soap;
using SharedClasses;
using System.Threading;
using System.IO;

namespace ServerAppBeta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string data = null;
        private Packet pck;
        private Socket handler;

        private object ReceiveObject(Socket sock){
            if (!sock.Connected) return null;
            NetworkStream stream = new NetworkStream(sock);
            IFormatter formatter = new BinaryFormatter();
            try
            {
                object o = (object)formatter.Deserialize(stream);
                return o;
            }
            catch(SocketException se){
                Console.WriteLine("Exception socket");
            }
            return null;
        }

        private void listenerThread()
        {
            Thread.Sleep(300);
            while (true)
            {
                pck = null;

                pck = (Packet)ReceiveObject(handler);
                if (pck == null) continue;
                Console.WriteLine("Packet received : {0}", pck.getString());
                this.lInfo.Content = pck.getString();


            }
        }
        private void startListening()
        {
            byte[] bytes = new Byte[1024];
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");//ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 9191);

            Socket listener = new Socket(AddressFamily.InterNetwork,
            SocketType.Stream, ProtocolType.Tcp);
            Console.WriteLine("Socket created");
            //Socket handler = null ;
            try
            {
                listener.Bind(localEndPoint);
                Console.WriteLine("Bind created");
                listener.Listen(10);
                Console.WriteLine("Listening");

                // Start listening for connections.
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.
                handler = listener.Accept();
                //new Thread(listenerThread).Start();
                while (true)
                {
                    pck = null;
                    pck = (Packet)ReceiveObject(handler);
                    Console.WriteLine("Packet received : {0}", pck.getString());


                }
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

            }
            catch (SocketException se)
            {
                Console.WriteLine("Unexpected");
            }
            catch (IOException ioe)
            {
                Console.WriteLine("IOE");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            finally
            {
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
        public MainWindow()
        {
            InitializeComponent();
            startListening();
        }
    }
}
