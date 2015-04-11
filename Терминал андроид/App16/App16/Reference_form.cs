using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.IO;
using System.Threading;

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
    public class ReferenceForm : Activity
    {
        TextView watch;
        Button bexit, bsend;
        EditText t1, t2, t3, t4, t5;
        LinearLayout mainlayout;
        bool q;
        Thread SleepThread;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Reference);

            watch = (TextView)FindViewById(Resource.Id.textClock);
            t1 = (EditText)FindViewById(Resource.Id.t1);
            t2 = (EditText)FindViewById(Resource.Id.t2);
            t3 = (EditText)FindViewById(Resource.Id.t3);
            t4 = (EditText)FindViewById(Resource.Id.t4);
            t5 = (EditText)FindViewById(Resource.Id.t5);
            bsend = (Button)FindViewById(Resource.Id.bsend); bsend.Click += button_Click; bsend.Tag = 1;
            bexit = (Button)FindViewById(Resource.Id.bexit3); bexit.Click += button_Click; bexit.Tag = 0;
            mainlayout = (LinearLayout)FindViewById(Resource.Id.mainlayout); mainlayout.Click += button_Click; mainlayout.Tag = 3;
            
            q = true;

            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();

            RefreshLoop();
            CheckConnectionLoop();
        }

        void Sleep(object ob)
        {
            Thread.Sleep(120000);
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

        private void button_Click(object sender, EventArgs e)
        {
            View s = (View)sender;
            switch(int.Parse(s.Tag.ToString()))
            {
                case 0:
                    StartActivity(typeof(Buttons));
                    this.Finish();
                    break;
                case 1:
                     if (t1.Text.Length != 6)
                        ShowMessage("Ошибка", "Корректный номер студенческого билета состоит из 6 цифр", false);
                     if (t2.Text == "" || t3.Text == "")
                        ShowMessage("Ошибка", "Поля с ФИО и датой рождения обязательно должны быть заполнены", false);
                     SendReference();
                    break;
            }
            SleepThread.Abort();
            SleepThread = new Thread(new ParameterizedThreadStart(Sleep));
            SleepThread.Start();
        }
        private async void SendReference()
        {
            if (t1.Text.Length == 6 && t2.Text.Trim() != "" && t3.Text.Trim() != "")
            {
                while (true)
                {
                    if (SCT.SCTisFree)
                    {
                        if(t2.Text.Contains('_')||t2.Text.Contains(';')||t3.Text.Contains('_')||t3.Text.Contains(';')||t4.Text.Contains('_')||t4.Text.Contains(';')||t5.Text.Contains('_')||t5.Text.Contains(';'))
                        {
                            ShowMessage("Ошибка", "Строки не должны содержать символы ';' и '_'", false);
                            break;
                        }
                        SCT.SCTisFree = false;
                        SCT.Send(requests.Refc1.ToString());
                        SCT.Send(t1.Text);
                        StaticData.ChangedNumber = t1.Text;
                        SCT.Send(String.Format("{0}_{1}_{2}_{3}", t2.Text, t3.Text, t4.Text, t5.Text));
                        if (bool.Parse(SCT.Receive()))
                        {
                            StaticData.CreateQRCode(String.Format("Вы успешно отправили заявку на справку.\nВведенные вами данные:\nНомер студенческого билета: {0}\nФИО: {1}\nДата рождения: {2}\nНаправление справки: {3}\nДополнительная информация: {4}\nВсю подробную информацию о продвижении очереди узнайте на сайте http://studok.misis.ru \n\nУдачного дня!", t1.Text, t2.Text, t3.Text, t4.Text, t5.Text));
                            StartActivity(typeof(QRCodeDisplayingReference));
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
                    await Task.Delay(500);
                }
            }
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