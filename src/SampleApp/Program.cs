using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OrderPreservingEncryptionDotNet;
using System.Diagnostics;
namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var key = OPE.CreateKey(32);
            var ope = new OPE(key);
            var a = 1000;
            var encA = ope.Encrypt(a);
            var decA = ope.Decrypt(encA);
        }
    }
}