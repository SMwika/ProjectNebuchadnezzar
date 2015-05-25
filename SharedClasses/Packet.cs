using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharedClasses
{
    [Serializable()]
    public class Packet
    {
        public String user {get; set;}
        public DateTime date { get; set; }
        public String fileName { get; set; }
        public String fileHash { get; set; }
        public WatcherInfoType iType { get; set; }

        public Packet(String usr, DateTime dt, String name, String hash, WatcherInfoType type)
        {
            this.user = usr;
            this.date = dt;
            this.fileName = name;
            this.fileHash = hash;
            this.iType = type;
        }

        public string getString()
        {
            
            return "" + user + " " + iType + " " + date.ToString() + " " + fileName + " " + fileHash;
        }
    }
}
