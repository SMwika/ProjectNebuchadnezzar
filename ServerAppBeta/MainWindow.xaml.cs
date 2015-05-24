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
            object o = (object)formatter.Deserialize(stream);
            return o;
        }

        private void listenerThread()
        {
            Thread.Sleep(300);
            while (true)
            {
                pck = null;
                // An incoming connection needs to be processed.
                //while (true)
                //{
                //    bytes = new byte[1024];
                //    int bytesRec = handler.Receive(bytes);
                //    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                //    if (data.IndexOf("<EOF>") > -1)
                //    {
                //        break;
                //    }
                //}

                pck = (Packet)ReceiveObject(handler);
                if (pck == null)
                {
                    //Console.WriteLine("Null Object");
                    continue;
                }

                // Show the data on the console.
                //Console.WriteLine("Text received : {0}", data);
                Console.WriteLine("Packet received : {0}", pck.getString());
                this.lInfo.Content = pck.getString();
                // Echo the data back to the client.


            }
        }
        private void startListening()
        {
            byte[] bytes = new Byte[1024];
            //IPHostEntry ipHostInfo = Dns.Resolve(Dns.GetHostName());
            IPAddress ipAddress = IPAddress.Parse("192.168.1.51");//ipHostInfo.AddressList[0];
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

                    //data = null;
                    pck = null;
                    // An incoming connection needs to be processed.
                    //while (true)
                    //{
                    //    bytes = new byte[1024];
                    //    int bytesRec = handler.Receive(bytes);
                    //    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    //    if (data.IndexOf("<EOF>") > -1)
                    //    {
                    //        break;
                    //    }
                    //}

                    pck = (Packet)ReceiveObject(handler);

                //    // Show the data on the console.
                    //Console.WriteLine("Text received : {0}", data);
                    Console.WriteLine("Packet received : {0}", pck.getString());
                    //this.lInfo.Content = data;
                //    // Echo the data back to the client.


                }
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();

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
