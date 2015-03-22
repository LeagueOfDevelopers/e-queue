using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace WpfApplication3
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, EOSes, GetBD, GetHD, GetCl, SetCg, GetCg, enumErr }
    public partial class Terminal : Window
    {
        Socket Client;
        MainWindow MainWindow;
        byte[] buffer;
        public bool temp1;
        DispatcherTimer timer;
        DispatcherTimer ConfigRefresher;
        public Terminal(Socket sct, MainWindow MWind)
        {
            InitializeComponent();
            Client = sct;
            MainWindow = MWind;
            temp1 = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new DispatcherTimer();
            ConfigRefresher = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            ConfigRefresher.Interval = TimeSpan.FromSeconds(15);
            timer.Tick += timer_Tick1;
            ConfigRefresher.Tick += timer_Tick2;
            timer.Start();
            ConfigRefresher.Start();
        }

        private void timer_Tick1(object sender, EventArgs e)
        {
            DateTime NowTime = DateTime.Now;
            string NowTimeStr = NowTime.ToString("HH:mm:ss");
            textTime.Text = "Время:\n" + NowTimeStr;
        }
        private void timer_Tick2(object sender, EventArgs e)
        {
            try
            {
                string str = requests.GetCg.ToString();
                byte[] buf = Encoding.ASCII.GetBytes(str);
                Client.Send(buf);
                buf = new byte[6];
                Client.Receive(new byte[1], 1, SocketFlags.None);
                Client.Receive(buf, 6, SocketFlags.None);
                int msgsize = int.Parse(Encoding.ASCII.GetString(buf, 0, 6));
                buf = new byte[msgsize];
                Client.Receive(buf, msgsize, SocketFlags.None);
                str = Encoding.Unicode.GetString(buf);
                if (str == " ")
                    return;
                Update(str);
            }
            catch 
            { 
                MessageBox.Show("Потеряно соединение с сервером");
                timer.Stop();
                ConfigRefresher.Stop();
                this.Close();
            }
        }

        void Update(string s)
        {
            if (Client.Connected)
            {
                string[] str = s.Split(';');
                try
                {
                    b1.Text = str[0];
                    b2.Text = str[1];
                    b3.Text = str[2];
                    b4.Text = str[3];
                    b5.Text = str[4];
                    b6.Text = str[5];
                    AdBlock.Text = str[6];
                }
                catch { }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Client.Connected)
            {
                string message = "";
                if (sender.Equals(bb1))
                {
                    message = requests.prop1.ToString();
                }
                else if (sender.Equals(bb2))
                {
                    message = requests.prop2.ToString();
                }
                else if (sender.Equals(bb3))
                {
                    message = requests.prop3.ToString();
                }
                else if (sender.Equals(bb4))
                {
                    message = requests.prop4.ToString();
                }
                else if (sender.Equals(bb5))
                {
                    message = requests.prop5.ToString();
                }
                else if (sender.Equals(bb6))
                {
                    message = requests.prop6.ToString();
                }

                buffer = Encoding.ASCII.GetBytes(message);
                Client.Send(buffer);
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow.StartButtonActivate = true;
            timer.Stop();
            ConfigRefresher.Stop();
            EndingSession();
        }
        public void EndingSession()
        {
            try{
                Client.Send(buffer = Encoding.ASCII.GetBytes("EOSes"));
                Client.Close();
            }
            catch {}
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
}
