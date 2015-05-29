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

        public String GetFileContents(int id)
        {
            String str = new DBConnect().GetFileContents(id);
            //Console.WriteLine(str);
            return str;
        }

        public List<SharedClasses.PacketDB> GetFileRevisions(String name)
        {
            return new DBConnect().GetFileRevisions(name);
        }

        public int GetLastRevisionID(String name)
        {
            return new DBConnect().GetLastRevisionID(name);
        }
    }
}
