﻿using System;
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
        private List<String> ipList = new List<String>();
        private Thread autoUpdateThread, wcfConnectionThread;
        private bool liverBlink = false;
        //ServiceHost wcfHost;

        /// <summary>
        /// thread implementation for live GUI updates
        /// </summary>
        private void AutoUpdateThreadFunc() // Liver Updates
        {
            while (true)
            {
                this.lbLogList.Dispatcher.Invoke(() => updateLogs(false));
                this.lbClientList.Dispatcher.Invoke(updateActiveConnections);
                Thread.Sleep(5000);
            }
        }
        ///// <summary>
        ///// not used
        ///// </summary>
        //private void blinkerThread()
        //{
        //    if (liverBlink)
        //    {
        //        setColor(circleLiverNotifier, new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)));
        //        Thread.Sleep(100);
        //        setColor(circleLiverNotifier, new SolidColorBrush(Color.FromArgb(255, 0, 255, 0)));
        //        Thread.Sleep(100);
        //    }
        //    else
        //    {
        //        setColor(circleLiverNotifier, new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)));
        //    }
        //}

        private void setColor(Ellipse el, SolidColorBrush col)
        {
            if (el.Dispatcher.CheckAccess())
            {
                el.Fill = col;
            }
            else
            {
                el.Dispatcher.Invoke(() => setColor(el, col));
            }
        }

        private void WcfConnectionThreadFunc()
        {
            while (true)
            {
                try
                {
                    //liverBlink = true;
                    updateLists();
                    this.SetConnected(true);
                    this.autoUpdateThread = new Thread(AutoUpdateThreadFunc);
                    autoUpdateThread.Start();
                    //liverBlink = false;
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
        /// <summary>
        /// initializes basic form components
        /// </summary>
        private void InitControls()
        {
            this.tbDate.Text = "";
            this.tbFileHash.Text = "";
            this.tbFileName.Text = "";
            this.tbFilePreview.Text = "";
            this.tbIpAddress.Text = "";
            this.tbIType.Text = "";
            this.tbOldFileName.Text = "";
            this.tbPacketID.Text = "";
            this.tbUserName.Text = "";

            this.tbDate.IsEnabled = false;
            this.tbFileHash.IsEnabled = false;
            this.tbFileName.IsEnabled = false;
            this.tbFilePreview.IsEnabled = true;
            this.tbIpAddress.IsEnabled = false;
            this.tbIType.IsEnabled = false;
            this.tbOldFileName.IsEnabled = false;
            this.tbPacketID.IsEnabled = false;
            this.tbUserName.IsEnabled = false;
            this.bInjectConfig.IsEnabled = false;

            this.lDate.Content = "Date: ";
            this.lFileHash.Content = "File hash: ";
            this.lFileName.Content = "File name: ";
            this.lOldFileName.Content = "Old file name: ";
            this.lIpAddress.Content = "IP Address: ";
            this.lIType.Content = "Info type: ";
            this.lPacketID.Content = "Packet ID: ";
            this.lUserName.Content = "User name: ";

            this.lIP.Content = "IP: ";
            this.lFilePreview.Content = "File content preview: ";
            this.lFileList.Content = "File list: ";

            this.tbServerAddr.Text = "";
            this.cboxIsLocal.Content = "Is local?";
            this.bConnect.Content = "Connect";
            this.bInjectConfig.Content = "Inject config";
            this.bEditConfig.Content = "Edit config";

            this.lServerAddr.Content = "Server addr:";

            this.cboxIsLocal.IsChecked = true;
            this.tbServerAddr.Visibility = System.Windows.Visibility.Hidden;
            this.lServerAddr.Visibility = System.Windows.Visibility.Hidden;
        }
        public MainWindow()
        {
            InitializeComponent();
            InitControls();

            //BasicHttpBinding binding = new BasicHttpBinding();
            //binding.MaxReceivedMessageSize = 65536 * 32;
            //pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("http://192.168.1.51:9292/PacketDB"));

            //NetNamedPipeBinding binding = new NetNamedPipeBinding();
            //binding.MaxReceivedMessageSize = 65536 * 32;
            //pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("net.pipe://localhost/server/PipePacketDB"));

            //this.wcfConnectionThread = new Thread(WcfConnectionThreadFunc);
            //this.wcfConnectionThread.Start();

            //new Thread(blinkerThread).Start();
            //InitWCF();
            //updateLists();
        }

        public void ParseLiverEvent(String ev)
        {
            tbFilePreview.Text = ev;
        }

        /// <summary>
        /// Dispatcher method to set connected flag to GUI
        /// </summary>
        /// <param name="conn"></param>
        private void SetConnected(bool conn)
        {
            if (this.circleNotifier.Dispatcher.CheckAccess())
            {
                this.IsConnected = conn;
                this.tbServerAddr.IsEnabled = false;
                this.cboxIsLocal.IsEnabled = false;
                this.bConnect.IsEnabled = false;
            }
            else
            {
                SetIsConnectedCallback d = new SetIsConnectedCallback(SetConnected);
                this.circleNotifier.Dispatcher.Invoke(d, new object[] {conn});
            }
        }
        /// <summary>
        /// simple getter and setter for bool isConnected value.
        /// setter also changes color of inGUI notificator
        /// </summary>
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
                    bInjectConfig.IsEnabled = true;

                }
                else
                {
                    circleNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                    dpDatePicker.IsEnabled = false;
                    cbIPList.IsEnabled = false;
                    bInjectConfig.IsEnabled = false;
                }
                    
            }
            get
            {
                return this.isConnected;
            }
        }
        private List<PacketDB> list;
        private List<PacketDB> shownList;
        /// <summary>
        /// simple method to update lists of files/connected users/logs
        /// </summary>
        private void updateLists()
        {
            connector = pipeFactory.CreateChannel();
            list = connector.GetUniqueFileNames();
            //this.circleLiverNotifier.Dispatcher.Invoke(() => setColor(circleLiverNotifier, "Red"));
            if (this.cbIPList.Dispatcher.CheckAccess())
            {
                this.circleLiverNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                lbFileList.Items.Clear();
                foreach (PacketDB packet in list)
                {
                    lbFileList.Items.Add(packet.FileName);
                }
                updateIpList();
                updateActiveConnections();
                updateLogs();
                this.circleLiverNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            }
            else
            {
                this.cbIPList.Dispatcher.Invoke(updateLists);
            }
            shownList = list;
            //this.circleLiverNotifier.Dispatcher.Invoke(() => setColor(circleLiverNotifier, "Yellow"));
        }

        private void setColor(Ellipse ellip, String col)
        {
            if (col == "Red")
                this.circleLiverNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
            else if (col == "Green")
                this.circleLiverNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
            else if (col == "Yellow")
                this.circleLiverNotifier.Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
            //Thread.Sleep(500);
        }
        /// <summary>
        /// <see cref="updateLogs(bool firstTime)"/>
        /// </summary>
        private void updateLogs()
        {
            updateLogs(true);
        }
        /// <summary>
        /// method for updating log list in GUI
        /// </summary>
        /// <param name="firstTime">true if it's a initialization update</param>
        private void updateLogs(bool firstTime)
        {

            List<String> logList = connector.GetLogs();
            if (firstTime)
            {
                lbLogList.Items.Clear();
                foreach (String log in logList)
                {
                    if (log.StartsWith("[1]"))
                    {
                        Label l = new Label();
                        l.Content = log;
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                        lbLogList.Items.Add(l);
                        //MessageBox.Show("Possible plagiarism detected. See logs for more info.", "Possible plagiarism", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        lbLogList.Items.Add(log);
                    }
                }
            }
            else
            {
                if (logList.Count > lbLogList.Items.Count)
                {
                    if (!this.tabItemLogs.IsSelected)
                        tabItemLogs.Background = new SolidColorBrush(Color.FromArgb(255, 200, 0, 0));
                    foreach (String log in logList)
                    {
                        if (lbLogList.Items.Contains(log)) break;
                        if (log.StartsWith("[1]"))
                        {
                            Label l = new Label();
                            l.Content = log;
                            l.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                            lbLogList.Items.Insert(0, l);
                            MessageBox.Show("Possible plagiarism detected. See logs for more info.", "Possible plagiarism", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                        {
                            lbLogList.Items.Insert(0, log);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// updates active connection list in GUI
        /// </summary>
        private void updateActiveConnections()
        {
            //ipList = connector.GetActiveConnections();
            List<String> activeIpList = connector.GetActiveConnections();
            ipList = connector.GetValidConnections();
            if (ipList.Count != lbClientList.Items.Count)
            {
                if (!this.tabItemConnections.IsSelected)
                    tabItemConnections.Background = new SolidColorBrush(Color.FromArgb(255, 200, 0, 0));
            }
            List<String> tmpList = new List<String>();
                lbClientList.Items.Clear();
                foreach (String ip in ipList)
                {
                    String tmp = "";
                    foreach (String aip in activeIpList)
                    {
                        if (aip.StartsWith(ip))
                        {
                            tmp = aip + "[ON]";
                            break;
                            //tmpList.Add(aip + "[ON]");
                        }
                    }
                    if (tmp == "") tmp = ip + "[OFF]";
                    tmpList.Add(tmp);
                }
                foreach (String ip in tmpList)
                {
                    Label l = new Label();
                    l.Content = ip;
                    if (ip.EndsWith("[ON]"))
                        l.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 255, 0));
                    lbClientList.Items.Add(l);
                }
            //}
        }

        /// <summary>
        /// updates IP list to filter files stored in DB
        /// </summary>
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

        #region component events methods
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
            setColor(this.circleLiverNotifier, "Red");
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
            setColor(this.circleLiverNotifier, "Green");
        }

        List<PacketDB> revList;

        private void lbFileList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (((ListBox)sender).SelectedIndex == -1)
            {
                this.tbFilePreview.Text = "No file selected";
            }
            else
            {
                revList = connector.GetFileRevisions(((ListBox)sender).SelectedValue.ToString().Replace("\\", "\\\\"));
                cbRevList.Items.Clear();

                foreach (PacketDB packet in revList)
                {
                    cbRevList.Items.Add(packet.Date.ToString("dd-MM-yyyy HH:mm:ss")); ;
                }
                if (cbRevList.Items.Count > 0) cbRevList.SelectedIndex = 0;
            }
        }

        private void cbRevList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            setColor(this.circleLiverNotifier, "Red");
            if (((ComboBox)sender).SelectedIndex == -1)
            {
                this.tbFilePreview.Text = "No file selected";
            }
            else
            {
                int sel = ((ComboBox)sender).SelectedIndex;
                int id = revList[((ComboBox)sender).SelectedIndex].Id_files;
                this.tbDate.Text = revList[((ComboBox)sender).SelectedIndex].Date.ToString();
                this.tbFileHash.Text = revList[((ComboBox)sender).SelectedIndex].FileHash;
                this.tbFileName.Text = revList[sel].FileName;
                this.tbOldFileName.Text = revList[sel].OldFileName;
                this.tbIpAddress.Text = revList[sel].IpAddress;
                this.tbIType.Text = revList[sel].IType.ToString();
                this.tbPacketID.Text = revList[sel].Id_packet.ToString();
                this.tbUserName.Text = revList[sel].User;
                if (id < 0)
                {
                    this.tbFilePreview.Text = "No content available";
                }
                else
                {
                    String content = connector.GetFileContents(id);
                    if (content == "")
                    {
                        this.tbFilePreview.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));
                        content = "<Empty File>";
                    }
                    else
                    {
                        this.tbFilePreview.Foreground = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
                    }
                    this.tbFilePreview.Text = connector.GetFileContents(id);
                }
            }
            setColor(this.circleLiverNotifier, "Green");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(this.wcfConnectionThread != null)
                this.wcfConnectionThread.Abort();
            if(this.autoUpdateThread != null)
                this.autoUpdateThread.Abort();
        }

        private void tabItemLogs_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("clicked");
            tabItemLogs.Background = new SolidColorBrush(Color.FromArgb(255, 0xe5, 0xe5, 0xe5));
            tabItemConnections.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xe5, 0xe5, 0xe5));
        }

        private void circleNotifier_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.updateLists();
        }

        private void bConnect_Click(object sender, RoutedEventArgs e)
        {

            if (this.cboxIsLocal.IsChecked == true)
            {
                NetNamedPipeBinding binding = new NetNamedPipeBinding();
                binding.MaxReceivedMessageSize = 65536 * 32;
                pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("net.pipe://localhost/server/PipePacketDB"));
            }
            else
            {
                BasicHttpBinding binding = new BasicHttpBinding();
                binding.MaxReceivedMessageSize = 65536 * 32;
                pipeFactory = new ChannelFactory<IServerConnector>(binding, new EndpointAddress("http://" + this.tbServerAddr.Text + ":9292/PacketDB"));
            }

            this.wcfConnectionThread = new Thread(WcfConnectionThreadFunc);
            this.wcfConnectionThread.Start();

        }

        private void cboxIsLocal_Click(object sender, RoutedEventArgs e)
        {
            if (((CheckBox)sender).IsChecked == true)
            {
                this.tbServerAddr.Visibility = System.Windows.Visibility.Hidden;
                this.lServerAddr.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                this.tbServerAddr.Visibility = System.Windows.Visibility.Visible;
                this.lServerAddr.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private void TabControl_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (tabItemLogs.IsSelected)
                tabItemLogs.Background = new SolidColorBrush(Color.FromArgb(255, 0xe5, 0xe5, 0xe5));
            if (tabItemConnections.IsSelected)
                tabItemConnections.Background = new SolidColorBrush(Color.FromArgb(0xff, 0xe5, 0xe5, 0xe5));
        }

        private void bInjectConfig_Click(object sender, RoutedEventArgs e)
        {
            connector.injectConfig();
        }

        private void bEditConfig_Click(object sender, RoutedEventArgs e)
        {
            string filename = System.IO.Path.Combine(Environment.CurrentDirectory, "ConfigEditor.exe");
            string configName = System.IO.Path.Combine(Environment.CurrentDirectory, "ServerService.exe");
            configName = "ServerService.exe";
#if(DEBUG)
            filename = "\"C:\\Users\\Jakub\\Documents\\Visual Studio 2013\\Projects\\ProjectNebuchadnezzar\\ConfigEditor\\bin\\Debug\\ConfigEditor.exe\"";
            configName = "\"C:\\Users\\Jakub\\Documents\\Visual Studio 2013\\Projects\\ProjectNebuchadnezzar\\ServerService\\bin\\Debug\\ServerService.exe\"";
            Console.WriteLine(filename + " " + configName);
#endif
            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(filename, configName);
        }
        #endregion
    }
}
