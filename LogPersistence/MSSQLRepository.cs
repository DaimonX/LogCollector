using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LogCollector.Persistence.Model;
using System.Data.SqlClient;
using System.Configuration;





namespace LogCollector.Persistence
{
    public class MSSQLRepository : IDisposable, ISQLRepository
    {
        SqlConnection db = null;

        public MSSQLRepository()
        {
            ConnectionStringSettings constr = ConfigurationManager.ConnectionStrings["mssql"];
            SqlConnection db = new SqlConnection();

            //string constr = "Data Source=.\\SQLEXPRESS; AttachDbFilename=D:\\C#\\Andrey\\LogCollector\\LogCollector\\LogPersistence\\msMongo.mdf;Integrated Security=True;User Instance=True;";

            //SqlConnection db = new SqlConnection();
            //SqlConnectionStringBuilder constr = new SqlConnectionStringBuilder();
            //constr.DataSource = "DAIMONX";
            //constr.InitialCatalog = "MSMONGO";
            //constr.UserID = "sa";
            //constr.Password = "123456";

            db.ConnectionString = constr.ToString();

            //try
            //{
            //    db.Open();
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.ToString());
            //}

            //SqlCommand sclCmd = new SqlCommand("INSERT INTO ENTRIES (mdate,message) VALUES ('dsfdf','sdfds')", db);
            //sclCmd.ExecuteNonQuery();
           //db.Close();

        }

        public void DelLogEntry()
        {
            ConnectionStringSettings constr = ConfigurationManager.ConnectionStrings["mssql"];
            SqlConnection db = new SqlConnection();
            db.ConnectionString = constr.ToString();
            SqlCommand myCommand = new SqlCommand();
            myCommand.CommandText = "DELETE FROM ENTRIES";
            myCommand.Connection = db;
            
            db.Open();
            myCommand.ExecuteNonQuery();
            db.Close();

        }

        public void AddLogEntry(LogEntry entry)
        {
            ConnectionStringSettings constr = ConfigurationManager.ConnectionStrings["mssql"];
            SqlConnection db = new SqlConnection();
            //constr.DataSource = "DAIMONX";
            //constr.InitialCatalog = "MSMONGO";
            //constr.UserID = "sa";
            //constr.Password = "123456";
            //constr.IntegratedSecurity=true;
            db.ConnectionString = constr.ToString();

            SqlCommand myCommand = new SqlCommand();
            myCommand.Parameters.Add("@mdate", System.Data.SqlDbType.DateTime).Value = entry.Date;
            myCommand.Parameters.Add("@msg", System.Data.SqlDbType.VarChar).Value = entry.Message;
            myCommand.CommandText = "INSERT INTO ENTRIES (mdate, message) VALUES(@mdate, @msg)";
            myCommand.Connection = db;


            db.Open();
            myCommand.ExecuteNonQuery();
            db.Close();
        }

        public long GetEntryCount()
        {
            string sql = "SELECT * FROM ENTRIES";
            SqlCommand command = new SqlCommand(sql, db);
            command.Connection.Open();
            object result = command.ExecuteScalar();
            command.Connection.Close();
            long r = 0;
            if (result != null)
            {
                r = Convert.ToInt32(result);
            }
            return r;
        }

        public IList<LogEntry> GetEntriesPage(int limit, int offset)
        {
            string sql = "SELECT * FROM ENTRIES LIMIT @lim offset @off";

            SqlParameter lim = new SqlParameter();
            lim.SqlDbType = System.Data.SqlDbType.Int;
            lim.ParameterName = "@lim";
            lim.Value = limit;

            SqlParameter off = new SqlParameter();
            lim.SqlDbType = System.Data.SqlDbType.Int;
            lim.ParameterName = "@off";
            lim.Value = offset;

            SqlCommand scomm = new SqlCommand(sql, db);
            scomm.Parameters.Add(lim);
            scomm.Parameters.Add(off);

            scomm.Connection.Open();
            SqlDataReader reader = scomm.ExecuteReader();

            IList<LogEntry> entries = new List<LogEntry>();
            while (reader.Read())
            {
                entries.Add(new LogEntry { _id = reader[0].ToString(), Date = (DateTime)reader[1], Message = reader[2].ToString() });
            }
            scomm.Connection.Close();
            return entries;


        }






        #region IDisposable Members
        void IDisposable.Dispose()
        {

        }

        #endregion
    }
}