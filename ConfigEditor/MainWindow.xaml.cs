using Microsoft.Win32;
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
using System.Configuration;

namespace ConfigEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static bool isBatch = false;
        Configuration config;
        public MainWindow()
        {
            InitializeComponent();
            this.bSaveFile.Content = "Save config";
            this.bOpenFile.Content = "Open config";
            this.lOpenedFile.Content = "...";
            this.bSaveFile.IsEnabled = false;
            string[] args = Environment.GetCommandLineArgs();
            Console.WriteLine("Arg0: " + args[0]);
            if (args.Length > 1)
            {
                this.bOpenFile.IsEnabled = false;

                string cof = System.IO.Path.Combine(Environment.CurrentDirectory, args[1]);
                populateConfigList(cof);
                this.lOpenedFile.Content = cof;
                this.bSaveFile.IsEnabled = true;
            }
        }

        private void populateConfigList(String configFileName)
        {
            config = ConfigurationManager.OpenExeConfiguration(configFileName);
            Console.WriteLine("config: " + config.FilePath);
            foreach (KeyValueConfigurationElement set in config.AppSettings.Settings)
            {
                Label l = new Label();
                l.Content = set.Key + ":";
                lbSettingList.Items.Add(l);
                TextBox tb = new TextBox();
                tb.Name = set.Key;
                tb.Text = set.Value;
                lbSettingList.Items.Add(tb);
            }

        }

        private void bOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Config files(*.exe)|ServerService.exe;ClientService.exe|All files(*.*)|*.*";
            ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            if (ofd.ShowDialog() == true)
            {
                this.lOpenedFile.Content = ofd.FileName;
                this.populateConfigList(ofd.FileName);
                this.bSaveFile.IsEnabled = true;
            }
        }

        private void bSaveFile_Click(object sender, RoutedEventArgs e)
        {
            foreach (object o in lbSettingList.Items)
            {
                if (o is Label)
                {
                    continue;
                }
                if (o is TextBox)
                {
                    TextBox tb = (TextBox)o;
                    config.AppSettings.Settings[tb.Name].Value = tb.Text;
                }
            }
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
