﻿using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Formatters.Soap;
using System.Security.Cryptography;
using System.Text;
using SharedClasses;
using System.Management;
using System.Net.Sockets;
using System.IO;
using System.Net;
using System;

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
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).BeginInit();
            // 
            // NebuchadnezzarClient
            // 
            this.AutoLog = false;
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.eventLog1)).EndInit();

        }

        private void InitWatchers()
        {
            System.IO.FileSystemWatcher watcher = null;
            System.String[] paths = System.Configuration.ConfigurationManager.AppSettings["folderPath"].Split(';');
            System.String[] filters = System.Configuration.ConfigurationManager.AppSettings["fileFilter"].Split(';');
            //System.Console.WriteLine(paths);
            //System.Console.WriteLine(filters);
            this.watchers = new System.Collections.Generic.List<System.IO.FileSystemWatcher>();
            foreach (System.String path in paths)
            {
                if (path == "") continue;
                foreach (System.String filter in filters)
                {
                    watcher = new System.IO.FileSystemWatcher();
                    ((System.ComponentModel.ISupportInitialize)(watcher)).BeginInit();
                    watcher.Path = path;
                    watcher.Filter = filter;
                    watcher.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.Size | System.IO.NotifyFilters.DirectoryName)));
                    watcher.Changed += new System.IO.FileSystemEventHandler(this.watcherChanged);
                    watcher.Created += new System.IO.FileSystemEventHandler(this.watcherCreated);
                    watcher.Deleted += new System.IO.FileSystemEventHandler(this.watcherDeleted);
                    watcher.Renamed += new System.IO.RenamedEventHandler(this.watcherRenamed);
                    watcher.EnableRaisingEvents = true;
                    watcher.IncludeSubdirectories = (System.Configuration.ConfigurationManager.AppSettings["includeSubDirs"] == "true") ? true : false;
                    ((System.ComponentModel.ISupportInitialize)(watcher)).EndInit();
                    this.watchers.Add(watcher);
                }
            }
        }


        #endregion

        private System.Diagnostics.EventLog eventLog1;
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
#if(DEBUG)
                System.Console.WriteLine("StreamClosed");
#endif
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
            SendObject(new Packet(this.getCurrentUser(), System.DateTime.Now, "USER", "USER", WatcherInfoType.FILE_DELETED));
            foreach (Packet p in list)
            {
                SendObject(p);
                //list.Remove(p);
            }
            list.Clear();
        }


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

        private object ReceiveObject(Socket sock)
        {
            if (!sock.Connected) return null;
            NetworkStream stream = new NetworkStream(sock);
            stream.ReadTimeout = 1000;
            IFormatter formatter = new BinaryFormatter();
            try
            {
                object o = (object)formatter.Deserialize(stream);
                return o;
            }
            catch (SocketException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            return null;
        }

        private string getCurrentUser()
        {
            ManagementObjectSearcher mos = new ManagementObjectSearcher("Select * from Win32_Process Where Name = \"explorer.exe\"");
            ManagementObjectCollection moc = mos.Get();
            foreach (ManagementObject obj in moc)
            {
                string[] argList = new string[] { string.Empty, string.Empty };
                int ret = System.Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (0 == ret)
                {
                    string owner = argList[1] + "\\\\" + argList[0];
                    return owner;
                }
            }
            return "NO_USER";
        }

        #region FileSystemWatcher_Events
        private void watcherChanged(object sender, System.IO.FileSystemEventArgs e)
        {
            //System.Console.WriteLine("Changed file " + e.FullPath + " " + GetFileHash(e.FullPath));
            this.SendObject(new Packet(getCurrentUser(), System.DateTime.Now, e.FullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_CHANGED));
        }

        private void watcherDeleted(object sender, System.IO.FileSystemEventArgs e)
        {
            eventLog1.WriteEntry("Deleted file " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
#if(DEBUG)
            System.Console.WriteLine("Deleted file " + e.FullPath);
#endif
            this.SendObject(new Packet(getCurrentUser(), System.DateTime.Now, e.FullPath, "", WatcherInfoType.FILE_DELETED));
        }

        private void watcherCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            //xxx++;
            //eventLog1.WriteEntry("Created file " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
            //System.Console.WriteLine("Created file " + e.FullPath + " " + GetFileHash(e.FullPath));
            //int bytesSent = this.Send("Created file " + e.Name + "<EOF>");
            this.SendObject(new Packet(getCurrentUser(), System.DateTime.Now, e.FullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_CREATED));

        }

        private void watcherRenamed(object sender, System.IO.RenamedEventArgs e)
        {
            eventLog1.WriteEntry("Renamed file " + e.OldFullPath + " to " + e.FullPath, System.Diagnostics.EventLogEntryType.Information);
#if(DEBUG)
            System.Console.WriteLine("Renamed file " + e.OldFullPath + " to " + e.FullPath);
#endif
            this.SendObject(new Packet(getCurrentUser(), System.DateTime.Now, e.FullPath, e.OldFullPath, GetFileHash(e.FullPath), WatcherInfoType.FILE_RENAMED));
        }
        #endregion
    }
}
