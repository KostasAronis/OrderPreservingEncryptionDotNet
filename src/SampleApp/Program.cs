using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using OrderPreservingEncryptionDotNet;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var key = OPE.CreateKey(256);
            var kkey = Encoding.ASCII.GetBytes("2OXUVsWhyfpTIE9BWiZ7fKlC/2baLoFhagkXNTVjCf0=");
            var ope = new OPE(kkey);
            List<long> encryptedNumbers = new List<long>();
            List<long> crashingNumbers = new List<long>();
            for (var i = 1; i < 20000; i++)
            {
                try
                {
                    long encrypted = ope.Encrypt(i);
                    encryptedNumbers.Add(encrypted);
                    Console.WriteLine($"{i} -> {encrypted}");
                }
                catch(Exception ex)
                {
                    crashingNumbers.Add(i);
                    Console.WriteLine($" CRASHING !!! :=> {i}");
                }
            }
            Console.WriteLine(crashingNumbers);
        }
    }
}
// 1 0 1 0 1 1 1 0
//63316149