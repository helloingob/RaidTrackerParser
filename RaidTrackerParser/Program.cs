using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RaidTrackerParser
{
    class Program
    {
        //PRAGMA foreign_keys = ON;
        private static void Main()
        {
            var watchTotal = Stopwatch.StartNew();

            var sqlHandler = new SQLHandler();
            
            //Console.Write("Generate sqlite database... ");
            //sqlHandler.GenerateDatabase();
            //Console.WriteLine("Finished.");

            foreach (var fileName in Directory.GetFiles(@"E:\RaidTrackerLog"))
            {
                var raidTrackerParser = new RaidTrackerParser();
                Console.WriteLine("Parsing log from '" + Path.GetFileName(fileName) + "'");
                var raid = raidTrackerParser.ParseLog(fileName);
                Console.WriteLine("Finished.");

                Console.WriteLine("Persisting raid... ");
                sqlHandler.Persist(raid);
                Console.WriteLine("Finished.");
                Console.WriteLine("*************************************************************************************");
            }

            Console.WriteLine("Updating item quality ... ");
            //sqlHandler.UpdateItemQuality();
            Console.WriteLine("Finished.");

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