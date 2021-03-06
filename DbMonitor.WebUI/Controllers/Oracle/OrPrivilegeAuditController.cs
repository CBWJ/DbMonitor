﻿using DbMonitor.DBAccess.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DbMonitor.WebUI.Extensions;
using DbMonitor.DBAccess.Extensions;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrPrivilegeAuditController : BaseController
    {
        // GET: OrStatementAudit
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            using (OracleDAL dal = new OracleDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
            }
            var dic = db.Dictionary.Where(d => d.DTypeCode == "OracleAuditPrivilege" && d.DEnable == 1).
                OrderBy(d=>d.DCode).ToList();
            return View(dic);
        }

        public ActionResult List(long scId, int page = 1, int limit = 20, string user = "", string option = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                StringBuilder sbCount = new StringBuilder();
                StringBuilder sbSql = new StringBuilder();
                string tv = "DBA_PRIV_AUDIT_OPTS";
                sbCount.AppendFormat("SELECT COUNT(1) FROM {0} ", tv);
                sbSql.AppendFormat("SELECT * FROM (SELECT ROWNUM AS ROWNO, t.* FROM {0} t WHERE ", tv);
                //筛选条件
                if (!string.IsNullOrWhiteSpace(user))
                {
                    sbSql.AppendFormat("USER_NAME LIKE '%{0}%' AND ", user);
                    sbCount.AddCondition(string.Format("USER_NAME LIKE '%{0}%'", user));
                }
                if (!string.IsNullOrWhiteSpace(option))
                {
                    sbSql.AppendFormat("PRIVILEGE LIKE '%{0}%' AND ", option.ToUpper());
                    sbCount.AddCondition(string.Format("PRIVILEGE LIKE '%{0}%'", option.ToUpper()));
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
                RecordException(ex);
            }
            return ret;
        }

        public ActionResult Create(long scId)
        {
            //语句
            string[] arrPrivilege = { "CREATE EXTERNAL JOB","CREATE ANY JOB","GRANT ANY OBJECT PRIVILEGE","EXEMPT ACCESS POLICY",
                                            "CREATE ANY LIBRARY","GRANT ANY PRIVILEGE","DROP PROFILE","ALTER PROFILE","DROP ANY PROCEDURE",
                                            "ALTER ANY PROCEDURE","CREATE ANY PROCEDURE","ALTER DATABASE","GRANT ANY ROLE",
                                            "CREATE PUBLIC DATABASE LINK","DROP ANY TABLE","ALTER ANY TABLE","CREATE ANY TABLE",
                                            "DROP USER","ALTER USER","CREATE USER","CREATE SESSION","AUDIT SYSTEM","ALTER SYSTEM"};
            ViewBag.STMT = (from d in db.Dictionary
                            where d.DTypeCode == "OracleAuditPrivilege" && d.DEnable == 1
                            orderby d.DTypeCode
                            select d.DCode).OrderBy(c => c).ToList();
            //用户
            using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
            {
                ViewBag.User = dal.GetAllUsers().OrderBy(u => u).ToList();
            }
            return View(scId);
        }
        [HttpPost]
        public ActionResult Create(long scId, string stmt, string user, string way, string result)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("audit {0} ", stmt);
                if (user != "ALL")
                {
                    sbSql.AppendFormat("by {0} ", user);
                }
                sbSql.AppendFormat("by {0} ", way);
                if (result != "ALL")
                {
                    sbSql.AppendFormat("whenever {0} ", result);
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
                RecordException(ex);
            }
            return ret;
        }

        [HttpPost]
        public ActionResult Delete(long scId, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                var username = collection["USER_NAME"];
                var privilege = collection["PRIVILEGE"];

                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                sbSql.AppendFormat("noaudit {0}", privilege);
                if (!string.IsNullOrWhiteSpace(username))
                {
                    sbSql.AppendFormat(" by {0}", username);
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
                RecordException(ex);
            }
            return ret;
        }
    }
}