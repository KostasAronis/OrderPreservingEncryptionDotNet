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
            var key = OPE.CreateKey(256);
            var kkey = Encoding.ASCII.GetBytes("2OXUVsWhyfpTIE9BWiZ7fKlC/2baLoFhagkXNTVjCf0=");
            var ope = new OPE(kkey);
            List<long> encryptedNumbers = new List<long>();
            List<long> crashingNumbers = new List<long>();
            List<long> times = new List<long>();
            Stopwatch sw = new Stopwatch();
            for (var i = 1; i < 20000; i++)
            {
                try
                {
                    sw.Start();
                    long encrypted = ope.Encrypt(i);
                    encryptedNumbers.Add(encrypted);
                    Console.WriteLine($"{i} -> {encrypted}");
                    sw.Stop();
                    var elapsed = sw.ElapsedMilliseconds;
                    times.Add(elapsed);
                    sw.Reset();
                }
                catch (Exception ex)
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