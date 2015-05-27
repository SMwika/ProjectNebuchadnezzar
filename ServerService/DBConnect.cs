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
        private string server = "adamodrobina.com.pl";//"192.168.1.3";
        private List<string>[] list;
        private List<string>[] currencies;
        private List<string>[] shops;
        public DBConnect()
        {
            connString = "SERVER=" + server + ";PORT=3306;DATABASE=psr;UID=psr_user;PASSWORD=MisUszatek9;";

            conn = new MySqlConnection(connString);
            this.OpenConnection();
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

        private void ExecuteNonQuery(String query)
        {
            Console.WriteLine(query);
            //if (this.OpenConnection() == true)
            //{
                MySqlCommand cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

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
           using (MySqlDataReader dataReader = new MySqlCommand("SELECT MAX(id_files) FROM files", conn).ExecuteReader())
           {
               while (dataReader.Read())
               {
                   id = dataReader.GetInt32(0);
               }
           }
               
               this.ExecuteNonQuery(String.Format("INSERT INTO files(id_files, content) VALUES('" + id + 1 + "','" + content + "')"));
               //this.CloseConnection();

           //}
           return id+1;
        }

        public void addPacket(Packet p, String ip)
        {
            Console.WriteLine(addFiles("alejajaa"));
            string query = String.Format("INSERT INTO packet(user, date, fileName, filehash, iType, ip, oldFileName) VALUES('{0}', '{1}', '{2}', '{3}', {4}, '{5}', '{6}')",
                p.User, p.Date.ToString(), p.FileName, p.FileHash, (int)p.IType, ip, p.OldFileName);
            this.ExecuteNonQuery(query);
        }

        public void addLogs(String mess)
        {
            string query = String.Format("INSERT INTO logs(message, date) VALUES ('"+mess+"','"+System.DateTime.Now+"')");
            this.ExecuteNonQuery(query);
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
