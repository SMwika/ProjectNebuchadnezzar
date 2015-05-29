using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace ServerService
{
    public partial class NebuchadnezzarServer : ServiceBase
    {
        Server srv;
        ServerConnector srvConn;
        ServiceHost wcfHost;
        public NebuchadnezzarServer(string[] args)
        {
            InitializeComponent();
            string eventSourceName = "NebuchadnezzarServerService";
            string logName = "NebuchadnezzarMon";
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists(eventSourceName))
            {
                System.Diagnostics.EventLog.CreateEventSource(eventSourceName, logName);
            }
            eventLog1.Source = eventSourceName;
            eventLog1.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            srv = new Server(eventLog1);
            if (wcfHost != null)
            {
                wcfHost.Close();
            }

            wcfHost = new ServiceHost(typeof(ServerConnector), new Uri[] { new Uri("net.pipe://localhost") });
            //srvConn = new ServerConnector();
            //wcfHost.AddServiceEndpoint(typeof(SharedClasses.IServerConnector), new BasicHttpBinding(), "PacketDB");
            wcfHost.AddServiceEndpoint(typeof(SharedClasses.IServerConnector), new NetNamedPipeBinding(), "PipePacketDB");
            wcfHost.Open();
        }

        protected override void OnStop()
        {
            if (wcfHost != null)
            {
                wcfHost.Close();
                wcfHost = null;
            }
        }

        internal void TestStartupAndStop(string[] args)
        {
            this.OnStart(args);
            Console.ReadLine();
            this.OnStop();
        }

        private void eventLog1_EntryWritten(object sender, EntryWrittenEventArgs e)
        {

        }
    }
}
