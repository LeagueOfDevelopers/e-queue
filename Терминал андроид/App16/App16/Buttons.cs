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

namespace App16
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
        bool q;//переменная для проверки соединения(отсечка)
        bool w; // переменная дляотмены перезапуска

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
            q = true; w = true;

            RefreshLoop();
            CheckConnectionLoop();
            Sleep();

            Window.DecorView.SystemUiVisibility = StaticData.Flags;
            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        private async void Sleep()
        {
            await Task.Delay(30000);
            w = false;
            this.Finish();
        }//таймер бездействия

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                UpdateInfo();
                await Task.Delay(200);
            }
        }
        private void UpdateInfo()
        {
            try
            {
                for (int i = 0; i < StaticData.Size - 1; i++)
                    buttons[i].Text = StaticData.Config[i];
            }
            catch { }
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
                    await Task.Delay(3000);
                    q = false;
                    StartActivity(typeof(MainActivity));
                    this.Finish();
                }
                await Task.Delay(3000);
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
                    w = false;
                    this.Finish();
                }
            });
            dialog.Show();
        }

        private void button_Click(object sender, EventArgs e)
        {
            Button a = (Button)sender;
            w = false;
            switch ((int)a.Tag)
            {
                case 1:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberFastRef));
                    this.Finish();
                    break;
                case 2:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberFast));
                    this.Finish();
                    break;
                case 3:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(ReferenceForm));
                    this.Finish();
                    break;
                case 4:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 5:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 6:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 7:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 8:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 9:
                    StaticData.ChangedButton = (int)a.Tag;
                    StartActivity(typeof(InputingNumberMain));
                    this.Finish();
                    break;
                case 0:
                    StartActivity(typeof(DisplayingQueue));
                    this.Finish();
                    break;
            }
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
            if (w)
            {
                Intent intent = new Intent(Intent.ActionMain);
                intent.AddCategory("EQueueLouncher");
                this.Finish();
                StaticData.StopUpdating();
                StartActivity(intent);
            }
        }
    }
}