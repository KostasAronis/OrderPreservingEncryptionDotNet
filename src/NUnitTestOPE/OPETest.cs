using NUnit.Framework;
using OrderPreservingEncryptionDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace NUnitTestOPE
{
    public class OPETests
    {
        private byte[] key;
        private OPE ope;
        private Random rng;

        [SetUp]
        public void Setup()
        {
            key = OPE.CreateKey(32);
            ope = new OPE(key);
            rng = new Random();
        }

        [Test]
        public void AllNumbersTest()
        {
            List<long> encryptedNumbers = new List<long>();
            for (var i = 1; i < ope.InRange.End; i++)
            {
                try
                {
                    long encrypted = ope.Encrypt(i);
                    encryptedNumbers.Add(encrypted);
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            List<long> expectedNumbers = encryptedNumbers.OrderBy(d => d).ToList();
            Assert.IsTrue(expectedNumbers.SequenceEqual(encryptedNumbers));
        }

        [Test]
        public void RandomEncryptDecryptTest()
        {
            var a = rng.Next(0, 20000);
            var encA = ope.Encrypt(a);
            var decA = ope.Decrypt(encA);
            Assert.AreEqual(a, decA);
        }

        [Test]
        public void RandomSequentialTest()
        {
            var a = rng.Next(0, 20000);
            var b = a + 1;
            var encA = ope.Encrypt(a);
            var encB = ope.Encrypt(b);
            Assert.Greater(encB, encA);
        }
    }
}