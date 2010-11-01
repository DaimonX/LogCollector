using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogCollector.Persistence.Model;

namespace LogCollector.Persistence
{
    public interface ISQLRepository
    {
        /// <summary>
        /// Delete all entries. No params needed
        /// </summary>
        void DelLogEntry();
        
        //void DelLogEntry(string query);
        
        /// <summary>
        /// Добавляет лог в базу.
        /// </summary>
        /// <param name="entry">Запись из лога</param>
        void AddLogEntry(LogEntry entry);
        
        long GetEntryCount();
        IList<LogEntry> GetEntriesPage(int limit, int offset);
    }
}
