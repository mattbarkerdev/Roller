using Android.App;
using Android.OS;
using Android.Widget;
using System;
using System.Collections.Generic;
using Roller.Classes;


namespace Roller.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : Activity
    {

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);
            Button button = FindViewById<Button>(Resource.Id.blapBtn);

            button.Click += Button_Click;
        }

        private async void Button_Click(object sender, EventArgs e)
        {

            var basic = new Attack
            {
                ToHit = 3,
                ToWound = 3,
                RendModifier = 0,
                TargetSaveOn = 4,

                
            };

            List<IRollable> attacks = new List<IRollable> { basic };

            var rollz = new Loadout
            {
                NumberOfAttacks = 18,
                AttackConfiguration = attacks,
                IterationCount = 1
            };

            var result = await Core.RollDemDice(rollz);
            FindViewById<TextView>(Resource.Id.locationText).Text = result.VariableOutput.ToString();

        }
    }
}

