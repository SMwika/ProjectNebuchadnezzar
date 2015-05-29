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
        private List<PacketDB> list;
        private List<PacketDB> shownList;
        private void updateLists()
        {
            connector = pipeFactory.CreateChannel();
            list = connector.GetUniqueFileNames();
            if (this.cbIPList.Dispatcher.CheckAccess())
            {
                lbFileList.Items.Clear();
                foreach (PacketDB packet in list)
                {
                    lbFileList.Items.Add(packet.FileName);
                }
                updateIpList();
            }
            else
            {
                this.cbIPList.Dispatcher.Invoke(updateLists);
            }
            shownList = list;
        }

        private void updateIpList()
        {
            List<string> uniqueIPs = list.Select(x => x.IpAddress).Distinct().ToList();
            cbIPList.Items.Clear();
            cbIPList.Items.Add("<ANY>");
            cbIPList.SelectedIndex = 0;
            foreach (String ip in uniqueIPs)
            {
                cbIPList.Items.Add(ip);
            }
        }

        private void dpDatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isConnected)
            {
                //List<PacketDB> list;
                if(((DatePicker)sender).SelectedDate == null)
                    list = connector.GetUniqueFileNames();
                else
                    list = connector.GetUniqueFileNamesByDate(((DatePicker)sender).SelectedDate.ToString());
                lbFileList.Items.Clear();
                foreach (PacketDB packet in list)
                {
                    lbFileList.Items.Add(packet.FileName);
                }
                updateIpList();
                shownList = list;
            }
            lbFileList.SelectedIndex = -1;
        }

        private void cbIPList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isConnected)
            {
                if ((string)cbIPList.SelectedValue == "<ANY>")
                {
                    lbFileList.Items.Clear();
                    foreach (PacketDB packet in list)
                    {
                        lbFileList.Items.Add(packet.FileName);
                    }
                    shownList = list;
                }
                else
                {
                    List<PacketDB> listByIp = new List<PacketDB>();
                    listByIp = list.Where(x => x.IpAddress == (string)cbIPList.SelectedValue).ToList();
                    lbFileList.Items.Clear();
                    foreach (PacketDB packet in listByIp)
                    {
                        lbFileList.Items.Add(packet.FileName);
                    }
                    shownList = listByIp;
                }
                lbFileList.SelectedIndex = -1;
                
            }
        }

        private void lbFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex == -1)
            {
                this.tbFilePreview.Text = "";
            }
            else
            {
                //this.tbFilePreview.Text = shownList[((ListBox)sender).SelectedIndex].ToString();
                int id = shownList[((ListBox)sender).SelectedIndex].Id_files;
                if (id < 0)
                {
                    this.tbFilePreview.Text = "No content available";
                }
                else
                {
                    id = connector.GetLastRevisionID(((ListBox)sender).SelectedValue.ToString().Replace("\\", "\\\\"));
                    //this.tbFilePreview.Text = ((ListBox)sender).SelectedIndex.ToString();
                    //this.tbFilePreview.Text += id;
                    //this.tbFilePreview.Text = connector.GetFileContents(shownList[((ListBox)sender).SelectedIndex].Id_files);
                    this.tbFilePreview.Text = connector.GetFileContents(id);
                }
            }
        }
    }
}
