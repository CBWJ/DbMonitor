using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbMonitor.DBAccess.Concrete;
using System.Text;
using System.Data;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class AuditRecordController : BaseController
    {
        // GET: AuditRecord
        public ActionResult Index()
        {
            SetModuleAuthority();
            return View();
        }

        // GET: AuditRecord/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AuditRecord/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AuditRecord/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: AuditRecord/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AuditRecord/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: AuditRecord/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AuditRecord/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult List(int page = 1, int limit = 20, string username = "")
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
                if (!string.IsNullOrWhiteSpace(username))
                {
                    sbSql.AppendFormat("DB_USER LIKE '%{0}%' AND ", username.ToUpper());
                }
                sbSql.AppendFormat("ROWNUM <= {0}) table_alias WHERE table_alias.ROWNO > {1}", 
                    page * limit, (page - 1) * limit);

                int count = 0;
                DataTable dt = null;
                string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User ID=sys;Password=sys;DBA Privilege=SYSDBA;";
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
    }
}
