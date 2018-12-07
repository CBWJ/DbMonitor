using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using DbMonitor.Domain;
using DbMonitor.DBAccess;
using DbMonitor.DBAccess.Concrete;
using System.Data;
using System.Text;

namespace DbMonitor.WebUI.Utility
{
    public class StatusDataGraber
    {
        private static object _locker = new object();
        private static Dictionary<long, string> _dicWorkState = new Dictionary<long, string>();

        public static void ExecuteGrab(MonitorManagement mm, string dbType)
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
            //另开线程来采集
            Task.Factory.StartNew(new Action(() =>
            {
                GrabData(mm, dbType);
            }));
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
            catch { }
            lock (_locker)
            {
                _dicWorkState[mm.ID] = "wait";
            }
        }
        static void GrabOracleData(MonitorManagement mm)
        {
            //采集SQL
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("SELECT INSTANCE_NAME,HOST_NAME,VERSION,STARTUP_TIME,STATUS,DATABASE_STATUS FROM V$INSTANCE");
            DataTable dt = null;
            string connStr = DataGraber.GetSessionConnStr(mm.SCID.Value);
            using (OracleDAL dal = new OracleDAL(connStr))
            {
                dt = dal.ExecuteQuery(sbSql.ToString());
            }
            var grabTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var ctx = new DbMonitorEntities())
            {
                //把上一次数据设成历史数据
                var ds = ctx.DatabaseStatus.Where(s => s.REALTIME == 1);
                foreach (var s in ds)
                {
                    s.REALTIME = 0;
                    s.EditingTime = grabTime;
                }
                ctx.SaveChanges();
                foreach (DataRow dr in dt.Rows)
                {
                    DatabaseStatus status = ctx.DatabaseStatus.Create();
                    status.SCID = mm.SCID;
                    status.INSTANCE_NAME = dr["INSTANCE_NAME"].ToString();
                    status.HOST_NAME = dr["HOST_NAME"].ToString();
                    status.VERSION = dr["VERSION"].ToString();
                    status.STARTUP_TIME = dr["STARTUP_TIME"].ToString();
                    status.STATUS = dr["STATUS"].ToString();
                    status.DATABASE_STATUS = dr["DATABASE_STATUS"].ToString();
                    status.REALTIME = 1;
                    status.CreationTime = grabTime;

                    ctx.DatabaseStatus.Add(status);
                }
                ctx.SaveChanges();
                Console.WriteLine("id:{0} oracle-status-grab {1} items", mm.ID, dt.Rows.Count);
            }
        }
        static void GrabDmData(MonitorManagement mm)
        {
            //采集SQL
            StringBuilder sbSql = new StringBuilder();
            sbSql.Append("select HOST_NAME,INSTANCE_NAME,SVR_VERSION AS VERSION,START_TIME AS STARTUP_TIME,STATUS$ AS STATUS,MODE$ AS DATABASE_STATUS from V$INSTANCE");
            DataTable dt = null;
            string connStr = DataGraber.GetSessionConnStr(mm.SCID.Value);
            using (DmDAL dal = new DmDAL(connStr))
            {
                dt = dal.ExecuteQuery(sbSql.ToString());
            }
            var grabTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            using (var ctx = new DbMonitorEntities())
            {
                //把上一次数据设成历史数据
                var ds = ctx.DatabaseStatus.Where(s => s.REALTIME == 1);
                foreach (var s in ds)
                {
                    s.REALTIME = 0;
                    s.EditingTime = grabTime;
                }
                ctx.SaveChanges();
                foreach (DataRow dr in dt.Rows)
                {
                    DatabaseStatus status = ctx.DatabaseStatus.Create();
                    status.SCID = mm.SCID;
                    status.INSTANCE_NAME = dr["INSTANCE_NAME"].ToString();
                    status.HOST_NAME = dr["HOST_NAME"].ToString();
                    status.VERSION = dr["VERSION"].ToString();
                    status.STARTUP_TIME = dr["STARTUP_TIME"].ToString();
                    status.STATUS = dr["STATUS"].ToString();
                    status.DATABASE_STATUS = dr["DATABASE_STATUS"].ToString();
                    status.REALTIME = 1;
                    status.CreationTime = grabTime;

                    ctx.DatabaseStatus.Add(status);
                    ctx.SaveChanges();
                    Console.WriteLine("id:{0} dm-status-grab {1} items", mm.ID, dt.Rows.Count);
                }
            }
        }
    }
}