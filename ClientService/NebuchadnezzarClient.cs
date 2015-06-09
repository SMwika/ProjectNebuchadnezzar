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
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }
    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public long dwServiceType;
        public ServiceState dwCurrentState;
        public long dwControlsAccepted;
        public long dwWin32ExitCode;
        public long dwServiceSpecificExitCode;
        public long dwCheckPoint;
        public long dwWaitHint;
    }

    public partial class NebuchadnezzarClient : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetSericeStatus(IntPtr handle, ref ServiceStatus serviceStatus);


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


        private bool IsSocketConnected(System.Net.Sockets.Socket s, bool mode)
        {
            if (s == null) return false;
            bool poll = s.Poll(1000, System.Net.Sockets.SelectMode.SelectRead);
            bool avail = (s.Available == 0);
            if (poll && avail) return false;
            else return true;
        }
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
                        Console.WriteLine("count: " + packetList.Count);
                        this.sockfd = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                            System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
                        this.sockfd.Connect(remoteEP);
                        isConnected = true;
                        before = false;
                        Console.WriteLine("Connected!");
                        this.SendPacketList(packetList);
                    }
                    catch (Exception e)
                    {
                        this.sockfd = null;
                        eventLog1.WriteEntry("Cannot connect to server. Still trying...", EventLogEntryType.Warning);
                        Console.WriteLine("Cannot connect to server. Still trying...");
                    }
                }
                else
                {
                    Console.WriteLine("Already connected");
                }
                Thread.Sleep(1000 * 3);
            }
            Console.WriteLine("Connection Worker ended successfully");
            this.SendPacketList(packetList);
            
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
