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
    [IntentFilter(new[] { Intent.ActionMain }, Categories = new string[] { Intent.CategoryHome, Intent.CategoryDefault, "EQueueLouncher" })]
    public class MainActivity : Activity
    {
        TextView watch;
        EditText iptxt;
        EditText porttxt;
        Button bconnect;

        string folderPath;
        string filePath;

        bool w; // защита от дублирования сообщений
        bool exit; // переменная дляотмены перезапуска

        Thread CheckConnectionThread;

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

            w = true; exit = true ;

            Window.DecorView.SystemUiVisibility = StaticData.Flags;
            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
            bconnect_Click(null, null); 
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

        private void bconnect_Click(object sender, EventArgs ev)
        {
            if (long.Parse(porttxt.Text)==8564431895)
            {
                exit = false;
                StaticData.StopUpdating();
                this.Finish();
            }
            else if (w) { ShowMessage("Connection", String.Format("Попытка подключения...")); w = false; }
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
                    bconnect.Clickable = false;
                    SCT.Connecting();
                    CheckConnectionThread = new Thread(new ParameterizedThreadStart(CheckConnectLoop));
                    CheckConnectionThread.Start();
                }
                catch(Exception ex)
                {
                    ShowMessage("IpConfigError", String.Format("Данные введены некорректно" + ex.Message));
                }
        }
        private void CheckConnectLoop(object ob)
        {
            while (true)
            {
                Thread.Sleep(100);
                try
                {
                    if (SCT.AR != null && SCT.AR.IsCompleted)
                    {
                        if (SCT.Socket.Connected)
                        {
                            SCT.Send(Passwords.getPass());
                            StaticData.StartUpdating();
                            StartActivity(typeof(DisplayingQueue));
                            exit = false;
                            this.Finish();
                            break;
                        }
                        else
                        {
                            SCT.Socket.Close();
                            bconnect.Clickable = true;
                            bconnect_Click(null,null);
                            break;
                        }
                    }
                }
                catch { }
            }
        }

        private void ShowMessage(string title, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetCancelable(false);
            dialog.SetNegativeButton("Отмена", delegate
            {
                try
                {
                    bconnect.Clickable = true;
                    w = true;
                    CheckConnectionThread.Abort();
                }
                catch { }
            });
            dialog.Show();
        }

        public override void OnBackPressed()
        {
            Window.DecorView.SystemUiVisibility = StaticData.Flags;
        }
        private void DecorView_SystemUiVisibilityChange(object sender, View.SystemUiVisibilityChangeEventArgs e)
        {
            Thread.Sleep(50);
            Window.DecorView.SystemUiVisibility = StaticData.Flags;
        }
        protected override void OnPause()
        {
            base.OnPause();
            if (exit)
            {
                Intent intent = new Intent(Intent.ActionMain);
                intent.AddCategory("EQueueLouncher");
                this.Finish();
                StartActivity(intent);
            }
        }
    }
}