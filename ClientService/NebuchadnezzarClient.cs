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

        private System.Net.Sockets.Socket sockfd;
        public NebuchadnezzarClient(string[] args)
        {
            InitializeComponent();
            string eventSourceName = "MySource";
            string logName = "MyNewLog";
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

        private void initWatcher()
        {
            watcher.Path = "C:\temp";
            watcher.Filter = "*.txt";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
            watcher.Changed += new FileSystemEventHandler(watcherChanged);
            watcher.Created += new FileSystemEventHandler(watcherCreated);
            watcher.Deleted += new FileSystemEventHandler(watcherDeleted);
            watcher.Renamed += new RenamedEventHandler(watcherRenamed);
        }

        public void onTimer(object sender, System.Timers.ElapsedEventArgs args){
            //eventLog1.WriteEntry("Monitoring the System", EventLogEntryType.Information, 123);
        }

        //protected override void OnBeforeInstall(IDictionary savedState) { 
        //    string parameter = "MySource1\" \"MyLogFile1";
        //    Context.Parameters["assemblypath"] = "\"" + Context.Parameters["assemblypath"] + "\" \"" + parameter + "\""; 
        //    base.OnBeforeInstall(savedState); 
        //}

        protected override void OnStart(string[] args)
        {
            //Debugger.Break();
            //ServiceStatus serviceStatus = new ServiceStatus();
            //serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            //serviceStatus.dwWaitHint = 1000000;
            //SetSericeStatus(this.ServiceHandle, ref serviceStatus);
            int port = 9191;
            //System.Net.IPHostEntry ipHostInfo = System.Net.Dns.GetHostEntry("127.0.0.1");
            System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse("192.168.1.51");// ipHostInfo.AddressList[0];
            System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAddress, port);
            this.sockfd = new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);
            try
            {
                this.sockfd.Connect(remoteEP);

            }
            catch (Exception e)
            {
                eventLog1.WriteEntry(e.ToString(), EventLogEntryType.Error);
                Console.WriteLine(e.ToString());
            }
            //initWatcher();
            eventLog1.WriteEntry("In OnStart");
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Enabled = true;
            timer.Interval = 60000;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.onTimer);
            timer.Start();

            //serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            //SetSericeStatus(this.ServiceHandle, ref serviceStatus);
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
