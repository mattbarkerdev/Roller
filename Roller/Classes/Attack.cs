using System;
using System.Collections.Generic;
using System.Text;
using DieRoller;
using Roller.Util;

namespace Roller.Classes
{
    public class Attack : IRollable
    {
        public int ToHit { get; set; }
        public int ToWound { get; set; }
        public int RendModifier { get; set; }
        public int TargetSaveOn { get; set; }

        public Damage Damage { get; set; } = Damage.Single;
        public int SpecifiedDamage { get; set; }

        public RerollBehaviour HitReRoll { get; set; } = RerollBehaviour.None;
        public RerollBehaviour WoundReRoll { get; set; } = RerollBehaviour.None;
        public RerollBehaviour SaveReRoll { get; set; } = RerollBehaviour.None;

        //For cases like hit rols of 4+ deal x mortal wounds
        public Damage MortalWounds { get; set; }
        public int MortalWoundTrigger { get; set; }
        // For cases where normal damage is dealt on top of mortal wounds (e.g fireglaives)
        public bool MortalWoundsEndAttackSequence { get; set; }



        //advanced conditionals
        //case 1: hits roll of 6 become d6 hits
        public HitMultiplier HitMultiplier { get; set; } = HitMultiplier.None;
        public int HitMultiplierTrigger { get; set; }
        public int SpecifiedHitMultiple { get; set; }



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
            var hitProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(ToHit))
                .WithReroll(returnHitReroll(HitReRoll))
                .Build();
            double hitChance = (double) hitProbability.CalculateProbability();

