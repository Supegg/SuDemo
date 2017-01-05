using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Windows;

namespace SuUtil
{
    public enum DataBaseType
    {
        SqlServer,
        Sqlite,
        Oracle
    }

    /// <summary>
    /// The constructor may throw EXCEPTION.It isn't a good design.
    /// </summary>
    public class DBFactroy
    {
        private static object queueLock = new object();//exec command by insert time order
        private DbConnection sqlconn;
        private DbCommand dbCommand;
        private DbProviderFactory provider;
        private DbTransaction dbTransaction;
        //private bool isDBOpened = false;
        private string connString = null;

        public DBFactroy(DataBaseType dbType, string name="mssql")
        {
            connString = DBConfig.GetConnStr(name);
            string strProviderName = GetProviderNameByDbType(dbType);
            provider = DbProviderFactories.GetFactory(strProviderName);
            sqlconn = provider.CreateConnection();
            sqlconn.ConnectionString = connString;

            Open();
        }

        /// <summary>
        /// called before CRUD
        /// </summary>
        /// <returns></returns>
        public bool Open()
        {
            if (sqlconn.State == ConnectionState.Open)
            {
                return true;
            }
            try
            {
                sqlconn.Open();
            }
            catch(Exception ex)
            {
                SuUtil.Logger.Bard("open db failed.\r\n"+ex.ToString());
                throw new Exception("Open db failed,pls check ConnectionString.");
            }
            return true;
        }

        public void Close()
        {
            sqlconn.Close();
        }

        /// <summary>
        /// select data
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns></returns>
        public DbDataReader ExecQuery(string strSql)
        {
            lock (queueLock)
            {
                dbCommand = sqlconn.CreateCommand();
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = strSql;
                return dbCommand.ExecuteReader();
            }
        }

        /// <summary>
        /// insert,update,delete data
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns> 对于 UPDATE、INSERT 和 DELETE 语句，返回值为该命令所影响的行数。 对于其他所有类型的语句，返回值为 -1。</returns>
        public int ExecNoQuery(string strSql)
        {
            lock (queueLock)
            {
                dbCommand = sqlconn.CreateCommand();
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = strSql;
                return dbCommand.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// excute transaction
        /// </summary>
        /// <param name="listSqlString"></param>
        /// <returns></returns>
        public bool ExecuteSqlTran(List<string> listSqlString)
        {
            lock (queueLock)
            {
                dbCommand = sqlconn.CreateCommand();
                dbTransaction = sqlconn.BeginTransaction();
                dbCommand.Transaction = dbTransaction;

                for (int n = 0; n < listSqlString.Count; n++)
                {
                    string strsql = listSqlString[n];
                    if (strsql.Trim().Length > 1)
                    {
                        dbCommand.CommandText = strsql;
                        dbCommand.CommandType = CommandType.Text;
                        dbCommand.ExecuteNonQuery();
                    }
                }
                dbTransaction.Commit();
                return true;
            }
        }

        /// <summary>
        /// select DataTable
        /// </summary>
        /// <param name="strSql"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public DataTable GetDataTableBySqlString(string strSql, string tableName = "table1")
        {
            lock (queueLock)
            {
                dbCommand = sqlconn.CreateCommand();
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = strSql;
                DataSet ds = new DataSet();
                DbDataAdapter adapter = provider.CreateDataAdapter();
                adapter.SelectCommand = dbCommand;
                adapter.Fill(ds, tableName);
                return (DataTable)(ds.Tables[tableName]);
            }
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query
        /// </summary>
        /// <param name="strSql"></param>
        /// <returns>
        /// If the first column of the first row in the result set is not found, a null reference is returned.
        /// If the value in the database is null, the query returns DBNull.Value
        /// </returns>
        public object ExecuteScalar(string strSql)
        {
            lock (queueLock)
            {
                dbCommand = sqlconn.CreateCommand();
                dbCommand.CommandType = CommandType.Text;
                dbCommand.CommandText = strSql;
                return dbCommand.ExecuteScalar();
            }
        }

        private string GetProviderNameByDbType(DataBaseType dbType)
        {
            string strProviderName = string.Empty;
            switch (dbType)
            {
                case DataBaseType.SqlServer:
                    strProviderName = "System.Data.SqlClient";
                    break;
                case DataBaseType.Sqlite:
                    strProviderName = "System.Data.SQLite";
                    break;
                case DataBaseType.Oracle:
                    strProviderName = "System.Data.OracleClient";
                    break;
                default:
                    throw new Exception("Unsupport database");
            }
            return strProviderName;
        }
    }
}
