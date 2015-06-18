using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections;
using System.Runtime.Remoting.Contexts;
using System.IO;
using System.Threading;
using System.Configuration;

namespace ClientService
{
    
    public partial class NebuchadnezzarClient : ServiceBase
    {

        #region ConnectionThread
        private System.Net.Sockets.Socket sockfd;
        //private string ip = "127.0.0.1";
        //private int port = 9191;
        private String ip = ConfigurationManager.AppSettings["serverIP"];//"127.0.0.1";
        private int port = Convert.ToInt32(ConfigurationManager.AppSettings["serverPort"]);//9191;
        private ManualResetEvent _shutdownConnThreadEvent = new ManualResetEvent(false);
        private Thread _connectionThread;
        private bool isConnected = false;

        private System.Collections.Generic.List<System.IO.FileSystemWatcher> watchers;

        /// <summary>
        /// polls given socket to check connection
        /// </summary>
        /// <param name="s">socket to check</param>
        /// <param name="mode"></param>
        /// <returns>boolean value</returns>
        private bool IsSocketConnected(System.Net.Sockets.Socket s, bool mode)
        {
            if (s == null) return false;
            bool poll = s.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
            bool avail = (s.Available == 0);
            if (poll && avail) return false;
            else return true;
        }
        /// <summary>
        /// Connection worker thread - checks if server is available
        /// if server is available it sets the flag and starts polling for configuration injects
        /// if not it unset the flag and trying again sleeping given time
        /// </summary>
        private void ConnectionThreadWorker()
        {
            //Environment.SpecialFolder.

            Console.WriteLine(ip + ":" + port);
            eventLog1.WriteEntry("Connection Worker started...", EventLogEntryType.Information);
            System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse(ip);
            System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAddress, port);
            this.sockfd = null;
            //this.sockfd = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
            //    System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            bool before = true;
            while (true)
            {
                if (!IsSocketConnected(this.sockfd, before))
                {
                    isConnected = false;
                    try
                    {
#if(DEBUG)
                        Console.WriteLine("count: " + packetList.Count);
#endif
                        this.sockfd = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                            System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        this.sockfd.Connect(remoteEP);
                        byte[] buf = new byte[3];
                        sockfd.ReceiveTimeout = 1000;
                        sockfd.Receive(buf);
                        if (buf.SequenceEqual(new byte[] { 0xFF, 0xFE, 0xFD }))
                        {
#if(DEBUG)
                            Console.WriteLine("Cannot connect: Server refuses connection from this IP");
#endif
                            return;
                        }
                        sockfd.ReceiveTimeout = 0;
                        isConnected = true;
                        before = false;
#if(DEBUG)
                        Console.WriteLine("Connected!");
#endif
                        this.SendPacketList(packetList);
                    }
                    catch (Exception e)
                    {
                        this.sockfd = null;
                        eventLog1.WriteEntry("Cannot connect to server. Still trying...", EventLogEntryType.Warning);
#if(DEBUG)
                        Console.WriteLine("Cannot connect to server. Still trying...");
#endif
                    }
                }
                else
                {
                    SharedClasses.ConfigPacket cp;
                    cp = (SharedClasses.ConfigPacket)ReceiveObject(sockfd);
                    if (cp != null)
                    {
                        /*
                         * <add key="serverIP" value="127.0.0.1"/>
                         * <add key="serverPort" value="9191"/>
                         * <add key="folderPath" value="C:\\temp;D:\\tmp"/> <!-- enter folder paths to watch separated by semicolons ';' like: "C:\\temp;D:\\temp\\something" -->
                         * <add key="fileFilter" value="*.java;*.c"/> <!-- enter file filters separated by semicolons ';' like: "*.c;*.cs;*.html" -->
                         * <add key="includeSubDirs" value="true"/> <!-- true or false -->
                         * <add key="serialNumber" value="b7337eee-d172-4cc7-a9eb-c180662aa950"/>
                         * 
                         */
#if(DEBUG)
                        Console.WriteLine(cp.ToString());
#endif
                        var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                        config.AppSettings.Settings["serverIP"].Value = cp.ServerIP;
                        config.AppSettings.Settings["serverPort"].Value = cp.ServerPort;
                        config.AppSettings.Settings["folderPath"].Value = cp.WatcherDirectories;
                        config.AppSettings.Settings["fileFilter"].Value = cp.WatcherFilters;
                        config.AppSettings.Settings["includeSubDirs"].Value = cp.WatcherIncludeSubdirectories;
                        config.AppSettings.Settings["serialNumber"].Value = cp.SerialNumber;
                        config.Save(ConfigurationSaveMode.Modified);

                        ConfigurationManager.RefreshSection("appSettings");
                        //InitWatchers();
#if(DEBUG)
                        Console.WriteLine("New configuration injected - restarting Service");
#endif
                        eventLog1.WriteEntry("New configuration injected - restarting Service", EventLogEntryType.Information);
                        OnStop();
                        Environment.Exit(1);
                    }
#if(DEBUG)
                    Console.WriteLine("Already connected");
#endif
                }
                Thread.Sleep(1000 * 3);
            }
            //Console.WriteLine("Connection Worker ended successfully");
            //this.SendPacketList(packetList);
            
        }
        #endregion
        public NebuchadnezzarClient(string[] args)
        {
            InitializeComponent();
            InitWatchers();
            //string eventSourceName = "MySource";
            //string logName = "MyNewLog";
            string eventSourceName = "NebuchadnezzarClientService";
            string logName = "NebuchadnezzarMon";
            if (args.Count() > 0)
            {
                eventSourceName = args[0];
            }
            if (args.Count() > 1)
            {
                logName = args[1];
            }
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        public void onTimer(object sender, System.Timers.ElapsedEventArgs args){
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, 123);
        }
    
        protected override void OnStart(string[] args)
        {
            //System.Net.IPHostEntry ipHostInfo = System.Net.Dns.GetHostEntry("127.0.0.1");
            _connectionThread = new Thread(ConnectionThreadWorker);
            _connectionThread.Name = "Connection Maker";
            _connectionThread.IsBackground = true;
            _connectionThread.Start();
            eventLog1.WriteEntry("In OnStart");
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 60000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.onTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("In OnStop");
            SendObject(new SharedClasses.Packet("exit", DateTime.Now, "exit", "exit", SharedClasses.WatcherInfoType.FILE_CHANGED));
        }

        protected override void OnContinue()
        {
            eventLog1.WriteEntry("In OnContinue");
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

    }
}
