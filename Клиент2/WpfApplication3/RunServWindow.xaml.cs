using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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

namespace WpfApplication3
{
    /// <summary>
    /// Логика взаимодействия для RunServWindow.xaml
    /// </summary>
    public partial class RunServWindow : Window
    {
        MainWindow MainWindow;
        List<IPAddress> ip;
        int port;
        public RunServWindow(MainWindow mw)
        {
            MainWindow = mw;
            InitializeComponent();
        }

        void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string hostName = Dns.GetHostName();
            ip = new List<IPAddress>(Dns.GetHostEntry(hostName).AddressList);
            for (int i = 0; i < ip.Count; i++)
                if (IpIsOK(ip[i]))
                    IPBox.Items.Add(ip[i]);
                else
                {
                    ip.Remove(ip[i]);
                    i--;
                }
        }
        bool IpIsOK(IPAddress adr)
        {
            if (!(!adr.IsIPv6LinkLocal && !adr.IsIPv6Teredo && !adr.IsIPv6Multicast && !adr.IsIPv6SiteLocal && !adr.IsIPv4MappedToIPv6))
                return false;
            try
            {
                string[] buf = adr.ToString().Split('.');
                for (int i = 0; i < buf.Length; i++)
                    int.Parse(buf[i]);
            }
            catch { return false; }
            return true;
        }
        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                port = int.Parse(portBox.Text);
                if (!(port >= 0 && port < Math.Pow(2, 16)))
                    int.Parse("q");
            }
            catch { MessageBox.Show("Ошибка ввода"); return; }
            try
            {
                TcpListener tl = new TcpListener(port);
                tl.Start();
                tl.Stop();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Ошибка: " + exc.Message); return;
            }
            Process proc = new Process();
            proc.StartInfo.FileName = "ConsoleApplication42.exe";
            proc.StartInfo.Arguments = ip[IPBox.SelectedIndex].ToString() + " " + port;
            proc.Start();
            MainWindow.EndPoint = new IPEndPoint(ip[IPBox.SelectedIndex], port);
            MainWindow.StartConnection(true);
            this.Close();
        }
    }
}
