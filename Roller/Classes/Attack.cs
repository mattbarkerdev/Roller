using System;
using System.Collections.Generic;
using System.Text;
using Roller.Util;

namespace Roller.Classes
{
    public class Attack : IRollable
    {
        public int ToHit { get; set; }
        public int ToWound { get; set; }
        public int RendModifier { get; set; }
        public int TargetSaveOn { get; set; }

        public int RollDice()
        {
            //return 1;
            var hitRoll = ThreadSafeRandom.Next(1, 7);

            if (hitRoll >= ToHit)
            {
                var woundRoll = ThreadSafeRandom.Next(1, 7);
                if (woundRoll >= ToWound)
                {
                    var saveRoll = ThreadSafeRandom.Next(1, 7);
                    if (saveRoll >= (TargetSaveOn + RendModifier))
                    {
                        return 0;
                    }
                    return 1;
                    //switch (DamageType)
                    //{
                    //    case Damage.Single:
                    //        return 1;
                    //    case Damage.D3:
                    //        return ThreadSafeRandom.Next(1, 4);
                    //    case Damage.D6:
                    //        return ThreadSafeRandom.Next(1, 7);
                    //    case Damage.Specified:
                    //        return DamageValue ?? 0;
                    //    default:
                    //        return 0;
                    //}

                }
            }

            return 0;
        }

    }

    public class Loadout
    {
        public int IterationCount { get; set; } = 1;
        public int NumberOfAttacks { get; set; } = 0;

        public IEnumerable<IRollable> AttackConfiguration { get; set; }

    }

    public enum Damage
    {
        Single = 0,
        D3 = 1,
        D6 = 2,
        Specified = 3,
    }

}
