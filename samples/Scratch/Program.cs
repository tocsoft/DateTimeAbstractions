using System;
using System.Diagnostics;
using System.Threading;
using Tocsoft.DateTimeAbstractions;

namespace Scratch
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Real time");
            for (var i = 0; i < 10; i++)
            {
                logTime();
            }
            Console.WriteLine();
            Console.WriteLine("Pinned");
            using (ClockTimer.Pin(TimeSpan.FromMilliseconds(5)))
            using (Clock.Pin(new DateTime(2000, 01, 01)))
            {
                for (var i = 0; i < 10; i++)
                {
                    logTime();
                }
            }
            Console.WriteLine();

            Console.WriteLine("Real time");
            for (var i = 0; i < 10; i++)
            {
                logTime();
            }
            Console.WriteLine();

            Console.ReadLine();
        }

        private static void logTime()
        {
            var w = ClockTimer.StartNew();
            Console.Write(Clock.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            Thread.Sleep(100);
            w.Stop();
            Console.WriteLine($" - in {w.Elapsed}");
        }
    }
}
