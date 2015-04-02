using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Content.PM;

namespace App5
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class Buttons : Activity
    {
        TextView watch;
        Button b1;
        Button b2;
        Button b3;
        Button b4;
        Button b5;
        Button b6; 
        Button b7;
        Button b8;
        Button b9;
        Button bexit;
        Button[] buttons;
        bool q;
        AlertDialog.Builder dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Buttons);

            watch = (TextView)FindViewById(Resource.Id.textClock);

            b1 = (Button)FindViewById(Resource.Id.b1); b1.Click += button_Click; b1.Tag = 1;
            b2 = (Button)FindViewById(Resource.Id.b2); b2.Click += button_Click; b2.Tag = 2;
            b3 = (Button)FindViewById(Resource.Id.b3); b3.Click += button_Click; b3.Tag = 3;
            b4 = (Button)FindViewById(Resource.Id.b4); b4.Click += button_Click; b4.Tag = 4;
            b5 = (Button)FindViewById(Resource.Id.b5); b5.Click += button_Click; b5.Tag = 5;
            b6 = (Button)FindViewById(Resource.Id.b6); b6.Click += button_Click; b6.Tag = 6;
            b7 = (Button)FindViewById(Resource.Id.b7); b7.Click += button_Click; b7.Tag = 7;
            b8 = (Button)FindViewById(Resource.Id.b8); b8.Click += button_Click; b8.Tag = 8;
            b9 = (Button)FindViewById(Resource.Id.b9); b9.Click += button_Click; b9.Tag = 9;
            bexit = (Button)FindViewById(Resource.Id.bexit); bexit.Click += button_Click; bexit.Tag = 0;
            buttons = new Button[] { b1, b2, b3, b4, b5, b6, b7, b8, b9 };
            q = true;
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
                    UpdatingInfo();
                await Task.Delay(500);
            }
        }

        private void UpdatingInfo()
        {
            for (int i = 0; i < Data.Size-1; i++)
                buttons[i].Text = Data.Config[i];
        }

        void button_Click(object sender, EventArgs e)
        {
            Button a = (Button)sender;
            switch((int)a.Tag)
            {
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                case 8:
                    break;
                case 9:
                    break;
                case 0:
                    this.Finish();
                    break;
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