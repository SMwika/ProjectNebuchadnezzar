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



        public List<string>[] getGagdets()
        {
            return list;
        }

        public List<string>[] getCurrencies()
        {
            return currencies;
        }

        public List<string>[] getShops()
        {
            return shops;
        }

        /*
         * name, price_int, link, category, currency, image_link, shop
         */

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        private void ExecuteNonQuery(String query)
        {
            Console.WriteLine(query);
            //if (this.OpenConnection() == true)
            //{
            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                //MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
                

                //this.CloseConnection();
            //}
        }

        public void AddShop(String sname)
        {
            string query = String.Format("INSERT INTO shops(sname) VALUES('{0}');", sname);
            this.ExecuteNonQuery(query);
        }


        private int addFiles(String content)
        {
           int id = 0;
           //if (this.OpenConnection() == true)
           //{
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

        public void addPacket(Packet p, String ip)
        {
          //  Console.WriteLine(addFiles("alejajaa"));
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

        public void addLogs(String mess, int type)
        {
            string query = String.Format("INSERT INTO logs(message, date, type) VALUES ('" + mess + "','" + System.DateTime.Now + "', {0})", type);
            this.ExecuteNonQuery(query);
        }

        public void addLogs(String mess)
        {
            addLogs(mess, 0);
        }

        public void Insert(List<String> ins)
        {
            string query = String.Format("INSERT INTO gadgetList(gname, price_int, link, category, cash_curr, image_link, shop, promote, shop_id) VALUES('{0}', {1}, '{2}', {3}, {4}, '{5}', '{6}', {7}, {8});", ins[0], ins[1], ins[2], ins[3], ins[4], ins[5], ins[6], ins[7], ins[8]);

            this.ExecuteNonQuery(query);
        }
        public void Update(List<String> upd, string id)
        {
            string query = String.Format("UPDATE gadgetList SET gname='{0}', price_int={1}, link='{2}', category={3}, cash_curr={4}, image_link='{5}', shop='{6}', promote={8}, shop_id={9} WHERE id={7};", upd[0], upd[1], upd[2], upd[3], upd[4], upd[5], upd[6], id, upd[7], upd[8]);

            this.ExecuteNonQuery(query);
        }

        public void Delete(string id)
        {
            string query = String.Format("DELETE FROM gadgetList WHERE id={0};", id);

            this.ExecuteNonQuery(query);
        }

        public List<String> GetAllLogs(int id)
        {
            string query;
            if (id < 0)
                query = "SELECT type, message FROM logs ORDER BY id_logs DESC";
            else
                query = "SELECT type, message FROM logs WHERE id_logs = id ORDER BY id_logs DESC";
            List<String> logs = new List<String>();

            using(MySqlConnection conn = new MySqlConnection(connString))
            using (MySqlCommand cmd = new MySqlCommand(query, conn))
            {
                conn.Open();
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        logs.Add(String.Format("[{0}]{1}", reader["type"] + "", reader["message"] + ""));
                    }
                }
            }
            return logs;
        }

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


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public List<PacketDB> GetFileRevisions(String name)
        {
            string query = String.Format("SELECT user, date, fileName, oldFileName, fileHash, iType, id_Packet, id_files, ip FROM packet WHERE fileName = '{0}' ORDER BY date DESC", name);
            Console.WriteLine(query);
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

        public int GetLastRevisionID(String name)
        {
            List<PacketDB> list = GetFileRevisions(name);
            int id = list.Max(x => x.Id_files);
            return id;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
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

        public List<PacketDB> GetUniqueFileNames()
        {
            return GetUniqueFileNames("NO_DATE");
        }
        public int Select()
        {
            //string query = "SELECT * FROM gadgetList";
            string query = "SELECT g.id, gname, price_int, link, category, shop, sname, currency, image_link, promote FROM gadgetList g INNER JOIN currencies c ON g.cash_curr = c.id INNER JOIN shops s ON g.shop_id = s.id_shop ORDER BY gname ASC;";
            string queryCurrency = "SELECT * FROM currencies";
            string queryShop = "SELECT * FROM shops";
            list = new List<string>[9];
            list[0] = new List<string>();
            list[1] = new List<string>();
            list[2] = new List<string>();
            list[3] = new List<string>();
            list[4] = new List<string>();
            list[5] = new List<string>();
            list[6] = new List<string>();
            list[7] = new List<string>();
            list[8] = new List<string>();
            currencies = new List<string>[2];
            currencies[0] = new List<string>();
            currencies[1] = new List<string>();
            shops = new List<string>[2];
            shops[0] = new List<string>();
            shops[1] = new List<string>();


            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, conn);
                MySqlDataReader dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["gname"] + "");
                    list[2].Add(dataReader["price_int"] + "");
                    list[3].Add(dataReader["link"] + "");
                    list[4].Add(dataReader["category"] + "");
                    list[5].Add(dataReader["currency"] + "");
                    list[6].Add(dataReader["image_link"] + "");
                    list[7].Add(dataReader["sname"] + "");
                    list[8].Add(dataReader["promote"] + "");
                }

                dataReader.Close();

                cmd = new MySqlCommand(queryCurrency, conn);
                dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    currencies[0].Add(dataReader["id"] + "");
                    currencies[1].Add(dataReader["currency"] + "");
                }

                dataReader.Close();

                cmd = new MySqlCommand(queryShop, conn);
                dataReader = cmd.ExecuteReader();

                while (dataReader.Read())
                {
                    shops[0].Add(dataReader["id_shop"] + "");
                    shops[1].Add(dataReader["sname"] + "");
                }

                dataReader.Close();

                this.CloseConnection();

                return 1;
            }
            else
            {
                return 0;
            }

        }

    }
}
