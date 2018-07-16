using System;

namespace Roller.Util
{
    public static class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();

        [ThreadStatic]
        private static Random _local;

        private static Random Inst
        {
            get
            {
                if (_local == null)
                {
                    int seed;
                    lock (_global)
                    {
                        seed = _global.Next();
                    }
                    _local = new Random(seed);
                }
                return _local;
            }
        }

        /// <summary>
        /// Returns a 32-bit signed integer greater than or equal to <paramref name="min"/> and less than <paramref name="max"/>;
        /// that is, the range of return values includes <paramref name="min"/> but not <paramref name="max"/>.
        /// If <paramref name="min"/> equals <paramref name="max"/>, <paramref name="min"/> is returned.
        /// </summary>
        public static int Next(int min, int max) => Inst.Next(min, max);

        /// <summary>
        /// Returns a random floating-point number that is greater than or equal to 0.0, and less than 1.0.
        /// </summary>
        public static double NextDouble() => Inst.NextDouble();
    }
}

