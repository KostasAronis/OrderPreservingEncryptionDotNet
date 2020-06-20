using System;
using System.Collections;
using System.Collections.Generic;

namespace OrderPreservingEncryptionDotNet
{
    public static class Stat
    {
        public static long SampleUniform(ValueRange valueRange, CoinFlipper coins)
        {
            var currentRange = valueRange.Copy();
            if (currentRange.Size() == 0)
            {
                throw new Exception("");
            }
            while (currentRange.Size() > 1)
            {
                var mid = (currentRange.Start + currentRange.End) / 2;
                var bit = coins.GetCoin();
                if ((bool)bit)
                {
                    currentRange.Start = mid + 1;
                }
                else
                {
                    currentRange.End = mid;
                }
            }
            return currentRange.Start;
        }

        /*
         Get a sample from the hypergeometric distribution, using the provided bit list as a source of randomness
         */
        internal static long SampleHGD(ValueRange inRange, ValueRange outRange, long mid, CoinFlipper coins)
        {
            var inSize = inRange.Size();
            var outSize = outRange.Size();
            if (inSize < 0 || outSize < 0 || inSize > outSize || !outRange.Contains(mid))
            {
                throw new ArgumentException();
            }

            var nSampleIndex = mid - outRange.Start + 1;
            if (inSize == outSize)
            {
                return inRange.Start + nSampleIndex - 1;
            }

            double inSampleNum = RHyper(nSampleIndex, inSize, outSize - inSize, coins);

            if (inSampleNum == 0)
            {
                return inRange.Start;
            }
            else
            {
                var inSample = inRange.Start + inSampleNum - 1;
                if (!inRange.Contains(Convert.ToInt32(inSample)))
                {
                    throw new ArgumentException();
                }
                return Convert.ToInt32(inSample);
            }
        }
        /*
            Random variates from the hypergeometric distribution.
            
            Returns the number of white balls drawn when kk balls
            are drawn at random from an urn containing nn1 white
            and nn2 black balls.
        */
        private static double RHyper(long kk, double nn1, double nn2, CoinFlipper coins)
        {
            var prng = new PRNG(coins);
            if (kk > 10)
            {
                return HypergeometricHRUA(prng, nn1, nn2, kk);
            }
            else
            {
                return HypergeometricHYP(prng, nn1, nn2, kk);
            }
        }
        private static double HypergeometricHYP(PRNG prng, double good, double bad, long sample)
        {
            var d1 = bad + good - sample;
            var d2 = Convert.ToDouble(Math.Min(bad, good));
            var Y = d2;
            var K = sample;
            while (Y > 0.0)
            {
                var U = prng.Draw();
                Y -= Math.Floor(U + Y / (d1 + K));
                K -= 1;
                if (K == 0)
                {
                    break;
                }
            }
            var Z = Math.Floor(d2 - Y);
            if (good > bad)
            {
                Z = sample - Z;
            }
            return Z;
        }
        private static double HypergeometricHRUA(PRNG prng, double good, double bad, long sample)
        {
            var D1 = 1.7155277699214135;
            var D2 = 0.8989161620588988;
            var minGoodBad = Math.Min(good, bad);
            var popSize = good + bad;
            var maxGoodBad = Math.Max(good, bad);
            var m = Math.Min(sample, popSize - sample);
            var d4 = minGoodBad / popSize;
            var d5 = 1.0 - d4;
            var d6 = m * d4 + 0.5;
            var d7 = Math.Sqrt((popSize - m) * sample * d4 * d5 / (popSize - 1) + 0.5);
            var d8 = D1 * d7 + D2;
            var d9 = Math.Floor(((double)m + 1) * (minGoodBad + 1) / (popSize + 2));
            var d10 = LogGamma(d9 + 1) + LogGamma(minGoodBad - d9 + 1) + LogGamma(m - d9 + 1) + LogGamma(maxGoodBad - m + d9 + 1);
            var d11 = Math.Min(Math.Min(m, minGoodBad) + 1.0, Math.Floor(d6 + 16 * d7));
            double Z;
            while (true)
            {
                var X = prng.Draw();
                var Y = prng.Draw();
                var W = d6 + d8 * (Y - 0.5) / X;
                if (W < 0.0 || W >= d11)
                {
                    continue;
                }
                Z = Math.Floor(W);
                var T = d10 - (LogGamma(Z + 1) + LogGamma(minGoodBad - Z + 1) + LogGamma(m - Z + 1) + LogGamma(maxGoodBad - m + Z + 1));
                if ((X * (4.0 - X) - 3.0) <= T)
                {
                    break;
                }
                if (X * (X - T) >= 1)
                {
                    continue;
                }
                if (2.0 * Math.Log(X) <= T)
                {
                    break;
                }
            }
            if (good > bad)
            {
                Z = m - Z;
            }
            if (m < sample)
            {
                Z = good - Z;
            }
            return Z;
        }
        /*
            log-gamma function to support some of these distributions. The
            algorithm comes from SPECFUN by Shanjie Zhang and Jianming Jin and their
            book "Computation of Special Functions", 1996, John Wiley & Sons, Inc.
            */
        private static double LogGamma(double x)
        {
            var a = new List<double>()
    {
        8.333333333333333e-02, -2.777777777777778e-03,
        7.936507936507937e-04, -5.952380952380952e-04,
        8.417508417508418e-04, -1.917526917526918e-03,
        6.410256410256410e-03, -2.955065359477124e-02,
        1.796443723688307e-01, -1.39243221690590e+00
    };
            x *= 1.0;
            double x0 = x;
            double n = 0;
            if (x == 1.0 || x == 2.0)
            {
                return 0;
            }
            if (x <= 7.0)
            {
                n = Math.Floor(7 - x);
                x0 = x + n;
            }
            var x2 = 1.0 / (x0 * x0);
            var xp = 2 * Math.PI;
            var gl0 = a[9];
            for (var k = 8; k >= 0; k--)
            {
                gl0 *= x2;
                gl0 += a[k];
            }
            var gl = gl0 / x0 + 0.5 * Math.Log(xp) + (x0 - 0.5) * Math.Log(x0) - x0;
            if (x <= 7.0)
            {
                for (var k = 1; k < n + 1; k++)
                {
                    gl -= Math.Log(x0 - 1.0);
                    x0 -= 1.0;
                }
            }
            return gl;
        }
    }
    public class PRNG
    {
        private CoinFlipper _coins;

        public PRNG(CoinFlipper coins)
        {
            _coins = coins;
        }
        public double Draw()
        {
            var idx = 0;
            BitArray bits = new BitArray(32);
            while (idx < 32)
            {
                var bit = _coins.GetCoin();
                bits.Set(idx, bit);
                idx += 1;
            }
            long o = 0;
            var sum = 0;
            foreach (bool bit in bits)
            {
                long bitVal = bit ? 1 : 0;
                sum += (int)bitVal;
                o = (o << 1) | bitVal;
            }
            var res = 1.0 * o / (Math.Pow(2, 32) - 1);
            if (res <= 0 || res > 1)
            {
                throw new ArgumentOutOfRangeException();
            }
            return res;
        }
    }
}
