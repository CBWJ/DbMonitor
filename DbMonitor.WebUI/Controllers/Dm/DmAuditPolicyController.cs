using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Dm
{
    public class DmAuditPolicyController : BaseController
    {
        // GET: DmAuditPolicy
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            using (var dal = new DmDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
            }
            return View();
        }

        public ActionResult List(long scId, int page = 1, int limit = 20, string user = "", string objname = "", string policy = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                var policies = db.AuditPolicy.Where(p => p.SCID == scId).ToList();
                if(!string.IsNullOrWhiteSpace(objname))
                {
                    policies = policies.Where(p => p.APObjectName.Contains(objname)).ToList();
                }
                if (!string.IsNullOrWhiteSpace(policy))
                {
                    policies = policies.Where(p => p.APName.Contains(policy)).ToList();
                }
                var count = policies.Count();
                policies = policies.Skip((page - 1) * limit)
                        .Take(limit)
                        .ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = count,
                    data = policies
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
            CreateAcion();
            var policy = db.AuditPolicy.Create();
            policy.SCID = scId;
            using (var dal = new DmDAL(GetSessionConnStr(scId)))
            {
                ViewBag.User = dal.GetAllUsers();
            }
            return View(policy);
        }
        [HttpPost]
        public ActionResult Create(Domain.AuditPolicy policy, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                using (var dal = new DmDAL(GetSessionConnStr(policy.SCID.Value)))
                {

                    StringBuilder sbSql = new StringBuilder();
                    sbSql.AppendFormat("SP_AUDIT_OBJECT('{0}', '{1}', '{2}', '{3}','{4}','{5}')",
                        policy.APStatement, policy.APUser, policy.APSchema, policy.APObjectName,
                        policy.APColumnName, policy.APWhen);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }

                db.AuditPolicy.Add(policy);
                db.SaveChanges();
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
        public ActionResult Delete(List<int> idList)
        {
            JsonResult ret = new JsonResult();
            try
            {
                var pt = db.AuditPolicy.Find(idList[0]);
                using (var dal = new DmDAL(GetSessionConnStr(pt.SCID.Value)))
                {
                    foreach (var id in idList)
                    {
                        var policy = db.AuditPolicy.Find(id);

                        StringBuilder sbSql = new StringBuilder();
                        sbSql.AppendFormat("SP_NOAUDIT_OBJECT('{0}', '{1}', '{2}', '{3}','{4}','{5}')",
                            policy.APStatement, policy.APUser, policy.APSchema, policy.APObjectName,
                            policy.APColumnName, policy.APWhen);
                        dal.ExecuteNonQuery(sbSql.ToString());

                        db.AuditPolicy.Remove(policy);
                    }
                }
                db.SaveChanges();
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
        public ActionResult GetObjectName(long scId, string user, string objtype = "TABLE")
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                using (var dal = new DmDAL(GetSessionConnStr(scId)))
                {
                    objs.AddRange(dal.GetObjectName(user, "UTAB"));
                    objs.AddRange(dal.GetObjectName(user, "VIEW"));
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
    }
}