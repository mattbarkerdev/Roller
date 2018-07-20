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
                Damage = Damage.Single,
                SpecifiedDamage = 3,
                MortalWounds =  Damage.D6,
                MortalWoundTrigger = 6
            };

            List<IRollable> attacks = new List<IRollable> { basic };

            var rollz = new Loadout
            {
                NumberOfAttacks =1,
                AttackConfiguration = attacks
            };

            var result = await Core.RollDemDice(rollz);
            int tracker = 0;
            FindViewById<TextView>(Resource.Id.variableText).Text = "Variable Damage: " + System.Environment.NewLine;
            foreach (double d in result.StandardVariableDamageSpread)
            {
                //only show if more than 1% chance
                if (d > 0.009)
                {
                    FindViewById<TextView>(Resource.Id.variableText).Text +=
                        $"Damage [{tracker}] : {(Math.Round(d, 2)*100)}%" + System.Environment.NewLine;
                }

                tracker++;
            }

            FindViewById<TextView>(Resource.Id.variableText).Text += System.Environment.NewLine + "Mortal Damage: " + System.Environment.NewLine;
            int mtracker = 0;

            foreach (double d in result.MortalWoundSpread)
            {
                //only show if more than 1% chance
                if (d > 0.009)
                {
                    FindViewById<TextView>(Resource.Id.variableText).Text +=
                        $"Damage [{mtracker}] : {(Math.Round(d, 2) * 100)}%" + System.Environment.NewLine;
                }

                mtracker++;
            }



        }
    }
}

