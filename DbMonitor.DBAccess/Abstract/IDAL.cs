using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbMonitor.DBAccess.Abstract
{
    public interface IDAL : IDisposable
    {
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        void Open();
        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
        /// <summary>
        /// 执行非查询语句
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        int ExecuteNonQuery(string sqlText);
        /// <summary>
        /// 执行返回单个值的查询语句
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        object ExecuteScalar(string sqlText);
        /// <summary>
        /// 执行查询语句结果集存入DataTable
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        DataTable ExecuteQuery(string sqlText);
        /// <summary>
        /// 执行无结果的存储过程
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        int ExecuteProcedureNonQuery(string sqlText, params DbParameter[] parameters);
        /// <summary>
        /// 执行存储过程查询
        /// </summary>
        /// <param name="sqlText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable ExecuteProcedureQuery(string sqlText, params DbParameter[] parameters);
    }
}
