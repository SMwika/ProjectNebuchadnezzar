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
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetUniqueFileNames();
            db.Destroy();
            return list;
        }

        public List<SharedClasses.PacketDB> GetUniqueFileNamesByDate(string dt)
        {
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetUniqueFileNames(dt);
            db.Destroy();
            return list;
        }

        public String GetFileContents(int id)
        {
            DBConnect db = new DBConnect();
            String str = db.GetFileContents(id);
            db.Destroy();
            //Console.WriteLine(str);
            return str;
        }

        public List<SharedClasses.PacketDB> GetFileRevisions(String name)
        {
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetFileRevisions(name);
            db.Destroy();
            return list;
        }

        public int GetLastRevisionID(String name)
        {
            DBConnect db = new DBConnect();
            int id = db.GetLastRevisionID(name);
            db.Destroy();
            return id;
        }

        public List<String> GetActiveConnections()
        {
            List<String> list = new List<String>();
            list = Server.clientList;
            return list;
        }

        public List<String> GetValidConnections()
        {
            List<String> list = new List<String>();
            list = Server.validClients.ToList<String>();
            return list;
        }

        public List<String> GetLogs()
        {
            DBConnect db = new DBConnect();
            List<String> list = db.GetAllLogs(-1);
            db.Destroy();
            return list;
        }
    }
}
