using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;
using System.Data;

namespace SuUtil
{
    public abstract class OracleHelper
    {
        private static readonly string config = AppDomain.CurrentDomain.BaseDirectory + "Config\\DBConfig.xml";
        private static readonly string connString = DBConfig.GetConnStr("oracle");


        #region Test
        /// <summary>
        /// 测试数据库连接是否可用
        /// </summary>
        /// <returns></returns>
        public static bool TestConnection(string connString)
        {
            using (OracleConnection conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();
                }
                catch (Exception ex)
                {
                    Logger.Bard(ex);
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// 测试数据库连接是否可用
        /// </summary>
        /// <returns></returns>
        public static bool TestConnection()
        {
            return TestConnection(connString);
        }

        #endregion

        public static int ExecuteNonQuery(string sql, string connString)
        {
            using (OracleConnection conn = new OracleConnection(connString))
            {
                try
                {
                    conn.Open();
                    OracleCommand cmd = new OracleCommand(sql,conn);
                    return cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Logger.Bard(string.Format("OracleHelper.ExecuteNonQuery({0});\r\n{1}", sql, ex));
                    throw ex;
                }
            }
        }

        public static int ExecuteNonQuery(string sql)
        {
            return ExecuteNonQuery(sql, connString);
        }

        public static OracleDataReader QueryForReader(string sql, string connString)
        {
            try
            {
                OracleConnection conn = new OracleConnection(connString);
                conn.Open();
                OracleCommand cmd = new OracleCommand(sql, conn);
                OracleDataReader reader = cmd.ExecuteReader();
                //while (reader.Read())
                //{
                //    Console.WriteLine(reader.GetString(1));
                //}

                return reader;
            }
            catch (Exception ex)
            {
                Logger.Bard(ex);
                throw ex;
            }
        }

        /// <summary>
        /// Please close the reader manually
        /// </summary>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static OracleDataReader QueryForReader(string sql)
        {
            return QueryForReader(sql, connString);
        }
    }
}
