using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

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
            IEnumerator<bool> coins;
            if (inSize == 1){
                coins = GetCoins(this._privateKey, value);
                long cipherText = Stat.SampleUniform(outRange, coins);
                return cipherText;
            }
            coins = GetCoins(this._privateKey, mid);
            //c++;
            //Debug.WriteLine(c);
            
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

        public static IEnumerable<bool> TapeGen(byte[] privateKey, long value)
        {
            var hmac = new HMACSHA256(privateKey);
            hmac.Initialize();
            byte[] buffer = Encoding.ASCII.GetBytes(value.ToString());
            var hashedKey = hmac.ComputeHash(buffer);
            var aesCipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            aesCipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", hashedKey), new byte[16]));
            byte[] aesEncryptedValue = aesCipher.DoFinal(new byte[16]);
            BitArray coins = new BitArray(aesEncryptedValue);
            coins = MostSignificantBitFirst(coins);
            for (var i = 0; i < coins.Length; i++)
            {
                //Console.WriteLine(i + ": "+ coins[i]);
                yield return coins[i];
            }
        }
        public static IEnumerator<bool> GetCoins(byte[] privateKey, long value)
        {
            return TapeGen(privateKey, value).GetEnumerator();
        }
        public static BitArray MostSignificantBitFirst(BitArray b)
        {
            BitArray bb = new BitArray(b.Length);
            var x = -1;
            for (var i = 0; i< b.Count; i++)
            {
                if (i % 8 == 0)
                {
                    x += 8;
                }
                var j = x - (i % 8);
                bb.Set(j, b.Get(i));
            }
            return bb;
        }

        public int Decrypt(int value)
        {
            return 1;
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
