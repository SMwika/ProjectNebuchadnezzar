using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable()]
    public class ConfigPacket
    {
        private String serverIP { get; set; }
        private String serverPort { get; set; }
        private String watcherDirectories { get; set; }
        private String watcherFilters { get; set; }
        private String watcherIncludeSubdirectories { get; set; }
        private String serialNumber { get; set; }

        public String ServerIP
        {
            get
            {
                return this.serverIP;
            }
            set
            {
                this.serverIP = value;
            }
        }

        public String ServerPort
        {
            get
            {
                return this.serverPort;
            }
            set
            {
                this.serverPort = value;
            }
        }

        public String WatcherDirectories
        {
            get
            {
                return this.watcherDirectories;
            }
            set
            {
                this.watcherDirectories = value;
            }
        }

        public String WatcherFilters
        {
            get
            {
                return this.watcherFilters;
            }
            set
            {
                this.watcherFilters = value;
            }
        }

        public String WatcherIncludeSubdirectories
        {
            get
            {
                return this.watcherIncludeSubdirectories;
            }
            set
            {
                this.watcherIncludeSubdirectories = value;
            }
        }

        public String SerialNumber
        {
            get
            {
                return this.serialNumber;
            }
            set
            {
                this.serialNumber = value;
            }
        }

    }
}
