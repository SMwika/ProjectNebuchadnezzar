using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;

namespace SharedClasses
{
    /// <summary>
    /// Serializable packet to transmit primary informations about Client events
    /// </summary>
    [Serializable()]
    public class Packet
    {
        private String user {get; set;}
        private DateTime date { get; set; }
        private String fileName { get; set; }
        private String oldFileName { get; set; }
        private String fileHash { get; set; }
        private WatcherInfoType iType { get; set; }
        private String fileContent { get; set; }

        public String FileContent
        {
            get
            {
                return this.fileContent;
            }
        }
        public String User{
            get
            {
                return this.user;
            }
        }

        public DateTime Date
        {
            get
            {
                return this.date;
            }
        }

        public String FileName
        {
            get
            {
                return this.fileName;
            }
        }

        public String OldFileName
        {
            get
            {
                return this.oldFileName;
            }
        }

        public String FileHash
        {
            get
            {
                return this.fileHash;
            }
        }

        public WatcherInfoType IType
        {
            get
            {
                return this.iType;
            }
        }

        public Packet(String usr, DateTime dt, String name, String oldName, String hash, WatcherInfoType type, int sender)
            : this(usr, dt, name, hash, type, sender)
        {
            if (0 == sender)
            {
                this.oldFileName = oldName.Replace("\\", "\\\\");
            }
            else
            {
                this.oldFileName = oldName;
            }
        }
        public Packet(String usr, DateTime dt, String name, String oldName, String hash, WatcherInfoType type)
            : this(usr, dt, name, oldName, hash, type, 0)
        {
            //this.oldFileName = oldName.Replace("\\", "\\\\");
        }
        public Packet(String usr, DateTime dt, String name, String hash, WatcherInfoType type) 
            : this(usr, dt,name, hash, type, 0)
        {

        }
        public Packet(String usr, DateTime dt, String name, String hash, WatcherInfoType type, int sender)
        {
            if (0 == sender)
            {
                this.user = usr;
                this.date = dt;
                this.fileName = name.Replace("\\", "\\\\");
                this.fileHash = hash;
                this.iType = type;
                if(!IsExitPacket())
                    if (type == WatcherInfoType.FILE_CREATED | type == WatcherInfoType.FILE_CHANGED) this.fileContent = getFileContents(name);
            }
            else
            {
                this.user = usr;
                this.date = dt;
                this.fileName = name;
                this.fileHash = hash;
                this.iType = type;
            }
        }

        public string getString()
        {
            
            return "" + user + " " + iType + " " + date.ToString() + " " + fileName + " " + fileHash;
        }

        public override string ToString()
        {
            return this.getString();
        }

        private String getFileContents(String path)
        {
            return System.IO.File.ReadAllText(path, Encoding.GetEncoding("ISO-8859-2"));
        }

        public bool IsExitPacket()
        {
            if ((this.user == "exit") && (this.fileName == "exit") && (this.fileHash == "exit")) return true;
            else return false;
        }
    }
}
