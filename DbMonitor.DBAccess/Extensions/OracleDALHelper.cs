using DbMonitor.DBAccess.Concrete;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMonitor.DBAccess.Extensions
{
    public static class OracleDALHelper
    {
        /// <summary>
        /// 查询一列值并以List<string>返回
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public static List<string> GetOneColumnValue(this OracleDAL dal, string sqlText)
        {
            List<string> ret = new List<string>();
            var dt = dal.ExecuteQuery(sqlText);

            foreach (DataRow row in dt.Rows)
            {
                ret.Add(row[0].ToString());
            }
            return ret;
        }
        /// <summary>
        /// 获取所有用户
        /// </summary>
        /// <param name="dal"></param>
        /// <returns></returns>
        public static List<string> GetAllUsers(this OracleDAL dal)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("select username from dba_users");

            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取某种所有对象
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <param name="objtype"></param>
        /// <returns></returns>
        public static List<string> GetObjectName(this OracleDAL dal, string user, string objtype)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("select object_name from dba_objects where owner = '{0}' and object_type = '{1}'",
                    user, objtype);

            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取用户的所有的表
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetAllTables(this OracleDAL dal, string user)
        {
            return GetObjectName(dal, user, "TABLE");
        }
        /// <summary>
        /// 获取某个对象的所有列名
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <param name="objname"></param>
        /// <returns></returns>
        public static List<string> GetColumnName(this OracleDAL dal, string user, string objname)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("select column_name from dba_tab_columns where owner='{0}' and TABLE_NAME = '{1}'",
                    user, objname);

            return dal.GetOneColumnValue(sbSql.ToString());
        }
    }
}
