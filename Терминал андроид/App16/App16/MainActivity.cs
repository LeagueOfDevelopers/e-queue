using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;

namespace App16
{
    [Activity(Label = "EQueue", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity
    {
        TextView watch;
        EditText iptxt;
        EditText porttxt;
        Button bconnect;

        string folderPath;
        string filePath;

        bool q; // переменная, надо ли проверять результат подключения


        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Welcome);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            iptxt = (EditText)FindViewById(Resource.Id.text_ip);
            porttxt = (EditText)FindViewById(Resource.Id.text_port);
            bconnect = (Button)FindViewById(Resource.Id.bConnect); bconnect.Click += bconnect_Click;

            SCT tmp = new SCT();
            StaticData tmp2 = new StaticData(10);

            folderPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal";
            filePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal/ipconfig";

            checkConfig();

            RefreshLoop();
        }
        private void checkConfig()
        {
            if (File.Exists(filePath))
            {
                StreamReader sr = new StreamReader(filePath);
                try
                {
                    IPAddress ip = IPAddress.Parse(sr.ReadLine());
                    int port = int.Parse(sr.ReadLine());
                    IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                    iptxt.Text = ip.ToString();
                    porttxt.Text = port.ToString();
                    sr.Close();
                }
                catch
                {
                    sr.Close();
                    StreamWriter sw = new StreamWriter(filePath, false);
                    sw.Close();
                }
            }
        }
        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(200);
            }
        }

        private void bconnect_Click(object sender, EventArgs e)
        {
            ShowMessage("Connection", String.Format("Попытка подключения..."));
            try
            {
                IPAddress ip = IPAddress.Parse(iptxt.Text);
                int port = int.Parse(porttxt.Text);
                IPEndPoint ipEndPoint = new IPEndPoint(ip, port);
                StreamWriter sw = new StreamWriter(filePath, false);
                SCT.IPEndPoint = ipEndPoint;
                sw.WriteLine(ip.ToString());
                sw.WriteLine(port.ToString());
                sw.Close();
                q = false;
                bconnect.Clickable = false;
                SCT.Connecting();
                timeout(15);
                CheckConnectLoop();
            }
            catch(Exception ex)
            {
                ShowMessage("IpConfigError", String.Format("Данные введены некорректно" + ex.Message));
            }
        }
        private async void timeout(int seconds)
        {
            await Task.Delay(seconds * 1000);
            q = true;
        }
        private async void CheckConnectLoop()
        {
            while (true)
            {
                await Task.Delay(100);
                if (q || (SCT.AR != null && SCT.AR.IsCompleted))
                {
                    if (SCT.Socket.Connected)
                    {
                        ShowMessage("Connection", "Соединение установлено");
                        SCT.Send(Passwords.getPass());
                        StartActivity(typeof(DisplayingQueue));
                        this.Finish();
                        break;
                    }
                    else
                    {
                        ShowMessage("Connection", "Не удалось установить соединение");
                        SCT.Socket.Close();
                        bconnect.Clickable = true;
                        break;
                    }
                }
            }
        }

        private void ShowMessage(string title, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.Create();
            dialog.Show();
        }
    }
}

