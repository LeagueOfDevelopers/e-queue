using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Server
{
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, prop7, prop8, prop9, SetCg, Refc1, Refc2, GetMa, GetMH, GetRe, GetRH, GetFa, GetFH, GetCM, GetCF, GetRF, GetCg, GetRN, GetMT, GetMC, GetFC, GetRC, GeMHC, GeFHC, GeRHC, Updat, EOSes, enumErr }
    class Start
    {
        static void Main(string[] args)
        {
            //разбор аргументов, и запуск сервера
            IPAddress ip = IPAddress.Parse(args[0]);
            int port = int.Parse(args[1]);
            Server sv = new Server(ip, port);
        }
    }
    class Server
    {
        TcpListener Listener;
        static public DataBase DataBase;
        public Server(IPAddress addr, int port)
        {
            int SizeOfHistory = 100000;
            DataBase = new DataBase(SizeOfHistory);//инициализируем базу
            Listener = new TcpListener(addr, port);//создаем слушатель, запускаем его
            Listener.Start();
            Console.WriteLine("Сервер запущен.\nАдрес: {0}", Listener.LocalEndpoint);
            Console.WriteLine("Для выхода нажмите клавишу ESC");
            Thread Thread1 = new Thread(new ParameterizedThreadStart(Managing));//поток работающий с клавиатурой
            Thread1.Start();
            while (true)
            {
                Socket Client = Listener.AcceptSocket();//ожидание подключенного пользователя
                Console.WriteLine("Установленно соединение с {0}", Client.RemoteEndPoint);
                Thread Thread2 = new Thread(new ParameterizedThreadStart(ClientThread));//создание и запуск нового потока, передаем ему полученного клиента
                Thread2.Start(Client);
            }
        }
        static void ClientThread(Object StateInfo)
        {
            new AcceptedClient((Socket)StateInfo);
        }
        void Managing(object obj)
        {
            while (true)
                SwitchKeyInput(Console.ReadKey());
        }
        public static requests ParseReq(string value)// Parse из string в request
        {
            try
            {
                return (requests)Enum.Parse(typeof(requests), value);
            }
            catch { return requests.enumErr; }
        }
        void SwitchKeyInput(ConsoleKeyInfo k)
        {
            switch (k.Key)
            {
                case ConsoleKey.Escape:
                    File.SetAttributes(Environment.CurrentDirectory + "\\save\\database.txt", FileAttributes.Normal);
                    File.SetAttributes(Environment.CurrentDirectory + "\\save\\histbase.txt", FileAttributes.Normal);
                    Listener.Stop();
                    Environment.Exit(0);
                    break;
                case ConsoleKey.A:
                    break;
                default:
                    return;
            }
        }
    }
    class AcceptedClient
    {
        Socket Client;
        EndPoint EndPoint;
        byte[] Buffer;
        public AcceptedClient(Socket Cl)//работа с клиентом
        {
            Client = Cl;
            EndPoint = Client.RemoteEndPoint;
            Buffer = new byte[1024];
            bool q = true;
            try
            {
                while (q)
                {
                    string b;
                    string a = Receive(out b);
                    q = SwitchReq(a, b);
                }
            }
            catch{ Console.WriteLine("Ошибка в соединении."); }
            Client.Close();
            Console.WriteLine("Соединение с {0} завершено", EndPoint);
        }
        bool SwitchReq(string Request, string b)
        {
            if (Request.Length == 0)
                return true;
            Console.WriteLine(Request + "   " + DateTime.Now + "  " + b);
            string a;
            switch (Server.ParseReq(Request))
            {
                case requests.prop1:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop2:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop3:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop4:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop5:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop6:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop7:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop8:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.prop9:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.SetCg:
                    Server.DataBase.Add(int.Parse(Receive(out a)), Request);
                    break;
                case requests.Refc1:
                    Server.DataBase.AddRef(int.Parse(Receive(out a)), Receive(out a), 1);
                    break;
                case requests.Refc2:
                    Server.DataBase.AddRef(int.Parse(Receive(out a)), Receive(out a), 2);
                    break;

                case requests.GetMa:
                    Send(Server.DataBase.GetMain());
                    break;
                case requests.GetMH:
                    Send(Server.DataBase.GetMainHist());
                    break;
                case requests.GetRe:
                    Send(Server.DataBase.GetReference());
                    break;
                case requests.GetRH:
                    Send(Server.DataBase.GetReferenceHist());
                    break;
                case requests.GetFa:
                    Send(Server.DataBase.GetFast());
                    break;
                case requests.GetFH:
                    Send(Server.DataBase.GetFastHist());
                    break;
                case requests.GetCM:
                    Send(Server.DataBase.GetClientMain(int.Parse(Receive(out a))).ToString());
                    break;
                case requests.GetCF:
                    Send(Server.DataBase.GetClientFast(int.Parse(Receive(out a))).ToString());
                    break;
                case requests.GetRF:
                    Send(Server.DataBase.GetReferenceOne(int.Parse(Receive(out a))));
                    break;
                case requests.GetCg:
                    Send(Server.DataBase.GetConfig());
                    break;
                case requests.GetRN:
                    Send(Server.DataBase.GetRandomNumber().ToString());
                    break;
                case requests.GetMT:
                    Send(Server.DataBase.GetMiddleTime().ToString());
                    break;
                case requests.GetMC:
                    Send(Server.DataBase.GetCountMain().ToString());
                    break;
                case requests.GetFC:
                    Send(Server.DataBase.GetCountFast().ToString());
                    break;
                case requests.GetRC:
                    Send(Server.DataBase.GetCountReference().ToString());
                    break;
                case requests.GeMHC:
                    Send(Server.DataBase.GetCountMainHist().ToString());
                    break;
                case requests.GeFHC:
                    Send(Server.DataBase.GetCountFastHist().ToString());
                    break;
                case requests.GeRHC:
                    Send(Server.DataBase.GetCountReferenceHist().ToString());
                    break;

                case requests.Updat:
                    //Send(Server.DataBase.Update(int.Parse(Receive()), int.Parse(Receive())));
                    break;

                case requests.EOSes:
                    return false;
            }
            return true;
        }
        void Send(string msg)
        {
            byte[] buf1 = Encoding.Unicode.GetBytes(msg);
            msg = buf1.Length.ToString("D6");
            byte[] buf2 = Encoding.ASCII.GetBytes(msg);
            Client.Send(buf2, 6, SocketFlags.None);
            Client.Send(buf1, buf1.Length, SocketFlags.None);
        }
        string Receive(out string a)
        {
            byte[] buf = new byte[6];
            Client.Receive(buf, 6, SocketFlags.None);
            int msgsize = int.Parse(Encoding.ASCII.GetString(buf));
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Client.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            a = Client.RemoteEndPoint.ToString();
            return Encoding.Unicode.GetString(buf);
        }
    }
    class DataBase
    {
        SQLiteConnection Connection;
        SQLiteCommand cmd;
        string[] config;
        string pathDB;
        string pathConfig;
        int SizeOfHistory;

        public DataBase(int s)
        {
            config = new string[10];
            SizeOfHistory = s;
            pathDB = Environment.CurrentDirectory + "\\database.db";
            pathConfig = Environment.CurrentDirectory + "\\config.txt";
            ConnectingToDB();
            LoadConfig();
        }

        void ConnectingToDB()
        {
            string ConnCommand = String.Format("Data Source={0:s}; Version=3;", pathDB);
            if (File.Exists(pathDB))
            {
                Connection = new SQLiteConnection(ConnCommand);
            }
            else
            {
                Connection = new SQLiteConnection(ConnCommand);
                Connection.Open();
                cmd = Connection.CreateCommand();
                cmd.CommandText = "CREATE TABLE main(id INTEGER PRIMARY KEY AUTOINCREMENT, number INTEGER, purpose TEXT, toenter TEXT);" +
                                  "CREATE TABLE mainhist(id INTEGER PRIMARY KEY, number INTEGER, purpose TEXT, toenter TEXT, toexit INTEGER);" +
                                  "CREATE TABLE reference (id integer PRIMARY KEY AUTOINCREMENT, number integer, purpose text, toenter TEXT,priority integer,status integer);" +
                                  "CREATE TABLE referencehist (id integer PRIMARY KEY, number integer, purpose text, toenter TEXT, toexit TEXT, priority integer, status integer);" +
                                  "CREATE TABLE fast (id integer PRIMARY KEY AUTOINCREMENT,number integer,purpose text,toenter text);" + 
                                  "CREATE TABLE fasthist (id integer PRIMARY KEY AUTOINCREMENT,number integer,purpose text,toenter text,toexit text);";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (SQLiteException ex)
                {
                    Console.WriteLine(ex.Message);
                }
                Connection.Close();
            }
                
        }
        void LoadConfig()
        {
            try
            {
                StreamReader sr = new StreamReader(pathConfig, Encoding.GetEncoding(1251));
                for (int i = 0; i < 10; i++)
                    config[i] = sr.ReadLine();
                sr.Close();
                File.SetAttributes(pathConfig, FileAttributes.ReadOnly);
            }
            catch (FileNotFoundException)
            {
                File.CreateText(pathConfig);
                File.SetAttributes(pathConfig, FileAttributes.ReadOnly);
            }
            catch
            {
                config = new string[10];
                try
                {
                    StreamWriter swrt = new StreamWriter(pathConfig);
                    swrt.Close();
                }
                catch
                {
                    Console.WriteLine(@"Файл конфигурации не может быть загружен. Ограничен доступ(\save\config.txt).");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                File.SetAttributes(pathConfig, FileAttributes.ReadOnly);
                Console.WriteLine("Файл загрузки поврежден, создан новый.");
            }
        }
        
        void InsertInDB(string table, string value)
        {
            cmd = Connection.CreateCommand();
            switch (table)
            {
                case "main":
                    cmd.CommandText = String.Format("INSERT INTO main(number, purpose, toenter) VALUES ({0:s});", value);
                    break;
                case "mainhist":
                    InsertInDBhist("mainhist", value, "INSERT INTO {0:s}(id, number, purpose, toenter, toexit) VALUES ({1:s});");
                    return;
                case "reference":
                    cmd.CommandText = String.Format("INSERT INTO reference(number, purpose, toenter, priority, status) VALUES ({0:s});", value);
                    break;
                case "referencehist":
                    InsertInDBhist("referencehist", value, "INSERT INTO {0:s}(id, number, purpose, toenter, toexit, priority, status) VALUES ({1:s});");
                    return;
                case "fast":
                    cmd.CommandText = String.Format("INSERT INTO fast(number, purpose, toenter) VALUES ({0:s});", value);
                    break;
                case "fasthist":
                    InsertInDBhist("fasthist", value, "INSERT INTO {0:s}(id, number, purpose, toenter, toexit) VALUES ({1:s});");
                    return;
            }
            Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Connection.Close();
        }
        void InsertInDBhist(string table, string value, string command)
        {
            try
            {
                int s = GetCount(table);
                cmd = Connection.CreateCommand();

                if (s > SizeOfHistory) 
                {
                    cmd.CommandText = String.Format("SELECT * FROM {0:s};", table); 
                    Connection.Open();
                    SQLiteDataReader r = cmd.ExecuteReader();
                    r.Read();
                    int num = int.Parse(r.GetValue(1).ToString());
                    Connection.Close();
                    RemoveFromDB(table, num);
                }
                cmd.CommandText = String.Format(command, table, value);
                Connection.Open();
                cmd.ExecuteNonQuery();
                Connection.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        void RemoveFromDB(string table, int number)
        {
            cmd = Connection.CreateCommand();
            cmd.CommandText = String.Format("DELETE FROM {0:s} WHERE number={1:d}",table, number);
            Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Connection.Close();
        }
        string SelectFromDB(string table, int number)
        {
            string line= "";
            cmd = Connection.CreateCommand();
            cmd.CommandText = String.Format("SELECT * FROM {0:s} WHERE number = {1:d};", table, number);
            Connection.Open();
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                        line += r.GetValue(i).ToString() + "_";
                    line += ";";
                }
                r.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Connection.Close();
            if (line != "")
                line = line.Substring(0, line.Length - 2);
            else line = " ";
            return line;
        }
        string GetTable(string table)
        {
            string line = "";
            cmd = Connection.CreateCommand();
            cmd.CommandText = String.Format("SELECT * FROM {0:s};", table);
            Connection.Open();
            try
            {
                SQLiteDataReader r = cmd.ExecuteReader();
                while (r.Read())
                {
                    for (int i = 0; i < r.FieldCount; i++)
                    {
                        line += r.GetValue(i);
                        if (i != r.FieldCount - 1)
                            line += "_";
                    }
                    line += ";";
                }
                r.Close();
            }
            catch (SQLiteException ex)
            {
                Console.WriteLine(ex.Message);
            }
            Connection.Close();
            if (line != "")
                line = line.Substring(0, line.Length - 1);
            else line = " ";
            return line;
        }
        public string Update(int p, int stat)
        {
            cmd = Connection.CreateCommand();
            cmd.CommandText = String.Format("UPDATE reference SET status={0:s} WHERE number={1:d};", stat, p);
            Connection.Open();
            try
            {
                cmd.ExecuteNonQuery();
                Connection.Close();
            }
            catch (SQLiteException ex)
            {
                Connection.Close();
                Console.WriteLine(ex.Message);
                return "good";
            }
            return "bad";
        }
        public void SetConfig(string s)
        {
            File.SetAttributes(pathConfig, FileAttributes.Normal);
            try
            {
                config = s.Split(';');
                StreamWriter sw = new StreamWriter(pathConfig, false, Encoding.GetEncoding(1251));
                for (int i = 0; i < 7; i++)
                    sw.WriteLine(config[i]);
                sw.Close();
            }
            catch { Console.WriteLine("Сохранить данные не удалось"); }
            File.SetAttributes(pathConfig, FileAttributes.ReadOnly);
        }
        
        public int GetRandomNumber()
        {
            int num;
            Random r = new Random();
            while (true)
            {
                num = r.Next(1000, 9999);
                cmd = Connection.CreateCommand();
                cmd.CommandText = String.Format("SELECT count(rowid) FROM main WHERE number='{0:d}'", num);
                Connection.Open();
                int countRows = int.Parse(cmd.ExecuteScalar().ToString());
                Connection.Close();
                if (countRows == 0)
                {
                    break;
                }
            }
            return num;
        }
        public string GetMain()
        {
            return GetTable("main");
        }
        public string GetMainHist()
        {
            return GetTable("mainhist");
        }
        public string GetFast()
        {
            return GetTable("fast");
        }
        public string GetFastHist()
        {
            return GetTable("fasthist");
        }
        public string GetReference()
        {
            return GetTable("reference");
        }
        public string GetReferenceHist()
        {
            return GetTable("referencehist");
        }
        public int GetCountMain()
        {
            return GetCount("main");
        }
        public int GetCountFast()
        {
            return GetCount("fast");
        }
        public int GetCountReference()
        {
            return GetCount("reference");
        }
        public int GetCountMainHist()
        {
            return GetCount("mainhist");
        }
        public int GetCountFastHist()
        {
            return GetCount("fasthist");
        }
        public int GetCountReferenceHist()
        {
            return GetCount("referencehist");
        }
        public int GetMiddleTime()
        {
            TimeSpan q = new TimeSpan(); int k = 0;
            string[] b1 = GetMainHist().Split(';');
            string[] b2 = GetFastHist().Split(';');
            List<DateTime> b = new List<DateTime>();
            try
            {
                for (int i = 0; i < 100; i = i + 2)
                {
                    b.Add(DateTime.Parse(b1[4]));
                    b.Add(DateTime.Parse(b2[4]));
                }
            }
            catch { }
            b.Sort();
            for (int i = b.Count - 1; i > 1; i--)
            {
                TimeSpan tmp = b[i] - b[i - 1];
                if (tmp > TimeSpan.FromHours(1)||k>49)
                    break;
                q += tmp; k++;
            }
            q = TimeSpan.FromTicks(q.Ticks / 2);
            return q.Minutes;
        }
        int GetCount(string table)
        {
            cmd = Connection.CreateCommand();
            cmd.CommandText = String.Format("SELECT COUNT(1) FROM {0:s};", table);
            Connection.Open();
            SQLiteDataReader r = cmd.ExecuteReader();
            r.Read();
            int s = int.Parse(r.GetValue(0).ToString());
            r.Close();
            Connection.Close();
            return s;
        }

        public void Add(int num ,string purp)
        {
            string values ;
            if (int.Parse(purp.Substring(4)) <= 6)
            {
                values = String.Format("{0:d}, '{1:s}', '{2:s}'", num, purp, DateTime.Now.ToString());
                InsertInDB("main", values);
            }
            else
            {
                values = String.Format("{0:d}, '{1:s}', '{2:s}'", num, purp, DateTime.Now.ToString());
                InsertInDB("fast", values);
            }
            Console.WriteLine(values);
        }
        public void AddRef(int num, string purp, int priority)
        {
            string values;
            values = String.Format("{0:d}, '{1:s}', '{2:s}', {3:d}, {4:d}", num, purp, DateTime.Now.ToString(), priority, 1);
            InsertInDB("reference", values);
            Console.WriteLine(values);
        }

        public string GetConfig()
        {
            string str = "";
            try
            {
                for (int i = 0; i < 10; i++)
                    str += config[i] + ";";
                str = str.Substring(0, str.Length - 1);
            }
            catch { str = " "; }
            return str;
        }
        public string GetClientMain(int p)
        {
            string msg = SelectFromDB("main", p);
            if (msg != " ")
            {
                string[] tmpbuf1=msg.Split(';');
                for(int i = 0 ; i<tmpbuf1.Length; i++)
                {
                    string[] tmpbuf2 = tmpbuf1[i].Split('_');
                    InsertInDB("mainhist", String.Format("{0}, {1}, '{2}', '{3}', '{4}'", tmpbuf2[0], tmpbuf2[1], tmpbuf2[2], tmpbuf2[3], DateTime.Now.ToString()));
                }
                RemoveFromDB("main", p);
            }
            else
                return "error";
            return msg;
        }
        public string GetClientFast(int p)
        {
            string msg = SelectFromDB("fast", p);
            if (msg != " ")
            {
                string[] tmpbuf1 = msg.Split(';');
                for (int i = 0; i < tmpbuf1.Length; i++)
                {
                    string[] tmpbuf2 = tmpbuf1[i].Split('_');
                    InsertInDB("fasthist", String.Format("{0}, {1}, '{2}', '{3}', '{4}'", tmpbuf2[0], tmpbuf2[1], tmpbuf2[2], tmpbuf2[3], DateTime.Now.ToString()));
                }
                RemoveFromDB("fast", p);
            }
            else
                return "error";
            return msg;
        }
        public string GetReferenceOne(int p)
        {
            string msg = SelectFromDB("reference", p);
            if (msg != " ")
            {
                string[] tmpbuf1 = msg.Split(';');
                for (int i = 0; i < tmpbuf1.Length; i++)
                {
                    string[] tmpbuf2 = tmpbuf1[i].Split('_');
                    InsertInDB("referencehist", String.Format("{0}, {1}, '{2}', '{3}', '{4}', {5}, {6}", tmpbuf2[0], tmpbuf2[1], tmpbuf2[2], tmpbuf2[3], DateTime.Now.ToString(), tmpbuf2[4], tmpbuf2[5]));
                }
                RemoveFromDB("reference", p);
            }
            else
                return "error";
            return msg;
        }
    }
}