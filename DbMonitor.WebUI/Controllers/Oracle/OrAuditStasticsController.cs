using DbMonitor.DBAccess.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrAuditStasticsController : BaseController
    {
        // GET: OrAuditStastics
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }
        public ActionResult List(long scId, int page = 1, int limit = 20, string username = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat(@"select 'Statement' ITEM, count(1) ALL_COUNT,count(1) ENABLE_COUNT,0 DISABLE_COUNT from DBA_STMT_AUDIT_OPTS
                                    UNION ALL
                                    select 'Object' Item, count(1) ALL_COUNT,count(1) ENABLE_COUNT,0 DISABLE_COUNT from DBA_OBJ_AUDIT_OPTS
                                    UNION ALL
                                    select 'Privilege' Item, count(1) ALL_COUNT,count(1) ENABLE_COUNT,0 DISABLE_COUNT from DBA_PRIV_AUDIT_OPTS
                                    UNION ALL
                                    Select * from 
                                    (select 'FGA' Item, count(1) ALL_COUNT from DBA_AUDIT_POLICIES
                                    ),
                                    (select COUNT(1) ENABLE_COUNT from DBA_AUDIT_POLICIES WHERE ENABLED = 'YES'),
                                    (select COUNT(1) DISABLE_COUNT from DBA_AUDIT_POLICIES WHERE ENABLED = 'NO')");                

                int count = 4;
                DataTable dt = null;
                //string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User ID=sys;Password=sys;DBA Privilege=SYSDBA;";
                string connStr = GetSessionConnStr(scId);
                using (OracleDAL dal = new OracleDAL(connStr))
                {
                    dt = dal.ExecuteQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = count,
                    data = dt
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message,
                    total = 0,
                    data = ""
                });
                RecordException(ex);
            }
            return ret;
        }
    }
}