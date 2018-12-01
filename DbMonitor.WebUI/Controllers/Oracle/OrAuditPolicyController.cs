using DbMonitor.DBAccess.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DbMonitor.WebUI.Extensions;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrAuditPolicyController : BaseController
    {
        // GET: OrStatementAudit
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }

        public ActionResult List(long scId, int page = 1, int limit = 20, string user = "", string obj = "", string policy="")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                StringBuilder sbCount = new StringBuilder();
                StringBuilder sbSql = new StringBuilder();
                string tv = "DBA_AUDIT_POLICIES";
                sbCount.AppendFormat("SELECT COUNT(1) FROM {0} ", tv);
                sbSql.AppendFormat("SELECT * FROM (SELECT ROWNUM AS ROWNO, t.* FROM {0} t WHERE ", tv);
                //筛选条件
                if (!string.IsNullOrWhiteSpace(user))
                {
                    sbSql.AppendFormat("OBJECT_SCHEMA LIKE '%{0}%' AND ", user.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_SCHEMA LIKE '%{0}%'", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(obj))
                {
                    sbSql.AppendFormat("OBJECT_NAME LIKE '%{0}%' AND ", obj.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_NAME LIKE '%{0}%'", obj.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(policy))
                {
                    sbSql.AppendFormat("POLICY_NAME LIKE '%{0}%' AND ", policy.ToUpper());
                    sbCount.AddCondition(string.Format("POLICY_NAME LIKE '%{0}%'", policy.ToUpper()));
                }
                sbSql.AppendFormat("ROWNUM <= {0}) table_alias WHERE table_alias.ROWNO > {1}",
                    page * limit, (page - 1) * limit);

                int count = 0;
                DataTable dt = null;
                //string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User ID=sys;Password=sys;DBA Privilege=SYSDBA;";
                string connStr = GetSessionConnStr(scId);
                using (OracleDAL dal = new OracleDAL(connStr))
                {
                    count = Convert.ToInt32(dal.ExecuteScalar(sbCount.ToString()));
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
            }
            return ret;
        }

        public ActionResult Create(long scId)
        {            
            //用户
            using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendLine("select username from dba_users");
                DataTable dt = dal.ExecuteQuery(sbSql.ToString());
                List<string> users = new List<string>();
                foreach (DataRow row in dt.Rows)
                {
                    var u = row.ItemArray[0].ToString();
                    users.Add(u);
                }
                ViewBag.User = users.OrderBy(u => u).ToList();
            }
            return View(scId);
        }
        [HttpPost]
        public ActionResult Create(long scId, string stmt, string user, string objtype, string objname, string way, string result)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("audit {0} on {1}.{2} by {3}",
                    stmt, user, objname, way);
                if (result != "ALL")
                {
                    sbSql.AppendFormat(" whenever {0}", result);
                }
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
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
                    message = ex.Message
                });
            }
            return ret;
        }

        [HttpPost]
        public ActionResult Delete(long scId, string option, string user, string objname)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("noaudit {0} on {1}.{2}", option, user, objname);
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
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
                    message = ex.Message
                });
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
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("select object_name from dba_objects where owner='{0}' and object_type = '{1}'",
                    user, objtype);
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    DataTable dt = dal.ExecuteQuery(sbSql.ToString());

                    foreach (DataRow row in dt.Rows)
                    {
                        var u = row.ItemArray[0].ToString();
                        objs.Add(u);
                    }
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
            }
            return ret;
        }
    }
}