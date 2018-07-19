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

        //For cases like hits roll of 6 become d6 hits
        public HitMultiplier HitMultiplier { get; set; }
        public int HitMultiplierTrigger { get; set; }

        //For cases like hit rols of 4+ deal x mortal wounds
        public Damage MortalWounds { get; set; }
        public int MortalWoundTrigger { get; set; }

        public DamageOutput RollDemDice(Loadout setup)
        {
            return new DamageOutput
            {
                StandardVariableDamageSpread = CalculateDemStandardWoundProbabilities(setup),
                MortalWoundSpread = CalculateDemMortalWoundProbabilities(setup)
            };
        }
        internal double[] CalculateDemStandardWoundProbabilities(Loadout setup)
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
            for (int sucessfulHits = 0; sucessfulHits <= setup.NumberOfAttacks; sucessfulHits++)
            {
                double chanceOfIHits = Binomial(setup.NumberOfAttacks, sucessfulHits, hitChance);

                // j successful wounds
                for (int successfulWounds = 0; successfulWounds <= sucessfulHits; successfulWounds++)
                {
                    double chanceOfJWounds = Binomial(sucessfulHits, successfulWounds, woundChance);

                    // k failed saves
                    for (int successfulSaves = 0; successfulSaves <= successfulWounds; successfulSaves++)
                    {
                        double chanceOfKFailedSaves = Binomial(successfulWounds, successfulSaves, saveFailChance);
                        if (Damage != Damage.Specified)
                        {
                            //loop and add to damage outputs to get each
                            for (int damageCount = 1; damageCount <= returnMaxDamage(Damage); damageCount++)
                            {
                                int damage = successfulSaves * damageCount;
                                damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves) / returnMaxDamage(Damage);

                            }
                        }
                        else
                        {
                            //no variable damage if specified so just assign to that one
                            int damage = successfulSaves * SpecifiedDamage;
                            damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves);
                        }
                    }
                }
            }

            return damageProbArray;


        }
        internal double[] CalculateDemMortalWoundProbabilities(Loadout setup)
        {
            int maxHit;

            double hitChance = (double)(7 - MortalWoundTrigger) / 6;

            // Need max hit for array size initilaisation
            maxHit = (setup.NumberOfAttacks * returnMaxDamage(MortalWounds)) + 1;

            var damageProbArray = new double[maxHit];


           
            return damageProbArray;
            
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
            if (probability < 0 || probability > 1)
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

    public enum HitMultiplier
    {
        None = 0,
        D3 = 1,
        D6 = 2,
        Specified = 3,
    }
}
