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
using SharedClasses;
using System.Configuration;

namespace ServerService
{
    public partial class NebuchadnezzarServer : ServiceBase
    {
        Server srv;
        ServiceHost wcfHost;

        //ChannelFactory<IGuiWcfConnector> pipeFactory;
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
            wcfHost = new ServiceHost(typeof(ServerConnector), new Uri[] { new Uri("net.pipe://localhost/server"), new Uri("http://" + ConfigurationManager.AppSettings["listenerIP"] + ":9292") });
            //srvConn = new ServerConnector();
            wcfHost.AddServiceEndpoint(typeof(SharedClasses.IServerConnector), new NetNamedPipeBinding(), "PipePacketDB");
            wcfHost.AddServiceEndpoint(typeof(SharedClasses.IServerConnector), new BasicHttpBinding(), "PacketDB");
            wcfHost.Open();
            //NetNamedPipeBinding binding = new NetNamedPipeBinding();
            //binding.MaxReceivedMessageSize = 65536 * 32;
            //pipeFactory = new ChannelFactory<IGuiWcfConnector>(binding, new EndpointAddress("net.pipe://localhost/client/PipeLiverGUI"));
            //srv.StartWcfGuiConnection(pipeFactory);
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
