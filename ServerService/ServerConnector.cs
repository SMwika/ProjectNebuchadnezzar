using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerService
{
    class ServerConnector : SharedClasses.IServerConnector
    {
        public List<SharedClasses.PacketDB> GetUniqueFileNames()
        {
            return new DBConnect().GetUniqueFileNames();
        }

        public List<SharedClasses.PacketDB> GetUniqueFileNamesByDate(string dt)
        {
            return new DBConnect().GetUniqueFileNames(dt);
        }

        public string GetFileContents(int id)
        {
            string str = new DBConnect().GetFileContents(id);
            //Console.WriteLine(str);
            return str;
        }
    }
}
