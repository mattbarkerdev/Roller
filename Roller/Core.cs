using Roller.Classes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Roller
{
    public class Core
    {
        public static async Task<DamageOutput> RollDemDice(Loadout setup)
        {

            DamageOutput dm = new DamageOutput();
            await Task.Run(() =>
            {
                Parallel.ForEach(setup.AttackConfiguration, RollIt);
            });

            return dm;

            void RollIt(IRollable dice)
            {

                ConcurrentBag<double> variableWoundTracker = new ConcurrentBag<double>();

                Parallel.For(0, setup.NumberOfAttacks, i =>
                {
                    ConcurrentBag<double> attackTracker = new ConcurrentBag<double>();
                    Parallel.For(0, setup.IterationCount,
                        rollDice => { attackTracker.Add(dice.RollDice()); });
                    variableWoundTracker.Add(attackTracker.Sum() / setup.IterationCount);
                });


                dm.VariableOutput = (int)Math.Round(variableWoundTracker.Sum(),0);

                //dm.MortalWounds += dice.DamageOutput();
            }
        }

    }
}
