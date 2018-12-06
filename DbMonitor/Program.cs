using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMonitor.Domain;
using DbMonitor.DBAccess;
using DbMonitor.DBAccess.Concrete;
using System.Data;

namespace DbMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            MonitorManagement mm = null;
            using (var ctx = new DbMonitorEntities())
            {
                //ctx.User.Add(new User {
                //    ULoginName = "Admin",
                //    UPassword = "123"
                //});
                //ctx.SaveChanges();
                mm = ctx.MonitorManagement.Find(1);                
            }
            GrabData(mm);
            Console.WriteLine("OK");
            Console.ReadKey();
        }

        static void GrabData(MonitorManagement mm)
        {
            DateTime dtBeg;
            DateTime dtEnd;
            //首次采集
            //if (string.IsNullOrWhiteSpace(mm.MMLastTime))
            //{
            //    //从现在开始
            //    dtBeg = DateTime.Now;
            //}
            //else
            //{
            //    //从上次最大时间开始
            //    dtBeg = DateTime.Parse(mm.MMLastTime);
            //}
            ////采集范围
            //dtEnd = dtBeg.AddMinutes(mm.MMTimeRange.Value);
            //if(dtEnd > DateTime.Now)
            //{
            //    dtEnd = DateTime.Now;
            //}
            //从上次最大时间开始
            dtBeg = DateTime.Parse(mm.MMLastTime);
            dtEnd = DateTime.Now;
            //采集SQL
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT t.DB_USER,t.OBJECT_SCHEMA,t.OBJECT_NAME,t.STATEMENT_TYPE,t.SQL_TEXT,to_char(t.EXTENDED_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS') TIMESTAMP,o.OBJECT_TYPE ");
            sbSql.Append("FROM DBA_COMMON_AUDIT_TRAIL t ");
            sbSql.Append("LEFT JOIN dba_objects o ");
            sbSql.Append("ON t.OBJECT_SCHEMA = o.OWNER AND t.OBJECT_NAME = o.OBJECT_NAME ");
            sbSql.Append("WHERE t.OBJECT_NAME is not null AND object_type is not null and returncode = 0 ");
            sbSql.AppendFormat("and t.STATEMENT_TYPE not in('SELECT') ");
            sbSql.AppendFormat("and t.EXTENDED_TIMESTAMP >= to_date('{0}','yyyy-mm-dd hh24:mi:ss') ", dtBeg.ToString("yyyy-MM-dd HH:mm:ss"));
            sbSql.AppendFormat("and t.EXTENDED_TIMESTAMP < to_date('{0}','yyyy-mm-dd hh24:mi:ss') ", dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable dt = null;
            string connStr = GetSessionConnStr(mm.SCID.Value);
            using (OracleDAL dal = new OracleDAL(connStr))
            {
                dt = dal.ExecuteQuery(sbSql.ToString());
            }
            var grabTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var ctx = new DbMonitorEntities())
            {
                foreach(DataRow dr in dt.Rows)
                {
                    ChangeLog log = new ChangeLog();

                    log.SCID = mm.SCID;
                    log.CLChangeEvent = dr["STATEMENT_TYPE"].ToString();
                    log.CLContent = "";
                    log.CLObjectName = dr["OBJECT_NAME"].ToString();
                    log.CLSchema = dr["OBJECT_SCHEMA"].ToString();
                    log.CLObjectType = dr["OBJECT_TYPE"].ToString();
                    log.CLSQL_Text = dr["SQL_TEXT"].ToString();
                    log.CLOperator = dr["DB_USER"].ToString();
                    log.CLChangeTime = dr["TIMESTAMP"].ToString();
                    log.CLGrabTime = grabTime;

                    ctx.ChangeLog.Add(log);
                }
                var editMM = ctx.MonitorManagement.Find(mm.ID);
                editMM.MMLastTime = dtEnd.ToString("yyyy-MM-dd HH:mm:ss");
                ctx.SaveChanges();
            }
        }
        /// <summary>
        /// 获取每个会话的字符串
        /// </summary>
        /// <param name="scId"></param>
        /// <returns></returns>
        static string GetSessionConnStr(long scId)
        {
            StringBuilder sbConn = new StringBuilder();
            using (var db = new DbMonitorEntities())
            {
                var sc = db.SessionConnection.Find(scId);
                if (sc != null)
                {
                    if (sc.SCDBType == "ORACLE")
                    {
                        sbConn.AppendFormat("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));",
                            sc.SCHostName, sc.SCPort, sc.SCServiceName.ToUpper());
                        sbConn.AppendFormat("Persist Security Info=True;User ID={0};Password={1};",
                            sc.SCUser, sc.SCPassword);
                        if (sc.SCRole.ToUpper() == "SYSDBA")
                        {
                            sbConn.Append("DBA Privilege=SYSDBA;");
                        }
                    }
                    else if (sc.SCDBType == "DM")
                    {
                        sbConn.AppendFormat("Server={0}:{1};User Id={2};PWD={3}",
                            sc.SCHostName, sc.SCPort, sc.SCUser, sc.SCPassword);
                    }
                } 
            }
            return sbConn.ToString();
        }
    }
}
