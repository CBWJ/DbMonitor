using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
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
            using (var dal = new DmDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
            }
            var dic = db.Dictionary.Where(d => d.DTypeCode == "DmObjectType" && d.DEnable == 1)
                .OrderBy(d => d.DCode).ToList();
            var stmt = (from d in db.Dictionary
                            where (d.DTypeCode == "DmAuditSTMT" || d.DTypeCode == "DmAuditObject") && d.DEnable == 1
                            orderby d.DTypeCode
                            select d.DCode).OrderBy(c=>c).Distinct().ToList();
            stmt.Remove("ALL");
            ViewBag.STMT = stmt.OrderBy(s => s).ToList();
            return View(dic);
        }
        public ActionResult List(long scId, string user, string objname, string type, int page = 1, int limit = 30)
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
                                    (SELECT NAME FROM sysobjects WHERE TYPE$='SCH' AND SUBTYPE$ IS NULL AND ID = (SELECT SCHID FROM sysobjects WHERE ID = ad.TVPID)) AS SCHEMANAME,
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
                    sbCount.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                    sbSql.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(objname))
                {
                    sbCount.AddCondition(string.Format("OBJECTNAME LIKE '%{0}%'", objname.ToUpper()));
                    sbSql.AddCondition(string.Format("OBJECTNAME LIKE '%{0}%'", objname.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(type))
                {
                    sbCount.AddCondition(string.Format("STYPE LIKE '%{0}%'", type.ToUpper()));
                    sbSql.AddCondition(string.Format("STYPE LIKE '%{0}%'", type.ToUpper()));
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
                RecordException(ex);
            }
            return ret;
        }
        /// <summary>
        /// get的参数名必须是id,路由规则定了
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult CreateStatement(long id)
        {
            string[] arrStatement = {"ALL","USER","ROLE","TABLESPACE","SCHEMA","TABLE","VIEW","INDEX",
                    "PROCEDURE","TRIGGER","SEQUENCE","CONTEXT","SYNONYM","GRANT",
                    "REVOKE","AUDIT","NOAUDIT","INSERT TABLE","UPDATE TABLE",
                    "DELETE TABLE","SELECT TABLE","EXECUTE","PROCEDURE","PACKAGE",
                    "PACKAGE BODY","MAC POLICY","MAC LEVEL","MAC COMPARTMENT",
                    "MAC GROUP","MAC LABEL","MAC USER","MAC TABLE","MAC SESSION",
                    "CHECKPOINT","SAVEPOINT","EXPLAIN","NOT EXIST","DATABASE",
                    "CONNECT","COMMIT","ROLLBACK","SET TRANSACTION"};
            ViewBag.STMT = (from d in db.Dictionary
                            where d.DTypeCode == "DmAuditSTMT" && d.DEnable == 1
                            orderby d.DTypeCode
                            select d.DCode).OrderBy(c => c).ToList();

            using (var dal = new DmDAL(GetSessionConnStr(id)))
            {
                ViewBag.User = dal.GetAllUsers();
            }
            return View(id);
        }
        
        [HttpPost]
        public ActionResult CreateStatement(long scId, string type, string username, string whenever)
        {
            JsonResult ret = new JsonResult();
            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    //sbSql.Append("SP_AUDIT_STMT");
                    //dal.ExecuteProcedureNonQuery(sbSql.ToString(),
                    //    new DmParameter("TYPE", type),
                    //    new DmParameter("USERNAME", username),
                    //    new DmParameter("WHENEVER", whenever));
                    sbSql.AppendFormat("SP_AUDIT_STMT('{0}', '{1}', '{2}')",
                        type, username, whenever);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }

        public ActionResult CreateObject(long id)
        {
            string[] arrStatement = { "ALL", "INSERT", "UPDATE", "DELETE", "SELECT", "EXECUTE", "MERGE INTO", "EXECUTE TRIGGER", "LOCK TABLE" };
            ViewBag.STMT = (from d in db.Dictionary
                            where d.DTypeCode == "DmAuditObject" && d.DEnable == 1
                            orderby d.DTypeCode
                            select d.DCode).OrderBy(c => c).ToList();

            using (var dal = new DmDAL(GetSessionConnStr(id)))
            {
                ViewBag.User = dal.GetAllUsers();
            }
            ViewBag.ObjectTypes = db.Dictionary.Where(d => d.DTypeCode == "DmObjectType" && d.DEnable == 1)
                .OrderBy(d => d.DCode).ToList();
            return View(id);
        }
        [HttpPost]
        public ActionResult CreateObject(long scId, string type,
                                            string username,
                                            string schemaname,
                                            string tvname,
                                            string colname,
                                            string whenever)
        {
            JsonResult ret = new JsonResult();
            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    sbSql.AppendFormat("SP_AUDIT_OBJECT('{0}', '{1}', '{2}', '{3}'",
                        type, username, schemaname, tvname);
                    if (!string.IsNullOrWhiteSpace(colname))
                    {
                        sbSql.AppendFormat(",'{0}'", colname);
                    }
                    sbSql.AppendFormat(",'{0}')", whenever);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
        [HttpPost]
        public ActionResult GetObjectName(long scId, string user, string objtype = "TABLE")
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {
                    objs.AddRange(dal.GetObjectName(user, objtype));
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    data = objs
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }

        /// <summary>
        /// 获取某个对象的所有列名
        /// </summary>
        /// <param name="scId"></param>
        /// <param name="user"></param>
        /// <param name="objname"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetColumnName(long scId, string user, string objname)
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {
                    objs = dal.GetAllColumns(user, objname);   
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    data = objs
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
        [HttpPost]
        public ActionResult DeleteStatement(long scId, string type, string username, string whenever)
        {
            JsonResult ret = new JsonResult();
            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    sbSql.AppendFormat("SP_NOAUDIT_STMT('{0}', '{1}', '{2}')",
                        type, string.IsNullOrWhiteSpace(username) ? "NULL" : username, whenever);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
        [HttpPost]
        public ActionResult DeleteObject(long scId, string type,
                                            string username,
                                            string schemaname,
                                            string tvname,
                                            string colname,
                                            string whenever)
        {
            JsonResult ret = new JsonResult();
            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    sbSql.AppendFormat("SP_NOAUDIT_OBJECT('{0}', '{1}', '{2}', '{3}'",
                        type, username, schemaname, tvname);
                    if (!string.IsNullOrWhiteSpace(colname))
                    {
                        sbSql.AppendFormat(",'{0}'", colname);
                    }
                    sbSql.AppendFormat(",'{0}')", whenever);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
    }
}