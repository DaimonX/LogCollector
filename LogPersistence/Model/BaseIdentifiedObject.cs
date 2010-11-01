using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;

namespace LogCollector.Persistence.Model
{
    public abstract class BaseIdentifiedObject : IMongoEntity
    {
        public virtual string _id { get; set; }

        #region IMongoEntity Members

        public virtual Oid GetOid()
        {
            return new Oid(_id);
        }

        public abstract Document GetAsDocument();

        public abstract void UpdateFromDocument(Document document);

        #endregion

        public override string ToString()
        {
            return "_id: " + _id;
        }
    }
}
