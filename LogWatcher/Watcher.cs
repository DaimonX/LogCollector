using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Configuration;
using LogCollector.Persistence;
using LogCollector.Persistence.Model;

namespace LogWatcher
{
    public partial class Watcher : ServiceBase
    {
        public Watcher()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            FileWatchInfo info = new FileWatchInfo(ConfigurationManager.AppSettings["pattern"], ConfigurationManager.AppSettings["folder"]);
            DataAvaliableEvent += new DataAvaliableHandler(OnDataAvaliable);
            Thread readWorker = new Thread(ReadFileThread);
            readWorker.Start(info);
        }

        protected override void OnStop()
        {
            shouldStop = true;
        }

        #region private

        private bool shouldStop = false;
        public delegate void DataAvaliableHandler(object sender, EventArgs e);
        public event DataAvaliableHandler DataAvaliableEvent;
        private string strBuff;
        private LogRepository model = new LogRepository();

        private class FileWatchInfo
        {
            public string Pattern { get; private set; }
            public string Folder { get; private set; }


            public FileWatchInfo(string pattern, string folder)
            {
                this.Pattern = pattern;
                this.Folder = folder;
            }
        }

        protected void OnDataAvaliable(object sender, EventArgs e)
        {
            if (DataAvaliableEvent != null)
            {
                DataAvaliableEvent(sender, e);
            }
        }

        private string FindFile(FileWatchInfo info)
        {
            string[] files = Directory.GetFiles(info.Folder, info.Pattern);
            if (files.Length == 0) { throw new FileNotFoundException(); }

            IComparer comp = new FileComparer(FileComparer.CompareBy.LastWriteTime);
            Array.Sort(files, comp);
            return files[files.Length - 1];
        }

        private void ReadFileThread(object info)
        {
            string chunk = String.Empty;
            Regex regexObj = new Regex(@"\d{4,4}-\d{2,2}-\d{2,2}");


            while (!shouldStop)
            {
                string filename = FindFile((FileWatchInfo)info);
                bool isFileChanged = false;
                
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
                                    strBuff = chunk;
                                    OnNewDataAvaliable(null, new EventArgs());
                                    if (line1 != null)
                                        chunk = line1;
                                    else chunk = String.Empty;
                                }
                            }

                            Thread.Sleep(5);
                            if (FindFile((FileWatchInfo)info) != filename)
                                isFileChanged = true;

                            if (shouldStop)
                                return;
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


        private void OnNewDataAvaliable(object sender, EventArgs a)
        {
            lock (strBuff)
            {
                IList<LogEntry> entries = Parse(strBuff);
                strBuff = String.Empty;

                foreach (LogEntry entry in entries)
                    model.AddLogEntry(entry);

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
                    try
                    {
                        date = DateTime.Parse(matchObj.Groups["date"].ToString() + " " + matchObj.Groups["time"].ToString());
                    }
                    catch (FormatException fe)
                    {
                        date = DateTime.Parse(matchObj.Groups["date"].ToString());
                    }
                    offset = 2;
                    message = matchObj.Groups["message"].ToString();
                    entries.Add(new LogEntry { Date = date, Timezone = offset, Message = message });
                }
            }
            return entries;
        }

        #endregion
    }
}
