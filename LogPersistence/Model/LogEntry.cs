using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogCollector.Persistence.Model
{
    [Serializable]
    public class LogEntry: BaseIdentifiedObject
    {
        public virtual DateTime Date { get; set; }
        public virtual string Message { get; set; }
        public virtual int Timezone { get; set; }

        public override MongoDB.Document GetAsDocument()
        {
            throw new NotImplementedException();
        }

        public override void UpdateFromDocument(MongoDB.Document document)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder str = new StringBuilder(180);
            str.Append(base.ToString());
            str.Append(" Date: " + Date.ToShortDateString());
            str.Append(" " + Date.ToShortTimeString());
            str.Append(" Message: " + Message);
            str.Append(" Timezone: ");
            str.Append(Timezone);
            return str.ToString();
        }
    }
}