            var woundProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(ToWound))
                .WithReroll(returnHitReroll(WoundReRoll))
                .Build();
            double woundChance = (double) woundProbability.CalculateProbability();

            var saveProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(TargetSaveOn))
                .WithReroll(returnHitReroll(SaveReRoll))
                .Build();

            double saveChance = (double)saveProbability.CalculateProbability();
            double saveFailChance = (double)(1 - saveChance);
            double hitWithoutMortalWoundChance = (double)(hitChance - ((7 - MortalWoundTrigger) / 6));

            // Need max hit for array size initilaisation
            var maxHit = (setup.NumberOfAttacks * returnMaxDamage(Damage)) +1;

            var damageProbArray = new double[maxHit];

            // i successful hits
            for (int sucessfulHits = 0; sucessfulHits <= setup.NumberOfAttacks; sucessfulHits++)
            {
                double chanceOfIHits;

                if (MortalWoundsEndAttackSequence)
                {
                    chanceOfIHits = Binomial(setup.NumberOfAttacks, sucessfulHits, hitWithoutMortalWoundChance);
                }
                else
                {
                    chanceOfIHits = Binomial(setup.NumberOfAttacks, sucessfulHits, hitChance);
                }

                // j successful wounds
                for (int successfulWounds = 0; successfulWounds <= sucessfulHits; successfulWounds++)
                {
                    double chanceOfJWounds = Binomial(sucessfulHits, successfulWounds, woundChance);

                    // k failed saves
                    for (int failedSaves = 0; failedSaves <= successfulWounds; failedSaves++)
                    {
                        double chanceOfKFailedSaves = Binomial(successfulWounds, failedSaves, saveFailChance);
                        if (Damage != Damage.Specified)
                        {
                            //loop and add to damage outputs to get each
                            for (int damageCount = 1; damageCount <= returnMaxDamage(Damage); damageCount++)
                            {
                                int damage = failedSaves * damageCount;
                                damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves) / returnMaxDamage(Damage);
                            }
                        }
                        else
                        {
                            //no variable damage if specified so just assign to that one
                            int damage = failedSaves * SpecifiedDamage;
                            damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves);
                        }
                    }
                }
            }

            return damageProbArray;
        }

        internal double[] CalculateDemStandardWoundProbabilitiesMk2(Loadout setup)
        {
            double[] oneAttackDamageProbArray;
            double[] damageProbArray;

            oneAttackDamageProbArray = CalculateDemStandardWoundProbabilitiesForOneAttack(setup);

            damageProbArray = MultiplyProbabilityArray(oneAttackDamageProbArray, setup);

            return damageProbArray;
        }

        internal double[] MultiplyProbabilityArray(double[] oneAttackDamageProbArray, Loadout setup)
        {
            double[] damageProbArray;

            damageProbArray = new double[(oneAttackDamageProbArray.Length - 1) * setup.NumberOfAttacks];

            if (setup.NumberOfAttacks == 1)
            {
                damageProbArray = oneAttackDamageProbArray;
            }
            else
            {
                for (int curIteration = 1; curIteration < setup.NumberOfAttacks; curIteration++)
                {
                    if (curIteration == 1)
                    {
                        damageProbArray = oneAttackDamageProbArray;
                    }

                    damageProbArray = doMultiply(damageProbArray, oneAttackDamageProbArray);
                }
            }

            return damageProbArray;
        }

        internal double[] doMultiply(double[] array1, double[] array2)
        {
            double[] resultArray = new double[(array1.Length - 1) + (array2.Length - 1)];

            for (int i = 0; i < array1.Length; i++)
            {
                for (int j = 0; j < array1.Length; j++)
                {
                    resultArray[i + j] = array1[i] * array2[j];
                }
            }

            return resultArray;
        }

        internal double[] CalculateDemStandardWoundProbabilitiesForOneAttack(Loadout setup)
        {
            var hitProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(ToHit))
                .WithReroll(returnHitReroll(HitReRoll))
                .Build();
            double hitChance = (double)hitProbability.CalculateProbability();

            var woundProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(ToWound))
                .WithReroll(returnHitReroll(WoundReRoll))
                .Build();
            double woundChance = (double)woundProbability.CalculateProbability();

            var saveProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(TargetSaveOn))
                .WithReroll(returnHitReroll(SaveReRoll))
                .Build();
            double saveChance = (double)saveProbability.CalculateProbability();

            double saveFailChance = (double)(1 - saveChance);
            double hitWithoutMortalWoundChance = (double)(hitChance - ((7 - MortalWoundTrigger) / 6));
            double hitWithoutHitMultiplierChance = (double)(hitChance - ((7 - HitMultiplierTrigger) / 6));

            // Need max hit for array size initilaisation
            var maxHit = (returnMaxHitExplosion(HitMultiplier) * returnMaxDamage(Damage)) + 1;

            var damageProbArray = new double[maxHit];

            // Missed hit, so 0 damage
            damageProbArray[0] = 1 - hitChance;

            // Start from 1 instead of 0 (since we handled 0 above)
            for (int sucessfulHits = 1; sucessfulHits <= returnMaxHitExplosion(HitMultiplier); sucessfulHits++)
            {
                double chanceOfIHits;

                // Mortal wounds done and attack sequence ends
                if (MortalWoundsEndAttackSequence)
                {
                    chanceOfIHits = Binomial(setup.NumberOfAttacks, sucessfulHits, hitWithoutMortalWoundChance);
                }
                // All successful hits are multiplied
                else if (ToHit == HitMultiplierTrigger)
                {
                    chanceOfIHits = hitChance / returnMaxHitExplosion(HitMultiplier);
                }
                // Only hit rolls of x+ are multiplied
                else
                {
                    if (sucessfulHits == 1)
                    {
                        chanceOfIHits = hitWithoutHitMultiplierChance + ((hitChance - hitWithoutHitMultiplierChance) / returnMaxHitExplosion(HitMultiplier)) ;
                    }
                    else
                    {
                        chanceOfIHits = (hitChance - hitWithoutHitMultiplierChance) / returnMaxHitExplosion(HitMultiplier);
                    }
                }

                // j successful wounds
                for (int successfulWounds = 0; successfulWounds <= sucessfulHits; successfulWounds++)
                {
                    double chanceOfJWounds = Binomial(sucessfulHits, successfulWounds, woundChance);

                    // k failed saves
                    for (int failedSaves = 0; failedSaves <= successfulWounds; failedSaves++)
                    {
                        double chanceOfKFailedSaves = Binomial(successfulWounds, failedSaves, saveFailChance);
                        if (Damage != Damage.Specified)
                        {
                            //loop and add to damage outputs to get each
                            for (int damageCount = 1; damageCount <= returnMaxDamage(Damage); damageCount++)
                            {
                                int damage = failedSaves * damageCount;
                                damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves) / returnMaxDamage(Damage);
                            }
                        }
                        else
                        {
                            //no variable damage if specified so just assign to that one
                            int damage = failedSaves * SpecifiedDamage;
                            damageProbArray[damage] += (chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves);
                        }
                    }
                }
            }

            return damageProbArray;
        }


        internal double[] CalculateDemMortalWoundProbabilities(Loadout setup)
        {
            var hitProbability = RollBuilder.WithDie(Die.D6)
                .Targeting(Target.ValueAndAbove(MortalWoundTrigger))
                .WithReroll(Reroll.None)
                .Build();
            double hitChance = (double)hitProbability.CalculateProbability();

            // Need max hit for array size initilaisation
            var maxHit = (setup.NumberOfAttacks * returnMaxDamage(MortalWounds)) + 1;

            var damageProbArray = new double[maxHit];

            for (int sucessfulHits = 0; sucessfulHits <= setup.NumberOfAttacks; sucessfulHits++)
            {
                double chanceOfIHits = Binomial(setup.NumberOfAttacks, sucessfulHits, hitChance);

                if (Damage != Damage.Specified)
                {
                    //loop and add to damage outputs to get each
                    for (int damageCount = 1; damageCount <= returnMaxDamage(MortalWounds); damageCount++)
                    {
                        int damage = sucessfulHits * damageCount;
                        damageProbArray[damage] += (chanceOfIHits) / returnMaxDamage(MortalWounds);
                    }
                }
                else
                {
                    //no variable damage if specified so just assign to that one
                    int damage = sucessfulHits * SpecifiedDamage;
                    damageProbArray[damage] += chanceOfIHits;
                }
            }


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

        internal int returnMaxHitExplosion(HitMultiplier hitMultiplier)
        {
            switch (hitMultiplier)
            {
                case HitMultiplier.None:
                    return 1;
                case HitMultiplier.D3:
                    return 3;
                case HitMultiplier.D6:
                    return 6;
                case HitMultiplier.Specified:
                    return SpecifiedHitMultiple;
                default:
                    return 1;
            }
        }

        internal IRerollBehaviour returnHitReroll(RerollBehaviour toRoll)
        {
            switch (toRoll)
            {
                case RerollBehaviour.Ones:
                    return Reroll.Ones;
                case RerollBehaviour.Fails:
                    return Reroll.Failures;
                case RerollBehaviour.None:
                    return Reroll.None;
                default:
                    return Reroll.None;
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

    public enum RerollBehaviour
    {
        None =0,
        Ones = 1,
        Fails = 2,
    }


    public enum HitMultiplier
    {
        None = 0,
        D3 = 1,
        D6 = 2,
        Specified = 3,
    }
}
