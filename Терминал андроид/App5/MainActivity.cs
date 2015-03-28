using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace App5
{
    enum requests { prop1, prop2, prop3, prop4, prop5, prop6, EOSes, GetBD, GetHD, GetCl, SetCg, GetCg, enumErr }
    [Activity(Label = "Enter", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity
    {
        TextView watch;
        EditText iptxt;
        EditText porttxt;
        Button bconnect;

        string folderPath;
        string filePath;
        bool q; // переменная, надо ли проверять результат подключения

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Welcome);

            SCT temp = new SCT();

            watch = (TextView)FindViewById(Resource.Id.textClock);
            iptxt = (EditText)FindViewById(Resource.Id.text_ip);
            porttxt = (EditText)FindViewById(Resource.Id.text_port);
            bconnect = (Button)FindViewById(Resource.Id.bConnect);

            folderPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal";
            filePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal/ipconfig";

            bconnect.Click += bconnect_Click;

            checkConfig();
        }
        protected override void OnResume()
        {
            Loop();
            base.OnResume();
        }
        async void Loop()
        {
            while (true)
            {
                await Task.Delay(500);
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                
            }
        }
        async void CheckConnectLoop()
        {
            while (true)
            {
                await Task.Delay(100);
                if (q||(SCT.AR!=null&&SCT.AR.IsCompleted))
                {
                    if (SCT.Socket.Connected)
                    {
                        ShowMessage("Connection", "Соединение установлено", true);
                        StartActivity(typeof(DisplayingQueue));
                        this.Finish();
                        break;
                    }
                    else
                    {
                        ShowMessage("Connection", "Не удалось установить соединение", true);
                        bconnect.Clickable = true;
                        break;
                    }
                }
            }
        }
        async void timeout(int seconds)
        {
            await Task.Delay(seconds * 1000);
            q = true;
        }
        void checkConfig()
        {
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                File.Create(filePath);
                return;
            }
            else
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
        void bconnect_Click(object sender, EventArgs e)
        {
            ShowMessage("Connection", String.Format("Попытка подключения..."), false);
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
            catch
            {
                ShowMessage("IpConfigError", String.Format("Данные введены некорректно"), true);
            }
        }
        void ShowMessage(string title, string message, bool cancelable)
        {
            if (dialog != null)
            {
                dialog.Dispose();
            }
            dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetCancelable(cancelable);

            dialog.Create();
            dialog.Show();
        }
    }
}

