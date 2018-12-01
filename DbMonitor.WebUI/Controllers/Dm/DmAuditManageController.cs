using DbMonitor.DBAccess.Concrete;
using DbMonitor.WebUI.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Dm
{
    public class DmAuditManageController : BaseController
    {
        // GET: DmAuditManage
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }
        public ActionResult List(long scId, string user, string obj, string type, int page = 1, int limit = 30)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                    StringBuilder sbSql = new StringBuilder();
                    StringBuilder sbCount = new StringBuilder();

                    sbCount.Append("SELECT COUNT(*) FROM ");
                    sbSql.Append("SELECT * FROM ");
                    string subTable = @"(SELECT ad.*,
                                    obj1.NAME AS USERNAME,
                                    obj2.NAME AS OBJECTNAME,
                                    (SELECT NAME FROM SYSCOLUMNS WHERE ID = ad.TVPID AND COLID = ad.COLID) AS COLNAME,
                                    (CASE LEVEL WHEN 1 THEN '语句级' WHEN 2 THEN '对象级' ELSE '' END) AS SLEVEL,
                                    (CASE TYPE 	WHEN 0 THEN 'ALL'
	                                    WHEN 12 THEN 'USER'
	                                    WHEN 13 THEN 'ROLE'
	                                    WHEN 9 THEN 'TABLESPACE'
	                                    WHEN 14 THEN 'SCHEMA'
	                                    WHEN 15 THEN 'TABLE'
	                                    WHEN 16 THEN 'VIEW'
	                                    WHEN 17 THEN 'INDEX'
	                                    WHEN 18 THEN 'PROCEDURE'
	                                    WHEN 19 THEN 'TRIGGER'
	                                    WHEN 20 THEN 'SEQUENCE'
	                                    WHEN 21 THEN 'CONTEXT'
	                                    WHEN 26 THEN 'SYNONYM'
	                                    WHEN 22 THEN 'GRANT'
	                                    WHEN 23 THEN 'REVOKE'
	                                    WHEN 24 THEN 'AUDIT'
	                                    WHEN 25 THEN 'NOAUDIT'
	                                    WHEN 30 THEN 'INSERT TABLE'
	                                    WHEN 33 THEN 'UPDATE TABLE'
	                                    WHEN 32 THEN 'DELETE TABLE'
	                                    WHEN 31 THEN 'SELECT TABLE'
	                                    WHEN 18 THEN 'PROCEDURE'
	                                    WHEN 44 THEN 'PACKAGE'
	                                    WHEN 45 THEN 'PACKAGE BODY'
	                                    WHEN 34 THEN 'MAC POLICY'
	                                    WHEN 35 THEN 'MAC LEVEL'
	                                    WHEN 36 THEN 'MAC COMPARTMENT'
	                                    WHEN 37 THEN 'MAC GROUP'
	                                    WHEN 38 THEN 'MAC LABEL'
	                                    WHEN 40 THEN 'MAC USER'
	                                    WHEN 41 THEN 'MAC TABLE'
	                                    WHEN 39 THEN 'MAC SESSION'
	                                    WHEN 28 THEN 'CHECKPOINT'
	                                    WHEN 75 THEN 'SAVEPOINT'
	                                    WHEN 76 THEN 'EXPLAIN'
	                                    WHEN 77 THEN 'NOT EXIST'
	                                    WHEN 70 THEN 'DATABASE'
	                                    WHEN 74 THEN 'CONNECT'
	                                    WHEN 72 THEN 'COMMIT'
	                                    WHEN 73 THEN 'ROLLBACK'
	                                    WHEN 43 THEN 'SET TRANSACTION'
                                        WHEN 50 THEN 'INSERT'
	                                    WHEN 53 THEN 'UPDATE'
	                                    WHEN 52 THEN 'DELETE'
	                                    WHEN 51 THEN 'SELECT'
	                                    WHEN 54 THEN 'EXECUTE'
	                                    WHEN 56 THEN 'MERGE INTO'
	                                    WHEN 55 THEN 'EXECUTE TRIGGER'
	                                    WHEN 57 THEN 'LOCK TABLE'
	                                    ELSE '' END) AS STYPE,
                                    '' AS SWHENEVER
                                    FROM SYSAUDITOR.SYSAUDIT ad
                                    LEFT OUTER JOIN sysobjects obj1 
                                    ON obj1.ID = ad.UID
                                    LEFT OUTER JOIN sysobjects obj2
                                    ON obj2.ID = ad.TVPID)";
                sbCount.Append(subTable);
                sbSql.Append(subTable);
                if(!string.IsNullOrWhiteSpace(user))
                {
                    sbCount.AddCondition(string.Format("USERNAME LIKE '%{0}%", user.ToUpper()));
                    sbSql.AddCondition(string.Format("USERNAME LIKE '%{0}%", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(obj))
                {
                    sbCount.AddCondition(string.Format("OBJECTNAME LIKE '%{0}%", user.ToUpper()));
                    sbSql.AddCondition(string.Format("OBJECTNAME LIKE '%{0}%", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(type))
                {
                    sbCount.AddCondition(string.Format("STYPE LIKE '%{0}%", user.ToUpper()));
                    sbSql.AddCondition(string.Format("STYPE LIKE '%{0}%", user.ToUpper()));
                }

                sbSql.AppendFormat(" LIMIT {0} OFFSET {1}",
                    limit, (page - 1) * limit);

                int count = 0;
                DataTable dt = null;
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {
                    count = Convert.ToInt32(dal.ExecuteScalar(sbCount.ToString()));
                    dt = dal.ExecuteQuery(sbSql.ToString()); 
                }
                foreach (DataRow dr in dt.Rows)
                {
                    var when = Convert.ToInt32(dr["WHENEVER"]);
                    string sWhen = "";
                    switch (when)
                    {
                        case 1:
                            sWhen = "SUCCESSFUL";
                            break;
                        case 2:
                            sWhen = "FAIL";
                            break;
                        case 3:
                            sWhen = "ALL";
                            break;
                    }
                    dr["SWHENEVER"] = sWhen;
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
            }
            return ret;
        }
    }
}