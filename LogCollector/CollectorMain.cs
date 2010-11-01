using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogCollector.Persistence;
using LogCollector.Persistence.Model;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading;
using System.Collections;
using System.Configuration;

namespace LogCollector
{
    class CollectorMain
    {
        static void Main(string[] args)
        {
            FileWatchInfo info = new FileWatchInfo(ConfigurationManager.AppSettings["pattern"],ConfigurationManager.AppSettings["folder"]);
            model = ConfigureDB();
            model.DelLogEntry();
        
            DataAvaliableEvent += new DataAvaliableHandler(OnDataAvaliable);
            Thread readWorker = new Thread(CollectorMain.ReadFileThread);
            readWorker.Start(info);
        }

        public delegate void DataAvaliableHandler(object sender, EventArgs e);
        public static event DataAvaliableHandler DataAvaliableEvent;
        public static string strBuff;
        private static ISQLRepository model = null;

        private static ISQLRepository ConfigureDB()
        {
            string dbmodel = ConfigurationManager.AppSettings["dbmodel"];
            ISQLRepository model = null;
            switch (dbmodel) {
                case "mongo": model = new LogRepository(); break;
                case "mysql": model = new SQLRepository(); break;
                case "mssql": model = new MSSQLRepository(); break;
                default: model = new LogRepository(); break;
            }
            return model;
        }

        protected static void OnDataAvaliable(object sender, EventArgs e)
        {
            if (DataAvaliableEvent != null) {
                DataAvaliableEvent(sender, e);
            }
        }

        private class FileWatchInfo
        {
            public string Pattern { get; private set; }
            public string Folder {get; private set;}
            

            public FileWatchInfo(string pattern, string folder)
            {
                this.Pattern = pattern;
                this.Folder = folder;
            }
        }

        private static string FindFile(FileWatchInfo info)
        {
            string[] files = Directory.GetFiles(info.Folder, info.Pattern);
            if (files.Length == 0) { throw new FileNotFoundException(); }

            IComparer comp = new FileComparer(FileComparer.CompareBy.LastWriteTime);
            Array.Sort(files, comp);
            return files[files.Length - 1];
        }

        private static void ReadFileThread(object info)
        {
            string chunk = String.Empty;
            Regex regexObj = new Regex(@"\d{4,4}-\d{2,2}-\d{2,2}");

            while (true) //(!!!!)
            {
                string filename = FindFile((FileWatchInfo)info);
                bool isFileChanged = false;
                long count = 0;
                var dt = DateTime.UtcNow;
                var totaldl = DateTime.UtcNow;
                Console.Clear();
                //prevent file locking
                try
                {
                    using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        while (!isFileChanged)
                        {

                            string line1 = String.Empty;
                            string line2 = String.Empty;

                            line1 = sr.ReadLine();

                            if (line1 != null && ((chunk != String.Empty && !regexObj.IsMatch(line1)) || chunk == string.Empty))
                            {
                                chunk += line1;
                            }
                            else
                            {
                                if (chunk != String.Empty)
                                {
                                    Interlocked.Increment(ref count);
                                    if ((count % 1000) == 0)
                                    {
                                        var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
                                        var tt = new TimeSpan(DateTime.UtcNow.Ticks - totaldl.Ticks);

                                        string str = String.Format("1000 records per {0} seconds", ts.TotalSeconds);

                                        string total = String.Format("Total time {0} seconds. Total records {1}", tt.TotalSeconds, count);
                                        Console.SetCursorPosition(1, 1);
                                        Console.Write(str);
                                        Console.SetCursorPosition(1, 2);
                                        Console.Write(total);
                                        dt = DateTime.UtcNow;
                                    }

                                    strBuff = chunk;
                                    OnNewDataAvaliable(null, new EventArgs());
                                    if (line1 != null)
                                        chunk = line1;
                                    else chunk = String.Empty;
                                }
                            }

                            Thread.Sleep(1);
                            if (FindFile((FileWatchInfo)info) != filename)
                                isFileChanged = true;
                        }
                    }
                }
                catch (IOException e)
                {
                    Thread.Sleep(500);
                    continue;
                }
            }
        }
     
       static void OnNewDataAvaliable(object sender, EventArgs a)
        {
            lock(strBuff){
                IList<LogEntry> entries = Parse(strBuff);
                strBuff = String.Empty;

                foreach (LogEntry entry in entries) {
                    model.AddLogEntry(entry);
//                     Console.WriteLine("Adding " + entry);
                     
                }
            }
         }

        static IList<LogEntry> Parse(string logstr)
        {
            Match matchObj;
            Regex regexObj = new Regex(@"(?<date>\d{1,4}-\d{1,2}-\d{1,2})T(?<time>.*?)\+(?<offset>.*?)\s(?<message>.*?)(?=\d{4}-\d{2}-\d{2}|$)", RegexOptions.Singleline);
            MatchCollection col = regexObj.Matches(logstr);

            IList<LogEntry> entries = new List<LogEntry>();

            foreach (Match m in col)
            {
                matchObj = m;
                DateTime date;
                int offset;
                string message;
                if (matchObj.Success)
                {
                    try {
                        date = DateTime.Parse(matchObj.Groups["date"].ToString() + " " + matchObj.Groups["time"].ToString());
                    }
                    catch (FormatException fe) {
                        date = DateTime.Parse(matchObj.Groups["date"].ToString());
                    }
                    offset = 2;
                    message = matchObj.Groups["message"].ToString();
                    entries.Add(new LogEntry { Date = date, Timezone = offset, Message = message });
                }
            }
            return entries;
        }
    }
}
