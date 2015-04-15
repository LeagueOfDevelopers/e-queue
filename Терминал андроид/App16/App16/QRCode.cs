using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using Android.Graphics;

namespace App16
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class QRCodeDisplaying : Activity
    {
        TextView watch;
        TextView number;
        ImageView qrcode;
        TextView internetAddress;
        Button bexit;
        bool q;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.QRCode);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            number = (TextView)FindViewById(Resource.Id.textView8); number.Text = StaticData.ChangedNumber.ToString();
            qrcode = (ImageView)FindViewById(Resource.Id.imageqrcode); qrcode.SetImageBitmap(StaticData.QRCode);
            bexit = (Button)FindViewById(Resource.Id.bexit3); bexit.Click += bexit_Click;
            internetAddress = (TextView)FindViewById(Resource.Id.textView10); internetAddress.Text = String.Format("Подробную информацию узнайте на сайте http://studok.misis.ru/{0:d} ", StaticData.ChangedNumber);
            
            q = true;
            
            Sleep();

            RefreshLoop();
            CheckConnectionLoop();
        }

        private async void Sleep()
        {
            await Task.Delay(30000);
            this.Finish();
        }

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(200);
            }
        }

        private void bexit_Click(object sender, EventArgs e)
        {
            this.Finish();
        }

        private async void CheckConnectionLoop()
        {
            while (true)
            {
                if (!SCT.SocketConnected() && q)
                {
                    StaticData.StopUpdating();
                    try { ShowMessage("Ошибка", "Потеряно соединение с сервером.", true); }
                    catch { }
                    q = false;
                }
                await Task.Delay(100);
            }
        }

        private void ShowMessage(string title, string message, bool exit)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("OK", delegate
            {
                if (exit)
                {
                    StartActivity(typeof(MainActivity));
                    this.Finish();
                }
            });
            dialog.Show();
        }
    }
}