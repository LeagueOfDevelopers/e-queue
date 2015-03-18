using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using System.Threading.Tasks;

namespace App5
{
    [Activity(Label = "App5", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Light.NoTitleBar.Fullscreen", ConfigurationChanges = ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.SensorLandscape)]
    public class MainActivity : Activity
    {
        TextView watch;
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.WatchQueue);

            watch = (TextView)FindViewById(Resource.Id.textClock);
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
    }
}

