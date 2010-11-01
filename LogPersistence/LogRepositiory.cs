using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using LogCollector.Persistence.Model;

namespace LogCollector.Persistence
{
    public class LogRepository : RepositoryBase, ISQLRepository
    {
        IMongoDatabase db = null;
        IMongoCollection logs = null;
        MongoJson json = null;

        public LogRepository()
        {
            db = base.Init().MDB.GetDatabase("logstore");
            json = base.Init().json;
            logs = db.GetCollection("entries");
        }

        public long GetEntryCount()
        {
            return logs.Count();
        }

        public IList<LogEntry> GetEntriesPage(int limit, int offset)
        {
            var cursor = logs.FindAll().Limit(limit).Skip(offset);
            List<LogEntry> entries = new List<LogEntry>();
            foreach (var d in cursor.Documents)
                entries.Add(json.ObjectFrom<LogEntry>(d));

            return entries;
        }

        public void DelLogEntry()
        {
            logs.Remove(new Document { });
        }

        
        
        public void DelLogEntry(string query)
        {
            logs.Remove(new Document {{ "asdf", query }});
        }

        
        
        
        public void AddLogEntry(LogEntry entry)
        {
            Document entryDOC = json.DocumentFrom(entry);
            logs.Insert(entryDOC); 
        }

        public LogEntry GetLogEntry(int timezone)
        {
            var document = logs.FindOne(new Document { { "Timezone", timezone } });
            return json.ObjectFrom<LogEntry>(document); 
        }

        public IList<LogEntry> GetAllLogEntries()
        {
            var cursor = logs.FindAll();
            List<LogEntry> entries = new List<LogEntry>();
            foreach (var d in cursor.Documents)
                entries.Add(json.ObjectFrom<LogEntry>(d));

            return entries;
        }

        public IList<LogEntry> GetEntriesInRange(DateTime start, DateTime end)
        {
            Console.WriteLine("Start: " + start.ToShortDateString() + " " + start.ToShortTimeString());
            Console.WriteLine("End: " + end.ToShortDateString() + " " + end.ToShortTimeString());
            var cursor = logs.Find(new Document { { "Date",
                                                     new Document { { "$gte", start }, { "$lt", end } } 
                                                   } }).Options(QueryOptions.NoCursorTimeout);
            
            List<LogEntry> entries = new List<LogEntry>();
            foreach (var d in cursor.Documents)
                entries.Add(json.ObjectFrom<LogEntry>(d));

            return entries;
        }

    }
}
