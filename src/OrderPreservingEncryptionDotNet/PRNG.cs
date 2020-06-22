using System;
using System.Collections;

namespace OrderPreservingEncryptionDotNet
{
    internal class PRNG
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
