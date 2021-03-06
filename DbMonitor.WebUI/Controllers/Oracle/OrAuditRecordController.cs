﻿using DbMonitor.DBAccess.Concrete;
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

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrAuditRecordController : BaseController
    {
        // GET: OrAuditRecord
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            using (OracleDAL dal = new OracleDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
                ViewBag.ObjectTypes = dal.GetAllObjectTypes();
                ViewBag.STMT = dal.GetOneColumnValue("select statement_type from DBA_COMMON_AUDIT_TRAIL GROUP BY statement_type ORDER BY statement_type");
            }
            return View();
        }
        public ActionResult List(long scId, string schema, string user, string objname, string type, string begtime, string endtime, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                StringBuilder sbCount = new StringBuilder();
                StringBuilder sbSql = new StringBuilder();
                string tv = "DBA_COMMON_AUDIT_TRAIL";
                sbCount.AppendFormat("SELECT COUNT(1) FROM {0}", tv);
                sbSql.AppendFormat("SELECT * FROM (SELECT ROWNUM AS ROWNO, t.* FROM {0} t WHERE ", tv);
                //筛选条件
                if (!string.IsNullOrWhiteSpace(schema))
                {
                    sbSql.AppendFormat("OBJECT_SCHEMA LIKE '%{0}%' AND ", schema.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_SCHEMA LIKE '%{0}%'", schema.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(user))
                {
                    sbSql.AppendFormat("DB_USER LIKE '%{0}%' AND ", user.ToUpper());
                    sbCount.AddCondition(string.Format("DB_USER LIKE '%{0}%'", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(objname))
                {
                    sbSql.AppendFormat("OBJECT_NAME LIKE '%{0}%' AND ", objname.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_NAME LIKE '%{0}%'", objname.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(type))
                {
                    sbSql.AppendFormat("STATEMENT_TYPE LIKE '%{0}%' AND ", type.ToUpper());
                    sbCount.AddCondition(string.Format("STATEMENT_TYPE LIKE '%{0}%'", type.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(begtime))
                {
                    sbSql.AppendFormat("extended_timestamp >= to_date('{0}','yyyy-MM-dd HH24:mi:ss') AND ", begtime);
                    sbCount.AddCondition(string.Format("extended_timestamp >= to_date('{0}','yyyy-MM-dd HH24:mi:ss')", begtime));
                }
                if (!string.IsNullOrWhiteSpace(endtime))
                {
                    sbSql.AppendFormat("extended_timestamp <= to_date('{0}','yyyy-MM-dd HH24:mi:ss') AND ", endtime);
                    sbCount.AddCondition(string.Format("extended_timestamp <= to_date('{0}','yyyy-MM-dd HH24:mi:ss')", endtime));
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
    }
}