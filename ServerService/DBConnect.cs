using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.ComponentModel;
using SharedClasses;

namespace ServerService
{
    /// <summary>
    /// DataBase wrapper class
    /// </summary>
    class DBConnect
    {
        private MySqlConnection conn;
        private string connString;

        private string server = System.Configuration.ConfigurationManager.AppSettings["mySqlServer"];
        private string port = System.Configuration.ConfigurationManager.AppSettings["mySqlPort"];
        private string database = System.Configuration.ConfigurationManager.AppSettings["mySqlDatabase"];
        private string user = System.Configuration.ConfigurationManager.AppSettings["mySqlUsername"];
        private string password = System.Configuration.ConfigurationManager.AppSettings["mySqlPassword"];

        private List<string>[] list;
        private List<string>[] currencies;
        private List<string>[] shops;
        public DBConnect()
        {
            connString = "SERVER=" + "adamodrobina.com.pl" + ";PORT=3306;DATABASE=psr;UID=psr_user;PASSWORD=MisUszatek9;";
            //connString = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PASSWORD={4}", server, port, database, user, password);
            conn = new MySqlConnection(connString);
            //this.OpenConnection();
        }

        public void Destroy()
        {
            this.CloseConnection();
        }

        private bool OpenConnection()
        {
            try
            {
                conn.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server. Contact administrator");
                        break;
                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        private bool CloseConnection()
        {
            try
            {
                conn.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// executes the given query on DBMS
        /// </summary>
        /// <param name="query">query to execute</param>
        private void ExecuteNonQuery(String query)
        {
#if(DEBUG)
            Console.WriteLine(query);
#endif
            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// add file content record to DB
        /// </summary>
        /// <param name="content">content of the file to be written</param>
        /// <returns></returns>
        private int addFiles(String content)
        {
           int id = 0;
           using (MySqlConnection conn = new MySqlConnection(connString))
           {
               conn.Open();
               using (MySqlDataReader dataReader = new MySqlCommand("SELECT MAX(id_files) FROM files", conn).ExecuteReader())
               {
                   while (dataReader.Read())
                   {
                       id = dataReader.GetInt32(0);
                   }
               }
               id++;
               using (MySqlCommand comm = conn.CreateCommand())
               {
                   comm.CommandText = "INSERT INTO files(id_files, content) VALUES(@id, @content)";
                   comm.Parameters.AddWithValue("@id", id);
                   comm.Parameters.AddWithValue("@content", content);
                   comm.ExecuteNonQuery();
               }
           }
           return id;
        }
        /// <summary>
        /// add monitoring event to DB
        /// </summary>
        /// <param name="p">Packet object containing information about event</param>
        /// <param name="ip">IP address of event sender</param>
        public void addPacket(Packet p, String ip)
        {
            int id_file = -666;
            string query;
            if (p.IType == WatcherInfoType.FILE_CREATED | p.IType == WatcherInfoType.FILE_CHANGED)
            {
                id_file = this.addFiles(p.FileContent);
                query = String.Format("INSERT INTO packet(user, date, fileName, filehash, iType, ip, oldFileName, id_files) VALUES('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}', '{7}')",
                p.User, p.Date.ToString(), p.FileName, p.FileHash, (int)p.IType, ip, p.OldFileName, id_file);
            }
            else
            {
                query = String.Format("INSERT INTO packet(user, date, fileName, filehash, iType, ip, oldFileName) VALUES('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}')",
                p.User, p.Date.ToString(), p.FileName, p.FileHash, (int)p.IType, ip, p.OldFileName);
            }

            if (LookForPlagiarism(p.FileHash))
            {
                addLogs(String.Format("[{0}]Plagiarism detected on file {1}", ip, p.FileName), 1);
            }
            this.ExecuteNonQuery(query);
        }
        /// <summary>
        /// add log message to DB
        /// </summary>
        /// <param name="mess">message content</param>
        /// <param name="type">message type</param>
        public void addLogs(String mess, int type)
        {
            string query = String.Format("INSERT INTO logs(message, date, type) VALUES ('" + mess + "','" + System.DateTime.Now + "', {0})", type);
            this.ExecuteNonQuery(query);
        }
        /// <summary>
        /// <see cref="addLogs(String mess, int type)"/>
        /// </summary>
        /// <param name="mess"></param>
        public void addLogs(String mess)
        {
            addLogs(mess, 0);
        }
        
        public List<String> GetLogsFirstID(int fid)
        {
            List<String> logs = new List<String>();
            string query = String.Format("SELECT type, message FROM logs WHERE id_logs > {0}", fid);
            return logs;
        }

        /// <summary>
        /// gets all logs (or log by given ID) from DB
        /// used in GUI Log list
        /// </summary>
        /// <param name="id">id of given log (or -1 to get all logs)</param>
        /// <returns></returns>
        public List<String> GetAllLogs(int id)
        {
            string query;
            if (id < 0)
                query = "SELECT type, message, date FROM logs ORDER BY id_logs DESC";
            else
                query = "SELECT type, message, date FROM logs WHERE id_logs = id ORDER BY id_logs DESC";
            List<String> logs = new List<String>();

            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(String.Format("[{0}]({2}){1}", reader["type"] + "", reader["message"] + "", reader["date"] + ""));
                    }
                }
            }
            return logs;
        }
        /// <summary>
        /// checks the given hash in DB for plagiarism detecion
        /// extension method
        /// </summary>
        /// <param name="hash">hash of checked file</param>
        /// <returns></returns>
        private bool CheckIfLastDateByHashCouldBePlagiarism(String hash)
        {
            DateTime dtNow = DateTime.Now;
            string query = String.Format("SELECT date FROM packet WHERE filehash = '{0}' ORDER BY id_Packet DESC LIMIT 1", hash);
            using (MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string dt = reader.GetString(0);
                        DateTime dtThen = DateTime.Parse(dt);
                        if (dtNow.Subtract(dtThen).Minutes > 5)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// checks the given hash in DB for plagiarism detecion
        /// </summary>
        /// <param name="hash">hash of checked file</param>
        /// <returns></returns>
        public bool LookForPlagiarism(String hash)
        {
            string query = String.Format("SELECT COUNT(*) FROM packet WHERE filehash = '{0}'", hash);
            using (MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (0 < Convert.ToInt32(reader.GetString(0)))
                        {
                            if (CheckIfLastDateByHashCouldBePlagiarism(hash))
                                return true;
                            else
                                return false;
                        }
                        else
                        {
                            return false;
                        }
                    }
                        
                }
            }
            return false;
        }


        /// <summary>
        /// gets unique file names from DB
        /// <seealso cref="SharedClasses.PacketDB"/>
        /// </summary>
        /// <param name="date">date to check (if "NO_DATE" - it gets all file names</param>
        /// <returns>List of PacketDB objects</returns>
        public List<PacketDB> GetUniqueFileNames(String date)
        {
            string query;
            if (date == "NO_DATE")
            {
                query = "SELECT user, date, fileName, oldFileName, fileHash, iType, id_Packet, id_files, ip, COUNT(*) as count FROM packet GROUP BY fileName ORDER BY id_Packet DESC";
            }
            else
            {
                query = String.Format("SELECT user, date, fileName, oldFileName, fileHash, iType, id_Packet, id_files, ip, COUNT(*) as count FROM packet WHERE DATE(date) = '{0}' GROUP BY fileName ORDER BY id_Packet DESC", date);
            }
            List<PacketDB> packets = new List<PacketDB>();
            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id_files = -666;
                        string oldFileName = "NULL";
                        if (!(reader["id_files"] is DBNull)) id_files = Convert.ToInt32(reader["id_files"]);
                        if (!(reader["oldFileName"] is DBNull)) oldFileName = reader["oldFileName"] + "";
                        PacketDB pack = new PacketDB(new Packet(reader["user"] + "",
                            DateTime.ParseExact(reader["date"] + "", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            reader["fileName"] + "", oldFileName, reader["fileHash"] + "", (WatcherInfoType)reader["iType"], 1),
                            Convert.ToInt32(reader["id_Packet"]),
                            Convert.ToInt32(id_files),
                            Convert.ToInt32(reader["count"]),
                            reader["ip"] + "");
                        packets.Add(pack);
                    }
                }
            }
            
            return packets;
        }

