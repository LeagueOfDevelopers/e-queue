using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using Android.Graphics;

namespace App5
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class QRCodeDisplayingReference : Activity
    {
        TextView watch;
        TextView number;
        ImageView qrcode;
        Button bexit;
        bool q;
        AlertDialog.Builder dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.QRCodeRef);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            number = (TextView)FindViewById(Resource.Id.textView8);
            number.Text = Data.number;
            qrcode = (ImageView)FindViewById(Resource.Id.imageqrcode);
            bexit = (Button)FindViewById(Resource.Id.bexit3);bexit.Click+=bexit_Click;
            q = true;
        }

        private void bexit_Click(object sender, EventArgs e)
        {
            this.Finish();
        }
        protected override void OnResume()
        {
            base.OnResume();
            Loop();
            CheckConnectionLoop();
        }
        async void Loop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(500);
            }
        }
        async void CheckConnectionLoop()
        {
            while (true)
            {
                if (!SocketConnected() && q)
                {
                    try { ShowMessage("Ошибка", "Потеряно соединение с сервером.", true); }
                    catch { }
                    q = false;
                }
                await Task.Delay(100);
            }
        }
        bool SocketConnected()
        {
            bool part1 = SCT.Socket.Poll(1000, SelectMode.SelectRead);
            bool part2 = (SCT.Socket.Available == 0);
            if (part1 && part2)
                return false;
            else
                return true;
        }
        void ShowMessage(string title, string message, bool exit)
        {
            dialog = new AlertDialog.Builder(this);
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