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
            using (var dal = new DmDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
            }
            var dic = db.Dictionary.Where(d => d.DTypeCode == "DmObjectType" && d.DEnable == 1)
                .OrderBy(d => d.DCode).ToList();
            var stmt = (from d in db.Dictionary
                        where (d.DTypeCode == "DmAuditSTMT" || d.DTypeCode == "DmAuditObject") && d.DEnable == 1
                        orderby d.DTypeCode
                        select d.DCode).OrderBy(c => c).Distinct().ToList();
            stmt.Remove("ALL");
            ViewBag.STMT = stmt.OrderBy(s => s).ToList();
            return View(dic);
        }
        public ActionResult List(long scId, string user, string objname, string type, string begtime, string endtime, int page = 1, int limit = 20)
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
                    sbSql.Append("SELECT ar.*,'' AS POLICY_NAME FROM SYSAUDITOR.V$AUDITRECORDS ar");
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        sbSql.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                        sbCount.AddCondition(string.Format("USERNAME LIKE '%{0}%'", user.ToUpper()));
                    }
                    if (!string.IsNullOrWhiteSpace(objname))
                    {
                        sbSql.AddCondition(string.Format("OBJNAME LIKE '%{0}%'", objname.ToUpper()));
                        sbCount.AddCondition(string.Format("OBJNAME LIKE '%{0}%'", objname.ToUpper()));
                    }
                    if (!string.IsNullOrWhiteSpace(type))
                    {
                        sbSql.AddCondition(string.Format("OPERATION LIKE '%{0}%'", type.ToUpper()));
                        sbCount.AddCondition(string.Format("OPERATION LIKE '%{0}%'", type.ToUpper()));
                    }
                    if (!string.IsNullOrWhiteSpace(begtime))
                    {
                        sbSql.AddCondition(string.Format("OPTIME >= '{0}'", begtime));
                        sbCount.AddCondition(string.Format("OPTIME >= '{0}'", begtime));
                    }
                    if (!string.IsNullOrWhiteSpace(endtime))
                    {
                        sbSql.AddCondition(string.Format("OPTIME < '{0}'", endtime));
                        sbCount.AddCondition(string.Format("OPTIME < '{0}'", endtime));
                    }
                    sbSql.AppendFormat(" LIMIT {0} OFFSET {1}",
                        limit, (page - 1) * limit);


                    int count = Convert.ToInt32(dal.ExecuteScalar(sbCount.ToString()));
                    var dt = dal.ExecuteQuery(sbSql.ToString());
                    var policies = db.AuditPolicy.Where(p => p.SCID == scId).ToList();
                    if (dt.Rows.Count > 0 && policies.Count > 0)
                    {
                        
                        foreach(DataRow row in dt.Rows)
                        {
                            var username = row["USERNAME"].ToString();
                            var schema = row["SCHNAME"].ToString();
                            var obj = row["OBJNAME"].ToString();
                            var op = row["OPERATION"].ToString();
                            var sqlUpperCase = row["SQL_TEXT"].ToString().ToUpper();

                            if (schema == "" || obj == "") continue;
                            var policy = policies.Where(p => p.APUser == username && p.APSchema == schema && p.APObjectName == obj && p.APStatement == op &&
                                                        sqlUpperCase.Contains(p.APCondition)).FirstOrDefault();
                            /*var po = policies.FirstOrDefault();
                            if (po.APUser == username)
                            {
                                Console.WriteLine("abc");
                            }
                            if (po.APSchema == schema)
                            {
                                Console.WriteLine("abc");
                            }
                            if (po.APObjectName == obj)
                            {
                                Console.WriteLine("abc");
                            }
                            if (po.APStatement == op)
                            {
                                Console.WriteLine("abc");
                            }
                            if (sqlUpperCase.Contains(po.APCondition))
                            {
                                Console.WriteLine("");
                            }*/
                            if (policy != null)
                            {
                                row["POLICY_NAME"] = policy.APName;
                            }
                        }
                    }
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
                RecordException(ex);
            }
            return ret;
        }
    }
}