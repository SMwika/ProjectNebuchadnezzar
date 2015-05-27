using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Text;
using SharedClasses;

namespace ClientService
{
    partial class NebuchadnezzarClient
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventLog1 = new System.Diagnostics.EventLog();
            this.watcher = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.watcher)).BeginInit();

            // 
            // watcher
            // 
            this.watcher.EnableRaisingEvents = true;
            this.watcher.IncludeSubdirectories = true;
            this.watcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.Size)));
            this.watcher.Path = System.Configuration.ConfigurationManager.AppSettings["folderPath"];
            this.watcher.Filter = System.Configuration.ConfigurationManager.AppSettings["fileFilter"];
            this.watcher.Changed += new System.IO.FileSystemEventHandler(this.watcherChanged);
            this.watcher.Created += new System.IO.FileSystemEventHandler(this.watcherCreated);
            this.watcher.Deleted += new System.IO.FileSystemEventHandler(this.watcherDeleted);
            this.watcher.Renamed += new System.IO.RenamedEventHandler(this.watcherRenamed);
            // 
            // NebuchadnezzarClient
            // 
            this.AutoLog = false;
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.watcher)).EndInit();

        }


        #endregion

        private System.Diagnostics.EventLog eventLog1;
        private System.IO.FileSystemWatcher watcher;
        private System.Collections.Generic.List<Packet> packetList = new System.Collections.Generic.List<Packet>();

        private string GetFileHash(string path)
        {
            System.IO.FileStream fs = null;
            for (int i = 1; i <= 3; i++)
            {
                try
                {
                    fs = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
                    break;
                }
                catch (System.IO.FileNotFoundException)
                {
                    return "";
                }
                catch (System.IO.IOException e)
                {
                    if (i == 3) System.Console.WriteLine(e.ToString());
                    System.Threading.Thread.Sleep(1000);
                    //System.Console.WriteLine(e.ToString());
                }
            }
                
            return GetFileHash(fs);
        }

        private string GetFileHash(System.IO.FileStream stream)
        {
            StringBuilder sb = new StringBuilder();
            if (null != stream)
            {
                stream.Seek(0, System.IO.SeekOrigin.Begin);

                MD5 md5 = MD5CryptoServiceProvider.Create();
                byte[] hash = md5.ComputeHash(stream);
                foreach (byte b in hash)
                    sb.Append(b.ToString("x2"));
                stream.Seek(0, System.IO.SeekOrigin.Begin);
                stream.Close();
                System.Console.WriteLine("StreamClosed");
            }
            return sb.ToString();
        }

        private int Send(string msg)
        {
            byte[] byteMsg = System.Text.Encoding.ASCII.GetBytes(msg);
            return this.Send(byteMsg);
        }
        private int Send(byte[] msg){
            if (isConnected)
                return sockfd.Send(msg);
            else return -1;
        }

        private void SendPacketList(System.Collections.Generic.List<Packet> list){
            foreach (Packet p in list)
            {
                SendObject(p);
                //list.Remove(p);
            }
            list.Clear();
        }

        private int xxx;

        private void SendObject(object o)
        {
            //System.Console.WriteLine("count: " + xxx);
            if (!isConnected)
            {
                packetList.Add((Packet)o);
                return;
            }
            //if (!nonConnectedListSent)
            //{
            //    SendPacketList(packetList);
            //    nonConnectedListSent = true;
            //}
            IFormatter formatter = new BinaryFormatter();
            System.Net.Sockets.NetworkStream stream = new System.Net.Sockets.NetworkStream(sockfd);
            formatter.Serialize(stream, o);
        }

        #region FileSystemWatcher_Events
        private void watcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            //System.Console.WriteLine("Changed file " + e.FullPath + " " + GetFileHash(e.FullPath));
            this.SendObject(new Packet("user", System.DateTime.Now, e.FullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_CHANGED));
        }

        private void watcherDeleted(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog1.WriteEntry("Deleted file " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
            System.Console.WriteLine("Deleted file " + e.FullPath);
            this.SendObject(new Packet("user", System.DateTime.Now, e.FullPath, "", WatcherInfoType.FILE_DELETED));
        }

        private void watcherCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            //xxx++;
            //eventLog1.WriteEntry("Created file " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
            //System.Console.WriteLine("Created file " + e.FullPath + " " + GetFileHash(e.FullPath));
            //int bytesSent = this.Send("Created file " + e.Name + "<EOF>");
            this.SendObject(new Packet("user", System.DateTime.Now, e.FullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_CREATED));

        }

        private void watcherRenamed(object sender, System.IO.RenamedEventArgs e)
        {
            eventLog1.WriteEntry("Renamed file " + e.OldFullPath + " to " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
            System.Console.WriteLine("Renamed file " + e.OldFullPath + " to " + e.FullPath);
            this.SendObject(new Packet("user", System.DateTime.Now, e.FullPath, e.OldFullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_RENAMED));
        }
        #endregion
    }
}
