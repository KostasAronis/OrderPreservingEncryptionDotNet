using System;
using System.Diagnostics;
using System.Security.Cryptography;

namespace OrderPreservingEncryptionDotNet
{
    public class OPE
    {
        private const int DEFAULT_IN_RANGE_START = 0;
        private const int DEFAULT_IN_RANGE_END = 32767;
        private const int DEFAULT_OUT_RANGE_START = 0;
        private const int DEFAULT_OUT_RANGE_END = 2147483647;
        private readonly byte[] _privateKey;
        private readonly ValueRange _inRange;
        private readonly ValueRange _outRange;
        private int c;

        public OPE(byte[] privateKey)
        {
            c = 0;
            _privateKey = privateKey;
            _inRange = new ValueRange(DEFAULT_IN_RANGE_START, DEFAULT_IN_RANGE_END);
            _outRange = new ValueRange(DEFAULT_OUT_RANGE_START, DEFAULT_OUT_RANGE_END);
        }
        public OPE(byte[] privateKey, ValueRange inRange, ValueRange outRange)
        {
            _privateKey = privateKey;
            _inRange = inRange;
            _outRange = outRange;
            
        }
        public long Encrypt(int value)
        {
            if (!_inRange.Contains(value))
            {
                throw new ArgumentOutOfRangeException("Value must be within Inner range");
            }
            return EncryptRecursive(value, _inRange, _outRange);
        }
        public long Decrypt(long value)
        {
            if (!_outRange.Contains(value))
            {
                throw new ArgumentOutOfRangeException("Encrypted value must be within Outter range");
            }
            return DecryptRecursive(value, _inRange, _outRange);
        }



        private long EncryptRecursive(long value, ValueRange inRange, ValueRange outRange)
        {
            var inSize = inRange.Size();
            var outSize = outRange.Size();
            var inEdge = inRange.Start - 1;
            var outEdge = outRange.Start - 1;
            var mid = outEdge + Convert.ToInt32(Math.Ceiling(outSize / 2.0));
            if (inSize > outSize)
            {
                throw new Exception();
            }
            CoinFlipper coins;
            if (inSize == 1){
                coins = new CoinFlipper(this._privateKey, value);
                long cipherText = Stat.SampleUniform(outRange, coins);
                return cipherText;
            }
            coins = new CoinFlipper(this._privateKey, mid);
            
            var x = Stat.SampleHGD(inRange, outRange, mid, coins);
            ValueRange newInRange;
            ValueRange newOutRange;
            if(value <= x)
            {
                newInRange = new ValueRange(inEdge + 1, x);
                newOutRange = new ValueRange(outEdge + 1, mid);
            }
            else
            {
                newInRange = new ValueRange(x + 1, Convert.ToInt64(inEdge) + inSize);
                newOutRange = new ValueRange(mid + 1, outEdge + outSize);
            }
            return EncryptRecursive(value, newInRange, newOutRange);
        }


        private long DecryptRecursive(long value, ValueRange inRange, ValueRange outRange)
        {
            var inSize = inRange.Size();
            var outSize = outRange.Size();
            var inEdge = inRange.Start - 1;
            var outEdge = outRange.Start - 1;
            long inRangeMin;
            var mid = outEdge + Convert.ToInt32(Math.Ceiling(outSize / 2.0));
            if (inSize > outSize)
            {
                throw new Exception();
            }
            CoinFlipper coins;
            if (inSize == 1)
            {
                inRangeMin = inRange.Start;
                coins = new CoinFlipper(this._privateKey, inRangeMin);
                long cipherText = Stat.SampleUniform(outRange, coins);
                if(cipherText == value)
                {
                    return inRangeMin;
                }
                else
                {
                    throw new Exception("Invalid ciphertext!");
                }
            }
            coins = new CoinFlipper(this._privateKey, mid);
            var x = Stat.SampleHGD(inRange, outRange, mid, coins);
            ValueRange newInRange;
            ValueRange newOutRange;
            if (value <= mid)
            {
                newInRange = new ValueRange(inEdge + 1, x);
                newOutRange = new ValueRange(outEdge + 1, mid);
            }
            else
            {
                newInRange = new ValueRange(x + 1, Convert.ToInt64(inEdge) + inSize);
                newOutRange = new ValueRange(mid + 1, outEdge + outSize);
            }
            return DecryptRecursive(value, newInRange, newOutRange);
        }
        public static byte[] CreateKey(int keyBytes = 32)
        {
            byte[] key = new byte[keyBytes];
            using (RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider())
            {
                rnd.GetBytes(key);
                return key;
            }
        }

        public static string CreateKeyString(int keyBytes = 32)
        {
            string key = "";
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            using (RNGCryptoServiceProvider rnd = new RNGCryptoServiceProvider())
            {
                while (key.Length != keyBytes)
                {
                    byte[] oneByte = new byte[1];
                    rnd.GetBytes(oneByte);
                    char character = (char)oneByte[0];
                    if (valid.Contains(character))
                    {
                        key += character;
                    }
                }
                return key;
            }
        }
    }
}
