using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharedClasses
{
    [Serializable()]
    public class PacketDB : Packet
    {
        private int id_packet { get; set; }
        private int id_files { get; set; }
        private int count { get; set; }
        private string ipAddress { get; set; }
        public int Id_packet
        {
            get
            {
                return this.id_packet;
            }
        }
        public int Id_files
        {
            get
            {
                return this.id_files;
            }
        }
        public int Count
        {
            get
            {
                return this.count;
            }
        }
        public string IpAddress
        {
            get
            {
                return this.ipAddress;
            }
        }

        public PacketDB(Packet pack, int id_packet, int id_files, int count, string ip) 
            : base(pack.User, pack.Date, pack.FileName, pack.OldFileName, pack.FileHash, pack.IType, 1)
        {
            this.id_packet = id_packet;
            this.id_files = id_files;
            this.count = count;
            this.ipAddress = ip;
        }
    }
}
