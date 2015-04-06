using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;

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
    public class InputingNumberMain : Activity
    {
        TextView watch;
        EditText number;
        Button GetNumber;
        Button EnterQueue;
        Button bexit;
        bool q, q2;
        string numstr;
        AlertDialog.Builder dialog;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.InputingNumberMain);

            watch = (TextView)FindViewById(Resource.Id.textClock);

            numstr = "";
            number = (EditText)FindViewById(Resource.Id.text_number);
            GetNumber = (Button)FindViewById(Resource.Id.bgetnum); GetNumber.Click += button_Click; GetNumber.Tag = 1;
            EnterQueue = (Button)FindViewById(Resource.Id.bentque); EnterQueue.Click += button_Click; EnterQueue.Tag = 2;
            bexit = (Button)FindViewById(Resource.Id.bexit2); 
            bexit.Click += button_Click; 
            bexit.Tag = 0;
            q = true;
            q2 = false;
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
                if (q2) { number.Text = numstr; q2 = false; }
                await Task.Delay(500);
            }
        }

        void button_Click(object sender, EventArgs e)
        {
            Button a = (Button)sender;
            switch((int)a.Tag)
            {
                case 1:
                    new Thread(new ParameterizedThreadStart(GetRandomNumber)).Start();
                    break;
                case 2:
                    if (!(number.Text.Length == 6 || (number.Text.Length == 4 && number.Text == numstr)))
                        ShowMessage("Ошибка", "Используйте номер реального студенческого билета, либо получите случайный номер в системе.", false);
                    new Thread(new ParameterizedThreadStart(SendPurpose)).Start();
                    break;
                case 0:
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
            }
        }
        void GetRandomNumber(object ob)
        {
            while (true)
                if (SCT.SCTisFree) { SCT.SCTisFree = false; SCT.Send(requests.GetRN.ToString()); numstr = SCT.Receive(); q2 = true; SCT.SCTisFree = true; break; }
        }
        void SendPurpose(object ob)
        {
            while (true)
                if (SCT.SCTisFree)
                {
                    if (number.Text.Length == 6||(number.Text.Length == 4&&number.Text==numstr) )
                    {
                        SCT.SCTisFree = false;
                        SCT.Send("prop" + Data.button);
                        SCT.Send(number.Text);
                        Data.number = number.Text;
                        StartActivity(typeof(QRCodeDisplaying));
                        SCT.SCTisFree = true;
                        this.Finish();
                    }
                    else { numstr = ""; q2 = true; }
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