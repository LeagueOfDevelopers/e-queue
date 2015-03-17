using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;
namespace Server
{
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, EOSes, GetBD, GetHD, GetCl, SetCg, GetCg, enumErr}
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
            int SizeOfBase = 999;//Размер базы(нумерация доходит до него - дальше 1)
            int SizeOfHistory = 10000;
            Server.DataBase = new DataBase(SizeOfBase, SizeOfHistory);//инициализируем базу
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
            switch(k.Key)
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
        int CountOfBytes;//длина полученного сообщения
        byte[] Buffer;
        public AcceptedClient(Socket Cl)//работа с клиентом
        {
            Client = Cl;
            EndPoint = Client.RemoteEndPoint;
            string Request;
            CountOfBytes = 5;
            Buffer = new byte[1024];
            bool q = true;
            while (q)
            {
                Client.Receive(Buffer, CountOfBytes,SocketFlags.None);
                Request = Encoding.ASCII.GetString(Buffer, 0, CountOfBytes);
                q = SwitchReq(Request);
            }
            Client.Close();
            Console.WriteLine("Соединение с {0} завершено", EndPoint);
        }
        bool SwitchReq(string Request)
        {
            string msg; byte[] tmp;
            if (Request.Length == 0)
                return true;
            switch(Server.ParseReq(Request))
            {
                case requests.prop1:
                    Server.DataBase.Add(Request);
                    break;
                case requests.prop2:
                    Server.DataBase.Add(Request);
                    break;
                case requests.prop3:
                    Server.DataBase.Add(Request);
                    break;
                case requests.prop4:
                    Server.DataBase.Add(Request);
                    break;
                case requests.prop5:
                    Server.DataBase.Add(Request);
                    break;
                case requests.prop6:
                    Server.DataBase.Add(Request);
                    break;
                case requests.GetCl:
                    Client.Receive(Buffer, CountOfBytes, SocketFlags.None);
                    msg = Encoding.ASCII.GetString(Buffer, 0, CountOfBytes);
                    try
                    {
                        msg = Server.DataBase.GetClient(int.Parse(msg)).ToString();
                    }
                    catch { msg = "error"; }
                    tmp = Encoding.ASCII.GetBytes(msg);
                    msg = tmp.Length.ToString("D6");
                    byte[] Buffer2 = Encoding.ASCII.GetBytes(msg);
                    Client.Send(new byte[] { 1 }, 1, SocketFlags.None);
                    Client.Send(Buffer2, 6, SocketFlags.None);
                    Client.Send(tmp, tmp.Length, SocketFlags.None);
                    break;
                case requests.GetBD:
                    msg = Server.DataBase.ToStringBase();
                    tmp = Encoding.ASCII.GetBytes(msg);
                    msg = tmp.Length.ToString("D6");
                    Buffer2 = Encoding.ASCII.GetBytes(msg);
                    Client.Send(new byte[] { 1 }, 1, SocketFlags.None);
                    Client.Send(Buffer2, 6, SocketFlags.None);
                    Client.Send(tmp, tmp.Length, SocketFlags.None);
                    break;
                case requests.GetHD:
                    msg = Server.DataBase.ToStringHistory();
                    tmp = Encoding.ASCII.GetBytes(msg);
                    msg = tmp.Length.ToString("D6");
                    Buffer2 = Encoding.ASCII.GetBytes(msg);
                    Client.Send(new byte[] { 1 }, 1, SocketFlags.None);
                    Client.Send(Buffer2, 6, SocketFlags.None);
                    Client.Send(tmp, tmp.Length, SocketFlags.None);
                    break;
                case requests.SetCg:
                    byte[] buf = new byte[6];
                    Client.Receive(new byte[1], 1, SocketFlags.None);
                    Client.Receive(buf, 6, SocketFlags.None);
                    string str = Encoding.ASCII.GetString(buf);
                    int msgsize = int.Parse(str);
                    Buffer = new byte[msgsize];
                    int offset = 0;
                    bool q;
                    do
                    {
                        int geted = Client.Receive(Buffer, offset, msgsize, SocketFlags.None);
                        if (geted != msgsize)
                        {
                            offset = geted;
                            msgsize -= geted;
                            q = true;
                        }
                        else q = false;
                    } while (q);
                    msg = Encoding.Unicode.GetString(Buffer);
                    Server.DataBase.SetConfig(msg);
                    break;
                case requests.GetCg:
                    str = Server.DataBase.GetConfig();
                    tmp = Encoding.Unicode.GetBytes(str);
                    str = tmp.Length.ToString("D6");
                    Buffer2 = Encoding.ASCII.GetBytes(str);
                    Client.Send(new byte[] { 1 }, 1, SocketFlags.None);
                    Client.Send(Buffer2, 6, SocketFlags.None);
                    Client.Send(tmp, tmp.Length, SocketFlags.None);
                    break;
                case requests.EOSes:
                    return false;
            }
            return true;
        }
    }
    class DataBase
    {
        List<client> Base;
        List<exitedClient> History;
        string[] config;
        string pathBase;
        string pathHistory;
        string pathConfig;
        int SizeOfBase;
        int SizeOfHistory;
        int LastNumber;
        public DataBase(int s1, int s2)
        {
            Base = new List<client>();
            History = new List<exitedClient>();
            config = new string[7];
            SizeOfBase = s1;
            SizeOfHistory = s2;
            pathBase = Environment.CurrentDirectory + "\\save\\database.txt";
            pathHistory = Environment.CurrentDirectory + "\\save\\histbase.txt";
            pathConfig = Environment.CurrentDirectory + "\\save\\config.txt";
            LastNumber = 0;
            LoadBase();
            LoadHistory();
            LoadConfig();
        }
        void LoadBase()
        {
            try
            {
                StreamReader sr = new StreamReader(pathBase);
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    string[] buf = str.Split('_');
                    int n = int.Parse(buf[0]);
                    string p = buf[1];
                    DateTime d = DateTime.Parse(buf[2]);
                    if (DataError(n, p, d))//проверка загружаемых данных на корректность
                    {
                        sr.Close();
                        int.Parse("");
                    }
                    Base.Add(new client(n, p, d));
                    LastNumber = n;
                }
                sr.Close();
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
            }
            catch (FileNotFoundException)
            {
                File.CreateText(pathBase);
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Environment.CurrentDirectory + "\\save");
                File.CreateText(pathBase);
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
            }
            catch
            {
                Base = new List<client>();
                try
                {
                    StreamWriter swrt = new StreamWriter(pathBase);
                    swrt.Close();
                }
                catch
                {
                    Console.WriteLine("Сервер не может быть запущен. Ограничен доступ к файлу базы(\\save\\database).");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
                Console.WriteLine("Файл загрузки поврежден, создан новый.");
            }
        }
        void LoadHistory()
        {
            try
            {
                StreamReader sr = new StreamReader(pathHistory);
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    string[] buf = str.Split('_');
                    int n = int.Parse(buf[0]);
                    string p = buf[1];
                    DateTime d = DateTime.Parse(buf[2]);
                    DateTime d2 = DateTime.Parse(buf[3]);
                    History.Add(new exitedClient(n, p, d, d2));

                }
                sr.Close();
                File.SetAttributes(pathHistory, FileAttributes.ReadOnly);
            }
            catch (FileNotFoundException)
            {
                File.CreateText(pathHistory);
                File.SetAttributes(pathHistory, FileAttributes.ReadOnly);
            }
            catch
            {
                History = new List<exitedClient>();
                try
                {
                    StreamWriter swrt = new StreamWriter(pathHistory);
                    swrt.Close();
                }
                catch
                {
                    Console.WriteLine("Сервер не может быть запущен. Ограничен доступ к файлу базы(\\save\\histbase.txt). \nПожалуйста, убедитесь что файл базы имеет корректные атрибуты (разрешена запись)и перезапустите сервер.\n\nНажмите любую клавишу, для завершения.");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                File.SetAttributes(pathHistory, FileAttributes.ReadOnly);
                Console.WriteLine("Файл загрузки поврежден, создан новый.");
            }
        }
        void LoadConfig()
        {
            try
            {
                StreamReader sr = new StreamReader(pathConfig, Encoding.GetEncoding(1251));
                for (int i = 0; i < 7; i++)
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
                config = new string[7];
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
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
                Console.WriteLine("Файл загрузки поврежден, создан новый.");
            }
        }
        bool DataError(int n, string p, DateTime d)
        {
            if ((Base.Count != 0 && n != Base.Last().Number + 1 && Base.Last().Number != 999))
                return true;
            if (Base.Count != 0 && d < Base.Last().TimeOfEnter)
                return true;

            switch (Server.ParseReq(p))
            {
                case requests.prop1:
                    break;
                case requests.prop2:
                    break;
                case requests.prop3:
                    break;
                case requests.prop4:
                    break;
                case requests.prop5:
                    break;
                case requests.prop6:
                    break;
                default:
                    return true;
            }
            return false;
        }
        public void Add(string a)
        {
            int num;
            if (LastNumber == 999)
                num = 1;
            else
                num = LastNumber + 1;
            LastNumber = num;
            if (Base.Count == SizeOfBase)
                Base.Remove(Base[0]);
            Base.Add(new client(num, a));
            SaveBase();
        }
        public exitedClient GetClient(int i)
        {
            exitedClient tmp = new exitedClient(Base[i].Number, Base[i].Purpose, Base[i].TimeOfEnter, DateTime.Now);
            Base.Remove(Base[i]);
            History.Add(tmp);
            File.SetAttributes(pathBase, FileAttributes.Normal);
            try
            {
                StreamWriter swr = new StreamWriter(pathBase, false);
                for (int k = 0; k < Base.Count; k++)
                    swr.WriteLine(Base[k].ToString());
                swr.Close();
            }
            catch
            {
                Console.WriteLine("Неизвестная ошибка.");
                Console.ReadKey();
                Environment.Exit(0);
            } 
            File.SetAttributes(pathHistory, FileAttributes.Normal);
            try
            {
                if (History.Count == SizeOfHistory)//удаление первой строки из файла, при превышении размера
                {
                    StreamReader sre = new StreamReader(pathHistory);
                    sre.ReadLine();
                    string savefile = sre.ReadToEnd();
                    sre.Close();
                    StreamWriter swr = new StreamWriter(pathHistory, false);
                    swr.Write(savefile);
                    swr.Close();
                }
                StreamWriter sw = new StreamWriter(pathHistory, true);
                sw.WriteLine(History.Last().ToString());
                sw.Close();
                File.SetAttributes(pathHistory, FileAttributes.ReadOnly);
            }
            catch { }
            return tmp;
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
        public string GetConfig()
        {
            string str = "";
            try
            {
                for (int i = 0; i < 7; i++)
                    str += config[i] + ";";
                str = str.Substring(0, str.Length - 1);
            }
            catch { str = " "; }
            return str;
        }
        void SaveBase()
        {
            File.SetAttributes(pathBase, FileAttributes.Normal);
            try
            {
                if (Base.Count == SizeOfBase)//удаление первой строки из файла, при превышении размера
                {
                    StreamReader sre = new StreamReader(pathBase);
                    sre.ReadLine();
                    string savefile = sre.ReadToEnd();
                    sre.Close();
                    StreamWriter swr = new StreamWriter(pathBase, false);
                    swr.Write(savefile);
                    swr.Close();
                }
                StreamWriter sw = new StreamWriter(pathBase, true);
                sw.WriteLine(Base.Last().ToString());
                Console.WriteLine(Base.Last().ToString());
                sw.Close();
                File.SetAttributes(pathBase, FileAttributes.ReadOnly);
            }
            catch { }
        }
        public string ToStringBase()
        {
            string str = "";
            for (int i = 0; i < Base.Count; i++)
                str += Base[i].ToString() + ";";
            if (str == "")
                str = " ";
            return str;
        }
        public string ToStringHistory()
        {
            string str = "";
            for (int i = 0; i < History.Count; i++)
                str += History[i].ToString() + ";";
            if (str == "")
                str = " ";
            return str;
        }
    }
    struct client
    {
        int num;
        string purpose;
        DateTime timeOfEnter;
        public client(int n, string p)
        {
            num = n;
            purpose = p;
            timeOfEnter = DateTime.Now;
        }
        public client(int n, string p, DateTime E)
        {
            num = n;
            purpose = p;
            timeOfEnter = E;
        }
        public int Number
        {
            get { return num; }
        }
        public string Purpose
        {
            get { return purpose; }
        }
        public DateTime TimeOfEnter
        {
            get { return timeOfEnter; }
        }
        public string ToString()
        {
            return String.Format("{0,5:d}_{1,10:s}_{2}", Number, Purpose, TimeOfEnter);
        }

    }
    struct exitedClient
    {
        int num;
        string purpose;
        DateTime timeOfEnter;
        DateTime timeOfOut;
        public exitedClient(int n, string p, DateTime E, DateTime O)
        {
            num = n;
            purpose = p;
            timeOfEnter = E;
            timeOfOut = O;
        }
        public int Number
        {
            get { return num; }
        }
        public string Purpose
        {
            get { return purpose; }
        }
        public DateTime TimeOfEnter
        {
            get { return timeOfEnter; }
        }
        public DateTime TimeOfOut
        {
            get { return timeOfOut; }
        }
        public string ToString()
        {
            return String.Format("{0,5:d}_{1,10:s}_{2}_{3}", Number, Purpose, TimeOfEnter, TimeOfOut);
        }
    }
}
