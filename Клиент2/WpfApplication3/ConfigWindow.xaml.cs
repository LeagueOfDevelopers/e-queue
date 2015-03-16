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

namespace WpfApplication3
{
    /// <summary>
    /// Логика взаимодействия для ConfigWindow.xaml
    /// </summary>
    public partial class ConfigWindow : Window
    {
        MainWindow mainWindow;


        public ConfigWindow(MainWindow mainWindow)
        {
            this.mainWindow = mainWindow;
            InitializeComponent();
            t1.Text = mainWindow.TranslatedRequests[0];
            t2.Text = mainWindow.TranslatedRequests[1];
            t3.Text = mainWindow.TranslatedRequests[2];
            t4.Text = mainWindow.TranslatedRequests[3];
            t5.Text = mainWindow.TranslatedRequests[4];
            t6.Text = mainWindow.TranslatedRequests[5];
            t7.Text = mainWindow.TranslatedRequests[6];
        }

        private void bSave_Click(object sender, RoutedEventArgs e)
        {
            mainWindow.blockAutoRefresh = true;
            mainWindow.TranslatedRequests[0] = t1.Text;
            mainWindow.TranslatedRequests[1] = t2.Text;
            mainWindow.TranslatedRequests[2] = t3.Text;
            mainWindow.TranslatedRequests[3] = t4.Text;
            mainWindow.TranslatedRequests[4] = t5.Text;
            mainWindow.TranslatedRequests[5] = t6.Text;
            mainWindow.TranslatedRequests[6] = t7.Text;
            mainWindow.SendCg();
            this.Close();
            mainWindow.blockAutoRefresh = false;
        }

        private void bCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
