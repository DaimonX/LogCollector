using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogCollector.Persistence;
using LogCollector.Persistence.Model;
using System.Threading;
using System.IO;

namespace Mongo2Mysql
{
    class Program
    {
        public static LogRepository to = new LogRepository();
        public static SQLRepository from = new SQLRepository();
        public static int buffsize = 50;

        static void Main(string[] args)
        {
            long total = from.GetEntryCount();
            int pages = (int)total / buffsize;
            var dt = DateTime.UtcNow;
            var totaldl = DateTime.UtcNow;
            int reccount = 0;

            long ticksOnRead = 0;
            long ticksOnWrite = 0;

            int page = 0;
            long recordsDone = to.GetEntryCount(); //in case if we go on writing not from beginning;
            if (recordsDone > 0)
            {
                page = (int) recordsDone / buffsize;
            }
            Console.Clear();
            for (int i = page; i < pages; i++) //18539 should start frrom 30 %
            {
                IList<LogEntry> entries = from.GetEntriesPage(buffsize, i * buffsize);
                var tr = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
                ticksOnRead += tr.Ticks;
                
                var dw = DateTime.UtcNow;
                
                foreach (LogEntry e in entries)
                {
                    to.AddLogEntry(e);
                    Interlocked.Increment(ref reccount);
                }
                var tw = new TimeSpan(DateTime.UtcNow.Ticks - dw.Ticks);
                ticksOnWrite += tw.Ticks;

                var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
                var tt = new TimeSpan(DateTime.UtcNow.Ticks - totaldl.Ticks);
                if ((reccount % 1000) == 0)
                {
                    string str = String.Format("1000 records per {0} seconds", ts.TotalSeconds);
                    int estimatedTime = (int)(ts.TotalSeconds * ((total - i * buffsize)/ 1000));
                    double percentage = ((double)(i * buffsize * 100 / total));
                    string totalstr = String.Format("Total time {0} seconds. Total records {1}.\n"
                                                     +"Records done {2} ({3:0.00}%). Estimated time left {4} sec\n" 
                                                     +"Time spent on read {5} s. Time spent on write {6} s",
                        (int)tt.TotalSeconds, total, i * buffsize,
                         percentage, estimatedTime,
                         new TimeSpan(tr.Ticks).TotalSeconds, new TimeSpan(tw.Ticks).TotalSeconds
                        );

                    if (percentage % 5 == 0)
                    {
                        using (FileStream fs = new FileStream("log.txt", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                sw.WriteLine(String.Format("START============================================={0}=============================================", percentage));
                                sw.WriteLine(str);
                                sw.WriteLine(totalstr);
                                sw.WriteLine(String.Format("END==============================================={0}===========================================", percentage));
                            }
                        }
                    }
                    Console.SetCursorPosition(0, 1);
                    Console.Write(str);
                    Console.SetCursorPosition(0, 2);
                    Console.Write(totalstr);
                    dt = DateTime.UtcNow;
                    ticksOnRead = 0;
                    ticksOnWrite = 0;
                }
            }
        }
    }
}
