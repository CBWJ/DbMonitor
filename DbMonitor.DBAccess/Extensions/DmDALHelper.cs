using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMonitor.DBAccess.Concrete;
using System.Data;

namespace DbMonitor.DBAccess.Extensions
{
    /// <summary>
    /// 代码小菜，DM数据字典查询类
    ///
    /// </summary>
    public static class DmDALHelper
    {
        /// <summary>
        /// 查询一列值并以List<string>返回
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        public static List<string> GetOneColumnValue(this DmDAL dal, string sqlText)
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
        public static List<string> GetAllUsers(this DmDAL dal)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("SELECT NAME FROM sysobjects WHERE TYPE$='UR' AND SUBTYPE$='USER'");
            
            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取所有模式
        /// </summary>
        /// <param name="dal"></param>
        /// <returns></returns>
        public static List<string> GetAllSchemas(this DmDAL dal)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat("SELECT * FROM sysobjects WHERE TYPE$='SCH' AND SUBTYPE$ IS NULL");

            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取某种所有对象
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <param name="objtype"></param>
        /// <returns></returns>
        public static List<string> GetObjectName(this DmDAL dal, string user, string objtype)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat(@"SELECT NAME FROM sysobjects WHERE 
                SCHID = (SELECT ID FROM sysobjects WHERE TYPE$ = 'SCH' AND NAME ='{0}')
                AND TYPE$='SCHOBJ' AND SUBTYPE$='{1}'",
                    user, objtype);

            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取用户的所有的表
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetAllTables(this DmDAL dal, string user)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat(@"SELECT NAME FROM sysobjects WHERE 
                SCHID = (SELECT ID FROM sysobjects WHERE TYPE$ = 'SCH' AND NAME ='{0}')
                AND TYPE$='SCHOBJ' AND SUBTYPE$='UTAB'", user);
            return dal.GetOneColumnValue(sbSql.ToString());
        }
        /// <summary>
        /// 获取用户的所有的视图
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetAllViews(this DmDAL dal, string user)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat(@"SELECT NAME FROM sysobjects WHERE 
                SCHID = (SELECT ID FROM sysobjects WHERE TYPE$ = 'SCH' AND NAME ='{0}')
                AND TYPE$='SCHOBJ' AND SUBTYPE$='VIEW'", user);
            return dal.GetOneColumnValue(sbSql.ToString());
        }

        /// <summary>
        /// 获取用户某个对象的所有列
        /// </summary>
        /// <param name="dal"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static List<string> GetAllColumns(this DmDAL dal, string user, string obj)
        {
            StringBuilder sbSql = new StringBuilder();
            sbSql.AppendFormat(@"SELECT NAME FROM SYSCOLUMNS 
                WHERE ID = (SELECT ID FROM sysobjects 
                WHERE SCHID = (SELECT ID FROM sysobjects WHERE TYPE$ = 'SCH' AND NAME ='{0}') 
                AND NAME='{1}')", 
                    user, obj);
            return dal.GetOneColumnValue(sbSql.ToString());
        }
    }
}
