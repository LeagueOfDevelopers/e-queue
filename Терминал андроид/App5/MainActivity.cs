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
    [Activity(Label = "App5", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity
    {
        TextView watch;

        IPEndPoint ipEndPoint;
        Socket SSender;
        IAsyncResult ar;
        bool q = false; // переменная, надо ли проверять результат подключения

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.WatchQueue);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            if(checkConfig())
            {
                Connecting();
            }
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
                await Task.Delay(100);
                DateTime NowTime = DateTime.Now;
                string NowTimeStr = NowTime.ToString("HH:mm:ss");
                watch.Text = "Время:\n" + NowTimeStr;
                if(q)
                {
                    if (SSender.Connected)
                        ShowMessage("Connection", "Соединение установлено", false);
                    else
                    {
                        ShowMessage("Connection", "Не удалось установить соединение", true);
                    }
                    q = false;
                }
            }
        }
        bool checkConfig()
        {
            var folderPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) +"/Terminal";
            var filePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal/ipconfig";
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                File.Create(filePath);
                ShowMessage("IpConfig", String.Format("Создан файл конфигурации ({0}). Пожалуйста заполните его и перезапустите приложение", filePath), true);
                return false;
            }
            else
            {
                StreamReader sr = new StreamReader(filePath);
                try
                {
                    string str = sr.ReadLine();
                    IPAddress ip = IPAddress.Parse(str);
                    int port = int.Parse(sr.ReadLine());
                    ipEndPoint = new IPEndPoint(ip, port);
                    sr.Close();
                    return true;
                }
                catch
                {
                    ShowMessage("IpConfigError", String.Format("Информация в файле конфигурации ({0:s}) некорректна", filePath), false);
                    sr.Close();
                    return false;
                }
            }
        }
        void Connecting()
        {
            try
            {
                SSender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                SSender.BeginConnect(ipEndPoint, new AsyncCallback(callback), SSender);
                new Thread(new ParameterizedThreadStart(timeout)).Start();
            }
            catch (SocketException exc)
            {
                ShowMessage("ConnectionError", String.Format("Ошибка подключения: {0}", exc.Message), false);
            }

        }
        void callback(IAsyncResult r)
        {
            ar = r;
            q = true;
        }
        void timeout(object ob)
        {
            Thread.Sleep(TimeSpan.FromSeconds(15));
            if (ar==null||!ar.IsCompleted)
                q = true;
        }
        void ShowMessage(string title, string message, bool exit)
        {
            dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("OK", delegate
            {
                if (exit)
                    this.Finish();
            });
            dialog.Show();
        }
    }
}

