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
using Android.Media;

namespace App16
{
    class Player
    {
        private static string number;
        private static MediaPlayer[] player;
        public static List<int> sounds { get; set; }

        public static void InviteNextNumber(int num)
        {
            number = num.ToString();
            sounds = new List<int>() 
            {            
                Resource.Raw.bumbumbum,
                Resource.Raw.attention,
                Resource.Raw.invited,
                Resource.Raw.visitorWithNumber
            };
            AddDigits();
            play();
        }
        private static void play()
        {
            player = new MediaPlayer[sounds.Count];
            for (int i = 0; i < player.Length; i++)
                player[i] = MediaPlayer.Create(Application.Context, sounds[i]);
            for (int i = 0; i < player.Length - 1; i++)
                player[i].SetNextMediaPlayer(player[i + 1]);
            player[0].Start();
        }
        private static void AddDigits()
        {
            for (int i = 0; i < 2; i++)
            {
                if (number.Length == 4)
                {
                    switch(number[i*2])
                    {
                        case '1':
                            sounds.Add(Resource.Raw.n10);
                            break;
                        case '2':
                            sounds.Add(Resource.Raw.n20);
                            break;
                        case '3':
                            sounds.Add(Resource.Raw.n30);
                            break;
                        case '4':
                            sounds.Add(Resource.Raw.n40);
                            break;
                        case '5':
                            sounds.Add(Resource.Raw.n50);
                            break;
                        case '6':
                            sounds.Add(Resource.Raw.n60);
                            break;
                        case '7':
                            sounds.Add(Resource.Raw.n70);
                            break;
                        case '8':
                            sounds.Add(Resource.Raw.n80);
                            break;
                        case '9':
                            sounds.Add(Resource.Raw.n90);
                            break;
                    }
                    switch (number[i * 2 + 1])
                    {
                        case '1':
                            sounds.Add(Resource.Raw.n1);
                            break;
                        case '2':
                            sounds.Add(Resource.Raw.n2);
                            break;
                        case '3':
                            sounds.Add(Resource.Raw.n3);
                            break;
                        case '4':
                            sounds.Add(Resource.Raw.n4);
                            break;
                        case '5':
                            sounds.Add(Resource.Raw.n5);
                            break;
                        case '6':
                            sounds.Add(Resource.Raw.n6);
                            break;
                        case '7':
                            sounds.Add(Resource.Raw.n7);
                            break;
                        case '8':
                            sounds.Add(Resource.Raw.n8);
                            break;
                        case '9':
                            sounds.Add(Resource.Raw.n9);
                            break;
                    }
                }
                else
                {
                    switch (number[i * 3])
                    {
                        case '1':
                            sounds.Add(Resource.Raw.n100);
                            break;
                        case '2':
                            sounds.Add(Resource.Raw.n200);
                            break;
                        case '3':
                            sounds.Add(Resource.Raw.n300);
                            break;
                        case '4':
                            sounds.Add(Resource.Raw.n400);
                            break;
                        case '5':
                            sounds.Add(Resource.Raw.n500);
                            break;
                        case '6':
                            sounds.Add(Resource.Raw.n600);
                            break;
                        case '7':
                            sounds.Add(Resource.Raw.n700);
                            break;
                        case '8':
                            sounds.Add(Resource.Raw.n800);
                            break;
                        case '9':
                            sounds.Add(Resource.Raw.n900);
                            break;
                    }
                    switch (number[i * 3+1])
                    {
                        case '1':
                            sounds.Add(Resource.Raw.n10);
                            break;
                        case '2':
                            sounds.Add(Resource.Raw.n20);
                            break;
                        case '3':
                            sounds.Add(Resource.Raw.n30);
                            break;
                        case '4':
                            sounds.Add(Resource.Raw.n40);
                            break;
                        case '5':
                            sounds.Add(Resource.Raw.n50);
                            break;
                        case '6':
                            sounds.Add(Resource.Raw.n60);
                            break;
                        case '7':
                            sounds.Add(Resource.Raw.n70);
                            break;
                        case '8':
                            sounds.Add(Resource.Raw.n80);
                            break;
                        case '9':
                            sounds.Add(Resource.Raw.n90);
                            break;
                    }
                    switch (number[i * 3 + 2])
                    {
                        case '1':
                            sounds.Add(Resource.Raw.n1);
                            break;
                        case '2':
                            sounds.Add(Resource.Raw.n2);
                            break;
                        case '3':
                            sounds.Add(Resource.Raw.n3);
                            break;
                        case '4':
                            sounds.Add(Resource.Raw.n4);
                            break;
                        case '5':
                            sounds.Add(Resource.Raw.n5);
                            break;
                        case '6':
                            sounds.Add(Resource.Raw.n6);
                            break;
                        case '7':
                            sounds.Add(Resource.Raw.n7);
                            break;
                        case '8':
                            sounds.Add(Resource.Raw.n8);
                            break;
                        case '9':
                            sounds.Add(Resource.Raw.n9);
                            break;
                    }
                }
            }
        }
    }
}