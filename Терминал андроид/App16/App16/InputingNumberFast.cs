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
    public class InputingNumberFast : Activity
    {
        TextView watch;
        EditText number;
        Button bEnterQueue;
        Button bexit;
        LinearLayout mainlayout;
        bool q, q2;//первая для проверки соединения, вторая для обновления текстового поля
        bool exit; // переменная дляотмены перезапуска
        string numstr;
        Thread SleepThread;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.InputingNumberFast);

            watch = (TextView)FindViewById(Resource.Id.textClock); watch.SystemUiVisibility = (StatusBarVisibility)Android.Views.SystemUiFlags.HideNavigation;
            number = (EditText)FindViewById(Resource.Id.text_number);
            bEnterQueue = (Button)FindViewById(Resource.Id.bentque); bEnterQueue.Click += button_Click; bEnterQueue.Tag = 2;
            bexit = (Button)FindViewById(Resource.Id.bexit2); bexit.Click += button_Click; bexit.Tag = 0;
            mainlayout = (LinearLayout)FindViewById(Resource.Id.mainlayout); mainlayout.Click += button_Click; mainlayout.Tag = 3;

            q = true; q2 = false; exit = true;

            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();

            RefreshLoop();
            CheckConnectionLoop();

            Window.DecorView.SystemUiVisibilityChange += DecorView_SystemUiVisibilityChange;
        }

        private void Sleep(object ob)
        {
            Thread.Sleep(30000);
            exit = false;
            this.Finish();
        }

        private async void RefreshLoop()
        {
            while (true)
            {
                watch.Text = "Время:\n" + DateTime.Now.ToString("HH:mm:ss");
                if (q2) { number.Text = numstr; q2 = false; }
                await Task.Delay(200);
            }
        }

        private void button_Click(object sender, EventArgs e)
        {
            View a = (View)sender;
            switch((int)a.Tag)
            {
                case 2:
                    SendPurpose();
                    break;
                case 0:
                    exit = false;
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
            }
            SleepThread.Abort();
            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();
        }
        private async void SendPurpose()
        {
            while (true)
                if (SCT.SCTisFree)
                {
                    if (number.Text.Length == 6)
                    {
                        SCT.SCTisFree = false;
                        SCT.Send("prop" + StaticData.ChangedButton);
                        SCT.Send(number.Text);
                        StaticData.ChangedNumber = int.Parse(number.Text);
                        if (bool.Parse(SCT.Receive()))
                        {
                            exit = false;
                            StaticData.CreateQRCode(String.Format("Вы успешно встали в очередь по коротким вопросам, ваш номер: {0:s}.\nВсю подробную информацию о продвижении очереди узнайте на сайте http://studok.misis.ru \n\nУдачного дня!", number.Text));
                            StartActivity(typeof(QRCodeDisplaying));
                            SCT.SCTisFree = true;
                            this.Finish();
                        }
                        else
                        {
                            SCT.SCTisFree = true;
                            ShowMessage("Ошибка", "Не удалось выполнить операцию, повторите попытку", false);
                        }
                        break;
                    }
                    else
                    {
                        numstr = ""; q2 = true; ShowMessage("Ошибка", "Корректный номер студенческого билета состоит из 6 цифр", false);
                    }
                    break;
                } await Task.Delay(500);
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
                    exit = false;
                    this.Finish();
                }
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
                StaticData.StopUpdating();
                StartActivity(intent);
            }
        }
    }
}