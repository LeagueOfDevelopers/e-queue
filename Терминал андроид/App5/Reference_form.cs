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
using System.Threading;

namespace App5
{
    [Activity(Label = "EQueue", MainLauncher = false, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class ReferenceForm : Activity
    {
        TextView watch;
        Button bexit, bsend;
        EditText t1, t2, t3, t4, t5;
        bool q;
        AlertDialog.Builder dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Reference);

            t1 = (EditText)FindViewById(Resource.Id.t1);
            t2 = (EditText)FindViewById(Resource.Id.t2);
            t3 = (EditText)FindViewById(Resource.Id.t3);
            t4 = (EditText)FindViewById(Resource.Id.t4);
            t5 = (EditText)FindViewById(Resource.Id.t5);
            watch = (TextView)FindViewById(Resource.Id.textClock);
            bexit = (Button)FindViewById(Resource.Id.bexit3); bexit.Click += bexit_Click; bexit.Tag = 1;
            bsend = (Button)FindViewById(Resource.Id.bsend); bsend.Click += bexit_Click; bsend.Tag = 2;
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

        private void bexit_Click(object sender, EventArgs e)
        {
            Button s = (Button)sender;
            switch(int.Parse(s.Tag.ToString()))
            {
                case 1:
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
                case 2:
                     if (t1.Text.Length != 6)
                        ShowMessage("Ошибка", "Номер студенческого билета состоит из 6 цифр", false);
                     if (t2.Text == "" || t3.Text == "")
                        ShowMessage("Ошибка", "Поля с ФИО и датой рождения обязательно должны быть заполнены", false);
                    new Thread(new ParameterizedThreadStart(SendReference)).Start();
                    break;
            }
        }

        private void SendReference(object obj)
        {
            if (t1.Text.Length == 6&&t2.Text!=""&&t3.Text!="")
            {
                while (true)
                {
                    if (SCT.SCTisFree)
                    {
                        SCT.SCTisFree = false;
                        SCT.Send(requests.Refc1.ToString());
                        SCT.Send(t1.Text);
                        Data.number = t1.Text;
                        SCT.Send(String.Format("{0}_{1}_{2}_{3}", t2.Text, t3.Text, t4.Text, t5.Text));
                        StartActivity(typeof(QRCodeDisplayingReference));
                        SCT.SCTisFree = true;
                        this.Finish();
                        break;
                    }
                }
            }
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