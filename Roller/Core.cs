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
        public static double[] CalculateDemProbabilities(Loadout setup/*, Attack attackProfile*/)
        {
            double[] damageProbArray;
            int maxHit;
            // TODO Hard coded attack profile for now
            Attack attackProfile = new Attack
            {
                ToHit = 3,
                ToWound = 3,
                RendModifier = 0,
                TargetSaveOn = 4,
            };
            double hitChance = (7 - attackProfile.ToHit) / 6;
            double woundChance = (7 - attackProfile.ToWound) / 6;
            double saveChance = (7 - (attackProfile.TargetSaveOn + attackProfile.RendModifier)) / 6;
            double saveFailChance = 1 - saveChance;

            // Need max hit for array size initilaisation
            // TODO Just assuming damage 1 for now since damage types not setup in loadouts yet
            maxHit = setup.NumberOfAttacks * 1;

            damageProbArray = new double[maxHit];

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
                        int damage = k * 1;

                        damageProbArray[damage] += chanceOfIHits * chanceOfJWounds * chanceOfKFailedSaves;
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
                double result;

                if (numAttempts < successes)
                    throw (new ArgumentException("Number of successes cannot be greater than the number of attempts"));
                if (probability < 0 || probability > 1)
                    throw (new ArgumentOutOfRangeException("Probability must be between 0 and 1 inclusive"));

                result = (Factorial(numAttempts)) / (Factorial(successes) * Factorial(numAttempts - successes)) * Math.Pow(probability, successes) * Math.Pow(1 - probability, numAttempts - successes);

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


                dm.VariableOutput = (int)Math.Round(variableWoundTracker.Sum(), 0);

                //dm.MortalWounds += dice.DamageOutput();
            }
        }
    }
}
