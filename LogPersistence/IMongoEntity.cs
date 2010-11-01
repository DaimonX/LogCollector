using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;

namespace LogCollector.Persistence
{
    public interface IMongoEntity
    {
        string _id { get; set; }
        Oid GetOid();

        Document GetAsDocument();
        void UpdateFromDocument(Document document);
    }

}
