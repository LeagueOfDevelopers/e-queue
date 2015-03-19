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

namespace App5
{
    [Activity(Label = "App5", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity
    {
        TextView watch;

        IPAddress ip;
        int port;

        AlertDialog.Builder dialog;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.WatchQueue);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            checkConfig();
        }
        protected override void OnResume()
        {
            timerLoop();
            base.OnResume();
        }
        async void timerLoop()
        {
            while (true)
            {
                await Task.Delay(1000);
                DateTime NowTime = DateTime.Now;
                string NowTimeStr = NowTime.ToString("HH:mm:ss");
                watch.Text = "Время:\n" + NowTimeStr;
            }
        }
        void checkConfig()
        {
            var folderPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) +"/Terminal";
            var filePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DataDirectory.ToString()) + "/Terminal/ipconfig";
            if (!File.Exists(filePath))
            {
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                File.Create(filePath);
                dialog = new AlertDialog.Builder(this);
                dialog.SetTitle("IpConfig");
                dialog.SetMessage(String.Format("Создан файл конфигурации ({0}). Пожалуйста заполните его и перезапустите приложениею", filePath));
                dialog.SetPositiveButton("OK", delegate { this.Finish(); });
                dialog.Show();
            }
            else
            {
                StreamReader sr = new StreamReader(filePath);
                try
                {
                    ip = IPAddress.Parse(sr.ReadLine());
                    port = int.Parse(sr.ReadLine());
                }
                catch
                {
                    dialog = new AlertDialog.Builder(this);
                    dialog.SetTitle("IpConfigError");
                    dialog.SetMessage(String.Format("Информация в файле конфигурации ({0:s}) некорректна", filePath));
                    dialog.Show();
                }
                sr.Close();
            }
        }
    }
}

