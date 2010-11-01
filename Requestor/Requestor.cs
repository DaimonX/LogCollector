using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogCollector.Persistence;
using LogCollector.Persistence.Model;

namespace Requestor
{
    class Requestor
    {
        static void Main(string[] args)
        {
            ISQLRepository repos = new LogRepository();
            Console.WriteLine("Enter to proceeed");
            Console.ReadLine();
            
            //((LogRepository)repos).GetAllLogEntries();

           // var dt = DateTime.UtcNow;
            //IList<LogEntry> entries = repos.GetEntriesInRange(new DateTime(2010, 9, 25, 11, 02, 00), new DateTime(2010, 9, 25, 11, 05, 00));
 
           // var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            Console.WriteLine(String.Format("Found {0} entries", repos.GetEntryCount()));
            Console.ReadLine();
            var dt = DateTime.UtcNow;
            repos.DelLogEntry();
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
           // var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            Console.WriteLine(String.Format("Found {0} entries and it takes {1} milliseconds", repos.GetEntryCount(), ts.TotalMilliseconds));

                       
            Console.ReadLine();

            Test();
        }

        private static void Test()
        {
            Console.WriteLine();
        }
    }
}
