using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace OrderPreservingEncryptionDotNet
{
    public class CoinFlipper
    {
        private byte[] _privateKey;
        private long _value;
        private IEnumerator<bool> _coins;
        private int i = 0;

        public CoinFlipper(byte[] privateKey, long value)
        {
            _privateKey = privateKey;
            _value = value;
            _coins = GetCoins(_privateKey, _value);
        }

        public bool GetCoin()
        {
            var hasNext = _coins.MoveNext();
            i++;
            if (hasNext)
            {
                var val = _coins.Current;
                return val;
            } else
            {
                _coins = GetCoins(_privateKey, _value);
                return GetCoin();
            }
        }

        public static IEnumerator<bool> GetCoins(byte[] privateKey, long value)
        {
            var hmac = new HMACSHA256(privateKey);
            hmac.Initialize();
            byte[] buffer = Encoding.ASCII.GetBytes(value.ToString());
            var hashedKey = hmac.ComputeHash(buffer);
            var aesCipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            aesCipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", hashedKey), new byte[16]));
            while (true)
            {
                //byte[] aesEncryptedValue = aesCipher.DoFinal(new byte[16]);
                byte[] aesEncryptedValue = aesCipher.ProcessBytes(new byte[16]);
                BitArray coins = new BitArray(aesEncryptedValue);
                coins = MostSignificantBitFirst(coins);
                for (var i = 0;i < coins.Count; i++)
                {
                    yield return coins[i];
                }
            }
        }
        public static BitArray MostSignificantBitFirst(BitArray b)
        {
            BitArray bb = new BitArray(b.Length);
            var x = -1;
            for (var i = 0; i < b.Count; i++)
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

    }
}
