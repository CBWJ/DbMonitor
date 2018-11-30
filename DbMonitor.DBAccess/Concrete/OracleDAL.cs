using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMonitor.DBAccess.Abstract;
using System.Data;
using System.Data.Common;

namespace DbMonitor.DBAccess.Concrete
{
    public class OracleDAL : IDAL
    {
        public string ConnectionString { get; set; }
        private OracleConnection conn;
        public OracleDAL()
        {
            //ConnectionString = "";
            //Open();
        }
        public OracleDAL(string connString)
        {
            ConnectionString = connString;
            Open();
        }
        public void Close()
        {
            if (conn == null) return;
            if (conn.State != ConnectionState.Closed)
            {
                conn.Close();
            }
        }

        public void Dispose()
        {
            Close();
            conn.Dispose();
        }

        public int ExecuteNonQuery(string sqlText)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            return cmd.ExecuteNonQuery();
        }

        public int ExecuteProcedureNonQuery(string sqlText, params DbParameter[] parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandText = sqlText;
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }

        public DataTable ExecuteQuery(string sqlText)
        {
            DataTable dataTable = new DataTable();
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            var adapter = new OracleDataAdapter(cmd);
            adapter.Fill(dataTable);
            return dataTable;
        }

        public object ExecuteScalar(string sqlText)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            return cmd.ExecuteScalar();
        }

        public void Open()
        {
            conn = new OracleConnection();
            conn.ConnectionString = ConnectionString;
            conn.Open();
        }
    }
}
