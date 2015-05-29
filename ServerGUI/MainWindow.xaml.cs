using System;
using System.Collections.Generic;
using System.Linq;
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
using SharedClasses;
using System.ServiceModel;
using System.Threading;

namespace ServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class MainWindow : Window
    {
        IServerConnector connector;
        private bool isConnected = false;
        delegate void SetIsConnectedCallback(bool conn);
        private ChannelFactory<IServerConnector> pipeFactory;

        private void WcfConnectionThreadFunc()
        {
            while (true)
            {
                try
                {
                    updateLists();
                    this.SetConnected(true);
                    break;
                }
                catch (Exception ex)
                {
                    if(ex is EndpointNotFoundException || ex is CommunicationObjectFaultedException)
                    {
                        this.SetConnected(false);
                        Thread.Sleep(5000);
                    }
                }
            }
        }
        public MainWindow()
        {
            InitializeComponent();
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.MaxReceivedMessageSize = 65536 * 32;
            pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("net.pipe://localhost/PipePacketDB"));
            new Thread(WcfConnectionThreadFunc).Start();
            //updateLists();
        }

        private void SetConnected(bool conn)
        {
            if (this.circleNotifier.Dispatcher.CheckAccess())
            {
                this.IsConnected = conn;
            }
            else
            {
                SetIsConnectedCallback d = new SetIsConnectedCallback(SetConnected);
                this.circleNotifier.Dispatcher.Invoke(d, new object[] {conn});
            }
        }

        private bool IsConnected
        {
            set
            {
                this.isConnected = value;
                if (value == true)
                {
                    circleNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    dpDatePicker.IsEnabled = true;
                    cbIPList.IsEnabled = true;

                }
                else
                {
                    circleNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    dpDatePicker.IsEnabled = false;
                    cbIPList.IsEnabled = false;
                }
                    
            }
            get
            {
                return this.isConnected;
            }
        }

        private void updateLists()
        {
            connector = pipeFactory.CreateChannel();
            List<PacketDB> list = connector.GetUniqueFileNames();
            if (this.cbIPList.Dispatcher.CheckAccess())
            {
                lbFileList.Items.Clear();
                foreach (PacketDB packet in list)
                {
                    lbFileList.Items.Add(packet.FileName);
                }
                List<string> uniqueIPs = list.Select(x => x.IpAddress).Distinct().ToList();
                cbIPList.Items.Clear();
                cbIPList.Items.Add("<ANY>");
                cbIPList.SelectedIndex = 0;
                foreach (String ip in uniqueIPs)
                {
                    cbIPList.Items.Add(ip);
                }
            }
            else
            {
                this.cbIPList.Dispatcher.Invoke(updateLists);
            }
            

        }

        private void dpDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isConnected)
            {
                List<PacketDB> list;
                if(((DatePicker)sender).SelectedDate == null)
                    list = connector.GetUniqueFileNames();
                else
                    list = connector.GetUniqueFileNamesByDate(((DatePicker)sender).SelectedDate.ToString());
                lbFileList.Items.Clear();
                foreach (PacketDB packet in list)
                {
                    lbFileList.Items.Add(packet.FileName);
                }
            }            
        }

        private void cbIPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isConnected)
            {

            }
        }
    }
}
