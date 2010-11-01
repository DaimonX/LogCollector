using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB;
using Newtonsoft.Json;
using System.Configuration;

namespace LogCollector.Persistence
{

    public class RepositoryBase : IDisposable
    {
        private static DBState state = null;

        public virtual DBState Init()
        {
            if (RepositoryBase.state == null)
            {
                ConnectionStringSettings constr = ConfigurationManager.ConnectionStrings["mongo"];
                
                state = new DBState();
                state.MDB = new Mongo(constr.ConnectionString);
                state.MDB.Connect();
                state.json = new MongoJson();
            }

            return RepositoryBase.state;
        }

        protected RepositoryBase()   { }


        #region IDisposable Members

        public void Dispose()
        {
            state.MDB.Disconnect();
        }

        #endregion
    }

    public class DBState
    {
        public Mongo MDB { get; set; }
        public MongoJson json { get; set; }
    }
 
}
