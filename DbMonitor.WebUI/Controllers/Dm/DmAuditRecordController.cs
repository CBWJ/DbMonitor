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
    public class DmAuditRecordController : BaseController
    {
        // GET: DmAuditManage
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }
        public ActionResult List(long scId, string user, string obj, string operation, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    StringBuilder sbCount = new StringBuilder();

                    sbCount.Append("SELECT COUNT(*) FROM SYSAUDITOR.V$AUDITRECORDS");
                    sbSql.Append("SELECT * FROM SYSAUDITOR.V$AUDITRECORDS");
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        sbSql.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                        sbCount.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                    }
                    if (!string.IsNullOrWhiteSpace(obj))
                    {
                        sbSql.AddCondition(string.Format("OBJNAME LIKE '%{0}%'", obj.ToUpper()));
                        sbCount.AddCondition(string.Format("OBJNAME LIKE '%{0}%'", obj.ToUpper()));
                    }
                    if (!string.IsNullOrWhiteSpace(operation))
                    {
                        sbSql.AddCondition(string.Format("OPERATION LIKE '%{0}%'", operation.ToUpper()));
                        sbCount.AddCondition(string.Format("OPERATION LIKE '%{0}%'", operation.ToUpper()));
                    }
                    sbSql.AppendFormat(" LIMIT {0} OFFSET {1}",
                        limit, (page - 1) * limit);


                    int count = Convert.ToInt32(dal.ExecuteScalar(sbCount.ToString()));
                    var dt = dal.ExecuteQuery(sbSql.ToString());
                    ret.Data = JsonConvert.SerializeObject(new
                    {
                        status = 0,
                        message = "",
                        total = count,
                        data = dt
                    });
                }
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