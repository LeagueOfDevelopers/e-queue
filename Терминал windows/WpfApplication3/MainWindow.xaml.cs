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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;
using System.Net;

namespace WpfApplication3
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        int port;
        IPAddress ip;
        IPEndPoint ipEndPoint;
        public Terminal w1;
        public bool StartButtonActivate
        {
            set { bStart.IsEnabled = value; }
            get { return bStart.IsEnabled; }
        }
        public MainWindow()
        {
            InitializeComponent();
        }

        private void bExit_Click(object sender, RoutedEventArgs e)
        {
            if (w1 != null)
                w1.EndingSession();
            Environment.Exit(0);
        }

        private void bStart_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                port = int.Parse(portTextBox.Text);
                ip = IPAddress.Parse(ipTextBox.Text);
                ipEndPoint = new IPEndPoint(ip, port);
            }
            catch { MessageBox.Show("Ошибка ввода данных."); return; }
            Socket Ssender = new Socket(SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Ssender.Connect(ipEndPoint);
            }
            catch { MessageBox.Show("Ошибка подключения"); return; }
            w1 = new Terminal(Ssender, this);
            w1.Show();
            StartButtonActivate = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (w1 != null)
                w1.EndingSession();
            Environment.Exit(0);
        }
    }
}
