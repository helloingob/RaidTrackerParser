using System;
using System.Data.SQLite;
using System.Diagnostics;

namespace RaidTrackerParser
{
    class Program
    {

        
        static void Main(string[] args)
        {
            //var inputFile = @"E:\RaidTrackerLog\Log1_Grouptest.txt";
            var inputFile = @"E:\RaidTrackerLog\Log3_MC_Pre.txt";
            //var inputFile = @"E:\RaidTrackerLog\Log2_Ony.txt";

            var watchTotal = Stopwatch.StartNew();

            var sqlHandler = new SQLHandler();

            Console.Write("Generate sqlite database... ");
            //sqlHandler.GenerateDatabase();
            Console.WriteLine("Finished.");

            var raidTrackerParser = new RaidTrackerParser();
            Console.Write("Parsing log... ");
            var raid = raidTrackerParser.ParseLog(inputFile);
            Console.WriteLine("Finished.");
            
            Console.WriteLine("Persisting raid... ");
            sqlHandler.Persist(raid);

            watchTotal.Stop();

            var timeSpan = TimeSpan.FromMilliseconds(watchTotal.ElapsedMilliseconds);
            Console.WriteLine("-------------------------------------------------------------------------------------");
            Console.WriteLine("Total duration " + timeSpan.ToString(@"hh\:mm\:ss\.fff"));
            Console.WriteLine("=====================================================================================");

            Console.WriteLine("Press any key to continue ...");
            //Console.ReadKey();
        }
    }
}