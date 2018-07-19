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

        public Damage Damage { get; set; }
        public int SpecifiedDamage { get; set; }

        public ToHitReRoll HitReRoll { get; set; }
        public ToWoundReRoll WoundReRoll { get; set; }
        public TargetSaveReRoll SaveReRoll { get; set; }

        public double[] CalculateDemProbabilities(Loadout setup)
        {
            int maxHit;
         
            double hitChance = (double)(7 - ToHit) / 6;
            double woundChance = (double)(7 - ToWound) / 6;
            double saveChance = (double)(7 - (TargetSaveOn + RendModifier)) / 6;
            double saveFailChance = (double)(1 - saveChance);

            // Need max hit for array size initilaisation
            maxHit = (setup.NumberOfAttacks * returnMaxDamage(Damage)) +1;

            var damageProbArray = new double[maxHit];

            // i successful hits
            for (int i = 0; i <= setup.NumberOfAttacks; i++)
            {
                double chanceOfIHits = Binomial(setup.NumberOfAttacks, i, hitChance);

                // j successful wounds
                for (int j = 0; j <= i; j++)
                {
                    double chanceOfJWounds = Binomial(i, j, woundChance);

                    // k failed saves
                    for (int k = 0; k <= j; k++)
                    {
                        double chanceOfKFailedSaves = Binomial(j, k, saveFailChance);
                        // TODO Just assuming damage 1 for now since damage types not setup in loadouts yet
                        if (Damage != Damage.Specified)
                        {
                            //loop and add to damage outputs
                            for (int d = 1; d <= returnMaxDamage(Damage); d++)
                            {
                                int damage = k * d;
                                damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves) / returnMaxDamage(Damage);

                            }
                        }
                        else
                        {
                            int damage = k * SpecifiedDamage;
                            damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves);
                        }
                    }
                }
            }

            return damageProbArray;


            /*******************************************
             * LOCAL METHODS
            *******************************************/
            // Binomial method for getting the probability of x of a result in n throws
            // n = numAttempts; k = successes; p = probability
            // Function is: Prob of k successes in n attempts with probability p = (n!/(k!(n-k)!))*(p^k)*((1-p)^(n-k))
            double Binomial(int numAttempts, int successes, double probability)
            {
                if (numAttempts < successes)
                    throw (new ArgumentException("Number of successes cannot be greater than the number of attempts"));
               if(probability < 0 || probability > 1)
                    throw (new ArgumentOutOfRangeException("Probability must be between 0 and 1 inclusive"));

                var result = Factorial(numAttempts) / (Factorial(successes) * Factorial(numAttempts - successes)) * Math.Pow(probability, successes) * Math.Pow(1 - probability, numAttempts - successes);

                return result;
            }

            // Factorial method for probability calculations
            // Needs to be double to support massive output values
            // Supports up to 170 (i think) - output is ~7.25^306
            double Factorial(int i)
            {
                if (i < 0)
                    throw (new ArgumentOutOfRangeException("Factorial of negative numbers is not defined"));
                if (i == 0 || i == 1)
                    return 1;
                return i * Factorial(i - 1);
            }
        }

        internal int returnMaxDamage(Damage damageProfile)
        {
            switch (damageProfile)
            {
                case Damage.Single:
                    return 1;
                case Damage.D3:
                    return 3;
                case Damage.D6:
                    return 6;
                case Damage.Specified:
                    return SpecifiedDamage;
                default:
                    return 1;
            }
        }
    }

    public class Loadout
    {
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

    public enum ToHitReRoll
    {
        None =0,
        Ones = 1,
        OneAndTwos = 2,
        Fails = 3,
    }
    public enum ToWoundReRoll
    {
        None = 0,
        Ones = 1,
        OneAndTwos = 2,
        Fails = 3,
    }
    public enum TargetSaveReRoll
    {
        None = 0,
        Ones = 1,
        OneAndTwos = 2,
        Fails = 3,
    }
}
