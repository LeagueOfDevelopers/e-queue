using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
    /// Логика взаимодействия для SettingWindow.xaml
    /// </summary>
    public partial class SettingWindow : Window
    {
        MainWindow MainWindow;
        int port;
        IPAddress ip;
        public SettingWindow(bool connected, MainWindow mw)
        {
            MainWindow = mw;
            InitializeComponent();
            try
            {
                ipTextBox.Text = MainWindow.EndPoint.Address.ToString();
                portTextBox.Text = MainWindow.EndPoint.Port.ToString();
            }
            catch { }
            bSave.IsEnabled = !connected;
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                port = int.Parse(portTextBox.Text);
                ip = IPAddress.Parse(ipTextBox.Text);
                if (port < 0 || port > 65535)
                    Int32.Parse(" ");
            }
            catch { MessageBox.Show("Некорректно введены данные"); return; }
            MainWindow.EndPoint = new System.Net.IPEndPoint(ip, port);
            MainWindow.SaveIpPort();
            this.Close();
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
