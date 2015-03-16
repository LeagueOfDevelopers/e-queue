using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApplication3
{
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, EOSes, GetBD, GetHD, GetCl, SetCg, GetCg, enumErr }
    public partial class MainWindow : Window
    {
        Socket Socket;
        public IPEndPoint EndPoint;
        exitedClient CLIENT;
        public ObservableCollection<client> Base
        {
            get;
            set;
        }
        public ObservableCollection<exitedClient> History
        {
            get;
            set;
        }
        public string[] TranslatedRequests = new string[7];
        string pathIpPort = Environment.CurrentDirectory + @"\config.txt";
        public bool blockAutoRefresh = true;
        bool WaitingForClient = false;
        public MainWindow()
        {
            Base = new ObservableCollection<client>();
            History = new ObservableCollection<exitedClient>();
            LoadIpPort();
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(5000);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        private void timer_Tick(object sender, EventArgs e)
        {
            if (!blockAutoRefresh)
            {
                try
                {
                    LoadBD();
                    LoadHD();
                }
                catch(SocketException)
                { 
                    MessageBox.Show("Соединение с сервером потеряно");
                    blockAutoRefresh = true;
                }
            }

        }
        private void RunServ_Click(object sender, RoutedEventArgs e)
        {
             if (!(Socket != null && Socket.Connected))
             {
                 RunServWindow RSW = new RunServWindow(this);
                 RSW.Show();
             } else MessageBox.Show("Соединение уже установлено");

        }
        private void ConnectServ_Click(object sender, RoutedEventArgs e)
        {
            StartConnection(false);
        }
        private void DisconnectServ_Click(object sender, RoutedEventArgs e)
        {
            if (!blockAutoRefresh)
                try
                {
                    string req = requests.EOSes.ToString();
                    byte[] buf = Encoding.ASCII.GetBytes(req);
                    Socket.Send(buf);
                    Socket.Disconnect(false);
                    blockAutoRefresh = true;
                    MessageBox.Show("Соединение завершено");
                }
                catch { }
            else MessageBox.Show("Соединение не установлено");
        }
        private void ConnectionConfig_Click(object sender, RoutedEventArgs e)
        {
            SettingWindow SetWin;
            if (Socket != null && Socket.Connected)
                SetWin = new SettingWindow(true, this);
            else SetWin = new SettingWindow(false, this);
            SetWin.Show();
        }
        private void TerminalConfig_Click(object sender, RoutedEventArgs e)
        {
            if (Socket != null && Socket.Connected)
            {
                ConfigWindow cw = new ConfigWindow(this);
                cw.Show();
            }
            else MessageBox.Show("Соединение не установлено");
        }
        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (Socket != null && Socket.Connected)
            {
                blockAutoRefresh = true;
                LoadCl(0);
                blockAutoRefresh = false;
            }
            else MessageBox.Show("Соединение не установлено");
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            Environment.Exit(0);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                string req = requests.EOSes.ToString();
                byte[] buf = Encoding.ASCII.GetBytes(req);
                Socket.Send(buf);
            }
            catch { }
        }
        public void StartConnection(bool CreatingServ)
        {
            if (!(Socket != null && Socket.Connected))
            {
                if (CreatingServ)
                    Thread.Sleep(2000);
                try
                {
                    Socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    //Socket.Bind(new IPEndPoint(IPAddress.Parse("192.168.58.104"), 22556));   //подключение к конкретному локальному EndPoint
                    Socket.Connect(EndPoint);
                    if (CreatingServ)
                        MessageBox.Show("Сервер запущен");
                    else MessageBox.Show("Соединение установлено");
                }
                catch { MessageBox.Show("Ошибка подключения, проверьте настройки"); return; }
            }
            else MessageBox.Show("Соединение уже установлено");
            try
            {
                LoadCg();
                LoadBD();
                LoadHD();
                LoadCl(0);
            }
            catch { }
            blockAutoRefresh = false;
        }
        void LoadIpPort()
        {
            IPAddress ip = new IPAddress(1);
            int port = 1;
            StreamReader sr;
            bool q = false;
            try
            {
                sr = new StreamReader(pathIpPort);
                sr.Close();
            }
            catch
            {
                StreamWriter sw = new StreamWriter(pathIpPort, false);
                sw.Close();
                return;
            }
            try
            {
                sr = new StreamReader(pathIpPort);
                ip = IPAddress.Parse(sr.ReadLine());
                port = int.Parse(sr.ReadLine());
                sr.Close();
                q = true;
            }
            catch
            {
                sr.Close();
                StreamWriter sw = new StreamWriter(pathIpPort, false);
                sw.Close();
            }
            if (q)
                EndPoint = new IPEndPoint(ip, port);
        }
        public void SaveIpPort()
        {
            StreamWriter sw = new StreamWriter(pathIpPort);
            try
            {
                sw.WriteLine(EndPoint.Address.ToString());
                sw.WriteLine(EndPoint.Port.ToString());
                sw.Close();
            }
            catch
            {
                sw.Close();
                File.CreateText(pathIpPort);
            }
        }
        void LoadBD()
        {
            Base.Clear();
            string str = requests.GetBD.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6];
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            str = Encoding.ASCII.GetString(buf, 0, 6);
            int msgsize = int.Parse(str);
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.ASCII.GetString(buf, 0, buf.Length);
            string[] ClientBuf = str.Split(';');
            for (int i = 0; i < ClientBuf.Length; i++)
            {
                try
                {
                    string[] buf2 = ClientBuf[i].Split('_');
                    int n = int.Parse(buf2[0]);
                    string p = TranslatingReq(buf2[1]);
                    DateTime d = DateTime.Parse(buf2[2]);
                    Base.Add(new client(n, p, d));
                }
                catch { }
            }
            Counter1.Text = Base.Count.ToString();
            if (WaitingForClient && Base.Count != 0)
            {
                MessageBox.Show("Появился клиент в очереди");
                WaitingForClient = false;
            }
        }
        void LoadHD()
        {
            History.Clear();
            string str = requests.GetHD.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6]; 
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            int msgsize = int.Parse(Encoding.ASCII.GetString(buf, 0, 6));
            int offset = 0;
            buf = new byte[msgsize];
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.ASCII.GetString(buf, 0, buf.Length);
            string[] ClientBuf = str.Split(';');
            for (int i = 0; i < ClientBuf.Length; i++)
            {
                try
                {
                    string[] buf2 = ClientBuf[i].Split('_');
                    int n = int.Parse(buf2[0]);
                    string p = TranslatingReq(buf2[1]); ;
                    DateTime d = DateTime.Parse(buf2[2]);
                    DateTime d2 = DateTime.Parse(buf2[3]);
                    History.Add(new exitedClient(n, p, d, d2));
                }
                catch { }
            }
            Counter2.Text = History.Count.ToString();
        }
        void LoadCl(int i)
        {
            string str = requests.GetCl.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            str = i.ToString("D5");
            buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6];
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            int msgsize = int.Parse(Encoding.ASCII.GetString(buf, 0, 6));
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.ASCII.GetString(buf);
            if (str == "error")
            {
                MessageBox.Show("Список пуст");
                WaitingForClient = true;
                Base.Clear();
                return;
            }
            string[] buf2 = str.Split('_');
            int n = int.Parse(buf2[0]);
            string p = TranslatingReq(buf2[1]);
            DateTime d = DateTime.Parse(buf2[2]);
            DateTime d2 = DateTime.Parse(buf2[3]);
            CLIENT = new exitedClient(n, p, d, d2);
            tNumber.Content = CLIENT.Number.ToString("D3");
            tPurpose.Content = CLIENT.Purpose;
            tTimeOfEnter.Content = CLIENT.TimeOfEnter.ToString();
            tTimeOfOut.Content = CLIENT.TimeOfOut.ToString();
        }
        void LoadCg()
        {
            string str = requests.GetCg.ToString();
            byte[] buf = Encoding.ASCII.GetBytes(str);
            Socket.Send(buf);
            buf = new byte[6];
            Socket.Receive(new byte[1], 1, SocketFlags.None);
            Socket.Receive(buf, 6, SocketFlags.None);
            int msgsize = int.Parse(Encoding.ASCII.GetString(buf, 0, 6));
            buf = new byte[msgsize];
            int offset = 0;
            bool q;
            do
            {
                int geted = Socket.Receive(buf, offset, msgsize, SocketFlags.None);
                if (geted != msgsize)
                {
                    offset = geted;
                    msgsize -= geted;
                    q = true;
                }
                else q = false;
            } while (q);
            str = Encoding.Unicode.GetString(buf);
            TranslatedRequests = str.Split(';');
        }
        public void SendCg()
        {
            string msg = "";
            for (int i = 0; i < 7; i++)
                msg += TranslatedRequests[i] + ";";
            byte[] Buffer1 = Encoding.ASCII.GetBytes(requests.SetCg.ToString());
            byte[] Buffer3 = Encoding.Unicode.GetBytes(msg);
            byte[] Buffer2 = Encoding.ASCII.GetBytes(Buffer3.Length.ToString("D6"));
            Socket.Send(Buffer1);
            Socket.Send(new byte[] { 1 }, 1, SocketFlags.None);
            Socket.Send(Buffer2);
            Socket.Send(Buffer3);
        }
        string TranslatingReq(string p)
        {
            switch (ParseReq(p))
            {
                case requests.prop1:
                    return TranslatedRequests[0];
                case requests.prop2:
                    return TranslatedRequests[1];
                case requests.prop3:
                    return TranslatedRequests[2];
                case requests.prop4:
                    return TranslatedRequests[3];
                case requests.prop5:
                    return TranslatedRequests[4];
                case requests.prop6:
                    return TranslatedRequests[5];
                default:
                    return "";
            }
        }
        requests ParseReq(string value)// Parse из string в request
        {
            try
            {
                return (requests)Enum.Parse(typeof(requests), value);
            }
            catch { return requests.enumErr; }
        }
    }
    public class client
    {
        int num;
        string purpose;
        DateTime timeOfEnter;
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
    public class exitedClient
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
