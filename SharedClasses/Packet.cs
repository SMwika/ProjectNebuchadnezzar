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
        String user;
        DateTime date;
        String fileName;
        String fileHash;
        WatcherInfoType iType;

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
