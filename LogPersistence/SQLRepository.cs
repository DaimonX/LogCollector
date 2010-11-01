using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using LogCollector.Persistence.Model;
using System.Configuration;

namespace LogCollector.Persistence
{
    /*
     * CREATE TABLE `entries`.`entries` (
`oid` INT NOT NULL AUTO_INCREMENT PRIMARY KEY ,
`mdate` DATETIME NOT NULL ,
`message` VARCHAR( 400 ) NOT NULL
) ENGINE = MYISAM ;
     */
    public class SQLRepository : IDisposable,ISQLRepository
    {
       // string myConnectionString = "Database=entries;Data Source=localhost;User Id=root;";

        MySqlConnection db = null;

        public SQLRepository()
        {
            ConnectionStringSettings constr = ConfigurationManager.ConnectionStrings["mysql"];
            db = new MySqlConnection(constr.ConnectionString);
        }

        public void DelLogEntry()
        {

        }

        public void AddLogEntry(LogEntry entry)
        {
            string myInsertQuery = "INSERT INTO entries (mdate, message) Values(@mdate, @msg)";


            MySqlParameter mdate = new MySqlParameter();
            mdate.MySqlDbType = MySqlDbType.DateTime;
            mdate.ParameterName = "@mdate";
            mdate.Value = entry.Date;

            MySqlParameter msg = new MySqlParameter();
            msg.MySqlDbType = MySqlDbType.VarChar;
            msg.ParameterName = "@msg";
            msg.Value = entry.Message;

            MySqlCommand myCommand = new MySqlCommand(myInsertQuery, db);
            myCommand.Parameters.Add(mdate);
            myCommand.Parameters.Add(msg);

           myCommand.Connection.Open();
           myCommand.ExecuteNonQuery();
           myCommand.Connection.Close();

        }

        public long GetEntryCount()
        {
            string sql = "SELECT COUNT(*) FROM entries";

            MySqlCommand cmd = new MySqlCommand(sql, db);
            cmd.Connection.Open();
            object result = cmd.ExecuteScalar();
            cmd.Connection.Close();
            long r = 0;
            if (result != null)
            {
                r = Convert.ToInt64(result);
            }
            return r;
        }

        public IList<LogEntry> GetEntriesPage(int limit, int offset)
        {
            string sql = "select oid, mdate, message from entries limit @lim offset @off";

            MySqlParameter lim = new MySqlParameter();
            lim.MySqlDbType = MySqlDbType.Int32;
            lim.ParameterName = "@lim";
            lim.Value = limit;

            MySqlParameter off = new MySqlParameter();
            off.MySqlDbType = MySqlDbType.Int32;
            off.ParameterName = "@off";
            off.Value = offset;

            MySqlCommand myCommand = new MySqlCommand(sql, db);
            myCommand.Parameters.Add(lim);
            myCommand.Parameters.Add(off);

            myCommand.Connection.Open();
            MySqlDataReader rdr = myCommand.ExecuteReader();

            IList<LogEntry> entries = new List<LogEntry>();
          
            while (rdr.Read())
            {
                entries.Add(new LogEntry {_id = rdr[0].ToString() , Date = (DateTime)rdr[1], Message = rdr[2].ToString() });
            }

            myCommand.Connection.Close();
            return entries;
        }





        #region Члены IDisposable

        public void Dispose()
        {
               db.Close();
                db.Dispose();

        }

        #endregion
    }
}
