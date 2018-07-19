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
               dm = dice.RollDemDice(setup);
            }
        }
    }
}
