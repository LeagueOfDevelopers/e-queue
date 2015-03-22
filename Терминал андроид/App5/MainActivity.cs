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
        IPEndPoint ipEndPoint;
        Socket SSender;
        IAsyncResult ar;
        bool q = false; // переменная, надо ли проверять результат подключения

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Welcome);

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
                await Task.Delay(100);
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                if(q)
                {
                    if (SSender.Connected)
                    {
                        ShowMessage("Connection", "Соединение установлено", false);
                        SCT sct = new SCT(SSender);
                        StartActivity(typeof(DisplayingQueue));
                        this.Finish();
                    }
                    else
                    {
                        ShowMessage("Connection", "Не удалось установить соединение", false);
                    }
                    q = false;
                }
            }
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
                    ipEndPoint = new IPEndPoint(ip, port);
                    iptxt.Text = ip.ToString();
                    porttxt.Text = port.ToString();
                    sr.Close();
                }
                catch
                {
                    ShowMessage("IpConfigError", String.Format("Информация в файле конфигурации ({0:s}) некорректна", filePath), false);
                    sr.Close();
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
                ipEndPoint = new IPEndPoint(ip, port);
                StreamWriter sw = new StreamWriter(filePath, false);
                sw.WriteLine(ip.ToString());
                sw.WriteLine(port.ToString());
                sw.Close();
            }
            catch
            {
                ShowMessage("IpConfigError", String.Format("Данные введены некорректно"), false);
            }
            Connecting();
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

