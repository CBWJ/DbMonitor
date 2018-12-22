using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMonitor.Domain;
using DbMonitor.DBAccess;
using DbMonitor.DBAccess.Concrete;
using System.Data;
using System.Timers;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;

namespace DbMonitor.WebUI.Utility
{
    public class DataGraber
    {
        private static object _locker = new object();
        private static Timer _timer;
        private static Dictionary<long, string> _dicWorkState = new Dictionary<long, string>();
        private static Dictionary<string, string> _dicDmDDL = new Dictionary<string, string>();
        public static void Start()
        {
            using (var ctx = new DbMonitorEntities())
            {
                //加载数据
                var dmDDL = ctx.Dictionary.Where(d => d.DTypeCode == "DmDDL" && d.DEnable == 1).ToList();
                foreach(var d in dmDDL)
                {
                    _dicDmDDL.Add(d.DCode, d.DName);
                }
            }
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<MonitorManagement> mms = null;
            using (var ctx = new DbMonitorEntities())
            {
                mms = ctx.MonitorManagement.ToList();
                foreach (var mm in mms)
                {
                    var sc = ctx.SessionConnection.Find(mm.SCID);
                    if (sc != null && mm.MMOpen == 1)
                    {
                        ExecuteGrab(mm, sc.SCDBType);
                    }
                }
            }
        }        
        static void ExecuteGrab(MonitorManagement mm, string dbType)
        {
            //定义个工作状态（wait,working)
            if (_dicWorkState.ContainsKey(mm.ID))
            {
                lock (_locker)
                {
                    var state = _dicWorkState[mm.ID];
                    //工作中返回
                    if (state == "working")
                        return;
                }
            }
            else
            {
                _dicWorkState.Add(mm.ID, "wait");
            }
            //设置工作
            // _dicWorkState[mm.ID] = "working";
            var lastGrab = DateTime.Parse(mm.MMLastTime);
            DateTime currGrab = lastGrab;
            switch (mm.MMCycleUnit)
            {
                case "s":
                    currGrab = lastGrab.AddSeconds(mm.MMRefreshCycle.Value);
                    break;
                case "m":
                    currGrab = lastGrab.AddMinutes(mm.MMRefreshCycle.Value);
                    break;
                case "h":
                    currGrab = lastGrab.AddHours(mm.MMRefreshCycle.Value);
                    break;
            }
            if (DateTime.Now >= currGrab)
            {
                //另开线程来采集
                Task.Factory.StartNew(new Action(() => {
                    GrabData(mm, dbType);
                }));

                //状态抓取
                //StatusDataGraber.ExecuteGrab(mm, dbType);
            }
        }
        static void GrabData(MonitorManagement mm, string dbType)
        {
            lock (_locker)
            {
                _dicWorkState[mm.ID] = "working";
            }
            try
            {
                if (dbType == "ORACLE")
                {
                    GrabOracleData(mm);
                }
                else if (dbType == "DM")
                {
                    GrabDmData(mm);
                }
            }
            catch(Exception ex)
            {
                LogHelper.WriteError(ex, string.Format("抓取审计信息，监控管理ID：{0}，会话ID：{1}", mm.ID, mm.SCID));
            }
            lock (_locker)
            {
                _dicWorkState[mm.ID] = "wait";
            }
        }
        static void GrabOracleData(MonitorManagement mm)
        {
            DateTime dtBeg;
            DateTime dtEnd;
            //从上次最大时间开始
            dtBeg = DateTime.Parse(mm.MMLastTime);
            dtEnd = DateTime.Now;
            //采集SQL
            /*StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT t.DB_USER,t.OBJECT_SCHEMA,t.OBJECT_NAME,t.STATEMENT_TYPE,t.SQL_TEXT,to_char(t.EXTENDED_TIMESTAMP,'YYYY-MM-DD HH24:MI:SS') TIMESTAMP,o.OBJECT_TYPE ");
            sbSql.Append("FROM DBA_COMMON_AUDIT_TRAIL t ");
            sbSql.Append("LEFT JOIN dba_objects o ");
            sbSql.Append("ON t.OBJECT_SCHEMA = o.OWNER AND t.OBJECT_NAME = o.OBJECT_NAME ");
            sbSql.Append("WHERE t.OBJECT_NAME is not null AND object_type is not null and returncode = 0 ");
            sbSql.AppendFormat("and t.STATEMENT_TYPE not in('SELECT') ");
            sbSql.AppendFormat("and t.EXTENDED_TIMESTAMP >= to_date('{0}','yyyy-mm-dd hh24:mi:ss') ", dtBeg.ToString("yyyy-MM-dd HH:mm:ss"));
            sbSql.AppendFormat("and t.EXTENDED_TIMESTAMP < to_date('{0}','yyyy-mm-dd hh24:mi:ss') ", dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));
            */
            DataTable dt = null;
            string connStr = GetSessionConnStr(mm.SCID.Value);
            using (OracleDAL dal = new OracleDAL(connStr))
            {
                //dt = dal.ExecuteQuery(sbSql.ToString());
                dt = dal.ExecuteProcedureQuery("DB_MONITOR.P_GetChangeLog",
                    new OracleParameter("begtime", OracleDbType.Varchar2, dtBeg.ToString("yyyy-MM-dd HH:mm:ss"), ParameterDirection.Input),
                    new OracleParameter("endtime", OracleDbType.Varchar2, dtEnd.ToString("yyyy-MM-dd HH:mm:ss"), ParameterDirection.Input),
                    new OracleParameter("out_data", OracleDbType.RefCursor, ParameterDirection.Output)
                    );
            }
            var grabTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var ctx = new DbMonitorEntities())
            {
                foreach (DataRow dr in dt.Rows)
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
                    log.CLChangeTime = dr["OP_TIME"].ToString();
                    log.CLGrabTime = grabTime;
                    log.CLOldData = dr["OLD_DATA"].ToString();
                    log.CLNewData = dr["NEW_DATA"].ToString();
                    log.CLChangeType = dr["CHANGE_TYPE"].ToString();
                    ctx.ChangeLog.Add(log);
                    var editMM = ctx.MonitorManagement.Find(mm.ID);
                    editMM.MMLastTime = dtEnd.ToString("yyyy-MM-dd HH:mm:ss");
                    ctx.SaveChanges();
                }                
                Console.WriteLine("id:{0} grab {1} items", mm.ID, dt.Rows.Count);
            }
        }

        static void GrabDmData(MonitorManagement mm)
        {
            DateTime dtBeg;
            DateTime dtEnd;
            //从上次最大时间开始
            dtBeg = DateTime.Parse(mm.MMLastTime);
            dtEnd = DateTime.Now;
            //采集SQL
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT a.USERNAME, ");
            sbSql.Append("a.SCHNAME, ");
            sbSql.Append("a.OBJNAME, ");
            sbSql.Append("a.OPERATION, ");
            sbSql.Append("a.SQL_TEXT, ");
            sbSql.Append(" TO_CHAR(a.OPTIME,'yyyy-mm-dd HH24:MI:SS') TIMESTAMP, ");
            sbSql.Append("s.SUBTYPE$ OBJTYPE ");
            sbSql.Append("FROM SYSAUDITOR.V$AUDITRECORDS  a ");
            sbSql.Append("LEFT JOIN sysobjects s  ");
            sbSql.Append("ON a.SCHID = s.SCHID ");
            sbSql.Append("WHERE SUCC_FLAG = 'Y' ");
            sbSql.AppendFormat("AND a.OPTIME >= to_date('{0}', 'yyyy-mm-dd HH24:MI:SS') ", dtBeg.ToString("yyyy-MM-dd HH:mm:ss"));
            sbSql.AppendFormat("AND  a.OPTIME < to_date('{0}', 'yyyy-mm-dd HH24:MI:SS') ", dtEnd.ToString("yyyy-MM-dd HH:mm:ss"));

            DataTable dt = null;
            string connStr = GetSessionConnStr(mm.SCID.Value);
            using (DmDAL dal = new DmDAL(connStr))
            {
                dt = dal.ExecuteQuery(sbSql.ToString());
            }
            var grabTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bool bAdd;
            using (var ctx = new DbMonitorEntities())
            {
                foreach (DataRow dr in dt.Rows)
                {
                    bAdd = true;
                    ChangeLog log = new ChangeLog();

                    log.SCID = mm.SCID;
                    log.CLChangeEvent = dr["OPERATION"].ToString();
                    log.CLContent = "";
                    log.CLObjectName = dr["OBJNAME"].ToString();
                    log.CLSchema = dr["SCHNAME"].ToString();
                    log.CLObjectType = dr["OBJTYPE"].ToString();
                    log.CLSQL_Text = dr["SQL_TEXT"].ToString();
                    log.CLOperator = dr["USERNAME"].ToString();
                    log.CLChangeTime = dr["TIMESTAMP"].ToString();
                    log.CLGrabTime = grabTime;

                    var sql_Upper = log.CLSQL_Text.ToUpper();
                    if (_dicDmDDL.ContainsKey(log.CLChangeEvent))
                    {
                        var objName = GetObjectNameFormDDL(sql_Upper, log.CLChangeEvent);

                        if (log.CLChangeEvent.Contains("CREATE"))
                        {
                            log.CLOldData = "";
                            log.CLNewData = objName;
                        }
                        else if (log.CLChangeEvent.Contains("ALTER"))
                        {
                            var pos = sql_Upper.IndexOf(objName);
                            log.CLNewData = sql_Upper.Substring(pos + objName.Length);
                        }
                        else if (log.CLChangeEvent.Contains("DROP"))
                        {
                            log.CLOldData = objName;
                            log.CLNewData = "";
                        }
                        log.CLChangeType = _dicDmDDL[log.CLChangeEvent];
                    }
                    //数据操纵
                    else if (log.CLChangeEvent == "INSERT")
                    {
                        var pos = sql_Upper.IndexOf("VALUES") + 6;
                        var lastPos = sql_Upper.LastIndexOf(")");
                        log.CLOldData = "";
                        log.CLNewData = sql_Upper.Substring(pos, lastPos - pos);
                        log.CLChangeType = "插入数据";
                    }
                    else if (log.CLChangeEvent == "UPDATE")
                    {
                        var pos = sql_Upper.IndexOf("SET");
                        log.CLOldData = "";
                        log.CLNewData = sql_Upper.Substring(pos);
                        log.CLChangeType = "更新数据";
                    }
                    else if (log.CLChangeEvent == "DELETE")
                    {
                        var pos = sql_Upper.IndexOf(log.CLObjectName);
                        log.CLOldData = "";
                        log.CLNewData = sql_Upper.Substring(pos + log.CLObjectName.Length);
                        log.CLChangeType = "删除数据";
                    }
                    else
                        bAdd = false;
                    if (bAdd)
                        ctx.ChangeLog.Add(log);
                    //有数据才更新数据库
                    var editMM = ctx.MonitorManagement.Find(mm.ID);
                    editMM.MMLastTime = dtEnd.ToString("yyyy-MM-dd HH:mm:ss");
                    ctx.SaveChanges();
                }                
                Console.WriteLine("id:{0} grab {1} items", mm.ID, dt.Rows.Count);
            }
        }
        /// <summary>
        /// 获取每个会话的字符串
        /// </summary>
        /// <param name="scId"></param>
        /// <returns></returns>
        public static string GetSessionConnStr(long scId)
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
        /// <summary>
        /// 从DDL语言提取对象名
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetObjectNameFormDDL(string sql, string type)
        {
            string ret = "";
            string pattern = "";
            if (type.Contains("CREATE"))
            {
                pattern = type + @"\s(\S+)\s*\(";
            }
            else
            {
                pattern = type + @"\s(\S+)\s*";
            }
            Regex reg = new Regex(pattern);
            var m = reg.Match(sql);
            if (m.Groups.Count == 2)
            {
                ret = m.Groups[1].Value;
            }
            return ret;
        }
    }
}