        /// <summary>
        /// gets file revisions of given file name
        /// </summary>
        /// <param name="name">file name to check</param>
        /// <returns>list of PacketDB objects</returns>
        public List<PacketDB> GetFileRevisions(String name)
        {
            string query = String.Format("SELECT user, date, fileName, oldFileName, fileHash, iType, id_Packet, id_files, ip FROM packet WHERE fileName = '{0}' ORDER BY date DESC", name);
#if(DEBUG)
            Console.WriteLine(query);
#endif
            List<PacketDB> packets = new List<PacketDB>();
            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id_files = -666;
                        string oldFileName = "NULL";
                        if (!(reader["id_files"] is DBNull)) id_files = Convert.ToInt32(reader["id_files"]);
                        if (!(reader["oldFileName"] is DBNull)) oldFileName = reader["oldFileName"] + "";
                        PacketDB pack = new PacketDB(new Packet(reader["user"] + "",
                            DateTime.ParseExact(reader["date"] + "", "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                            reader["fileName"] + "", oldFileName, reader["fileHash"] + "", (WatcherInfoType)reader["iType"], 1),
                            Convert.ToInt32(reader["id_Packet"]),
                            Convert.ToInt32(id_files),
                            Convert.ToInt32(0),
                            reader["ip"] + "");
                        packets.Add(pack);
                    }
                }
            }
            return packets;
        }
        /// <summary>
        /// gets the last revision id of the given file name
        /// </summary>
        /// <param name="name">file name to check</param>
        /// <returns>id of the file</returns>
        public int GetLastRevisionID(String name)
        {
            List<PacketDB> list = GetFileRevisions(name);
            int id = list.Max(x => x.Id_files);
            return id;
        }

        /// <summary>
        /// gets file contents from databaase
        /// </summary>
        /// <param name="id">ID of the file</param>
        /// <returns>String containing contents of the file</returns>
        public String GetFileContents(int id)
        {
            string query = String.Format("SELECT content FROM files WHERE id_files = '{0}'", id);
            string content = "";
            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        content = reader.GetString(0);
                    }
                }
            }
            return content;            
        }

        /// <summary>
        /// <see cref="GetUniqueFileNames(String date)"/>
        /// </summary>
        /// <returns></returns>
        public List<PacketDB> GetUniqueFileNames()
        {
            return GetUniqueFileNames("NO_DATE");
        }

    }
}
