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

namespace ServerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    
    public partial class MainWindow : Window
    {
        IServerConnector connector;
        public MainWindow()
        {
            InitializeComponent();
            NetNamedPipeBinding binding = new NetNamedPipeBinding();
            binding.MaxReceivedMessageSize = 65536 * 32;
            ChannelFactory<IServerConnector> pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("net.pipe://localhost/PipePacketDB"));
            connector = pipeFactory.CreateChannel();
            List<PacketDB> list = connector.GetUniqueFileNames();
            lbFileList.Items.Clear();
            foreach (PacketDB packet in list)
            {
                lbFileList.Items.Add(packet.FileName);
            }
            List<string> uniqueIPs = list.Select(x => x.IpAddress).Distinct().ToList();
            cbIPList.ItemsSource = uniqueIPs;
        }

        private void dpDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            List<PacketDB> list = connector.GetUniqueFileNamesByDate(dpDatePicker.SelectedDate.ToString());
            lbFileList.Items.Clear();
            foreach (PacketDB packet in list)
            {
                lbFileList.Items.Add(packet.FileName);
            }
        }

        private void cbIPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
