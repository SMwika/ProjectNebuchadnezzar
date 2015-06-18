using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace ServerService
{
    /// <summary>
    /// implementation of WCF connetor inteface
    /// <seealso cref="SharedClasses.IServerConnector"/>
    /// </summary>
    class ServerConnector : SharedClasses.IServerConnector
    {
        /// <summary>
        /// gets list of unique file names from DB
        /// <seealso cref="SharedClasses.PacketDB"/>
        /// </summary>
        /// <returns>List of PacketDB objects</returns>
        public List<SharedClasses.PacketDB> GetUniqueFileNames()
        {
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetUniqueFileNames();
            db.Destroy();
            return list;
        }
        /// <summary>
        /// get list of unique filenames from given date
        /// </summary>
        /// <param name="dt">day to get from DB</param>
        /// <returns>List of PacketDB objects</returns>
        public List<SharedClasses.PacketDB> GetUniqueFileNamesByDate(string dt)
        {
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetUniqueFileNames(dt);
            db.Destroy();
            return list;
        }
        /// <summary>
        /// gets the contents of file from DB by given ID
        /// </summary>
        /// <param name="id">id of the file</param>
        /// <returns></returns>
        public String GetFileContents(int id)
        {
            DBConnect db = new DBConnect();
            String str = db.GetFileContents(id);
            db.Destroy();
            //Console.WriteLine(str);
            return str;
        }
        /// <summary>
        /// gets the list of file revisons from DB by given name
        /// </summary>
        /// <param name="name">name of the file</param>
        /// <returns>list of PacketDB obejcts</returns>
        public List<SharedClasses.PacketDB> GetFileRevisions(String name)
        {
            DBConnect db = new DBConnect();
            List<SharedClasses.PacketDB> list = db.GetFileRevisions(name);
            db.Destroy();
            return list;
        }
        /// <summary>
        /// gets the id of last file revision
        /// </summary>
        /// <param name="name">file name</param>
        /// <returns>id of the file</returns>
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
        /// <summary>
        /// gets the valid possible IP that can connect to Server service
        /// </summary>
        /// <returns>list of String IPs</returns>
        public List<String> GetValidConnections()
        {
            List<String> list = new List<String>();
            list = Server.validClients.ToList<String>();
            return list;
        }
        /// <summary>
        /// gets the logs from DB
        /// </summary>
        /// <returns>List of strings with logs</returns>
        public List<String> GetLogs()
        {
            DBConnect db = new DBConnect();
            List<String> list = db.GetAllLogs(-1);
            db.Destroy();
            return list;
        }
        /// <summary>
        /// prepares and calls the distibuted configurations injection
        /// </summary>
        public void injectConfig()
        {
            ConfigurationManager.RefreshSection("appSettings");
            SharedClasses.ConfigPacket cp = new SharedClasses.ConfigPacket();
            cp.ServerIP = ConfigurationManager.AppSettings["injectServerIP"];
            cp.ServerPort = ConfigurationManager.AppSettings["injectServerPort"];
            cp.WatcherDirectories = ConfigurationManager.AppSettings["injectWatcherDirectories"];
            cp.WatcherFilters = ConfigurationManager.AppSettings["injectWatcherFilters"];
            cp.WatcherIncludeSubdirectories = ConfigurationManager.AppSettings["injectIncludeSubdirectories"];
            cp.SerialNumber = ConfigurationManager.AppSettings["injectSerialNumber"];
            Server.InjectClientsConfiguration(cp);
        }
    }
}
