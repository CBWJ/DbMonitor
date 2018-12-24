using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrChanglogManageController : BaseController
    {
        // GET: OrChanglogManage
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            var sc = db.SessionConnection.Find(id);
            List<DbMonitor.Domain.Dictionary> dic = null;
            if (sc != null)
            {
                if (sc.SCDBType == "ORACLE")
                {
                    using (OracleDAL dal = new OracleDAL(GetSessionConnStr(id)))
                    {
                        ViewBag.Users = dal.GetAllUsers();
                    }
                    dic = (from d in db.Dictionary
                           where d.DTypeCode == "OracleObjectType" && d.DEnable == 1
                           select d).OrderBy(s => s.DCode).ToList();
                }
                else if (sc.SCDBType == "DM")
                {
                    using (var dal = new DmDAL(GetSessionConnStr(id)))
                    {
                        ViewBag.Users = dal.GetAllUsers();
                    }
                    dic = (from d in db.Dictionary
                           where d.DTypeCode == "DmObjectType" && d.DEnable == 1
                           select d).OrderBy(s => s.DCode).ToList();
                }
                ViewBag.DBType = sc.SCDBType;
            }
            return View(dic);
        }

        public ActionResult List(long scId, string user, string objtype, string objname, string begtime, string endtime, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var log = new List<DbMonitor.Domain.ChangeLog>();
                using (var ctx = new DbMonitor.Domain.DbMonitorEntities())
                {
                    log = ctx.ChangeLog.Where(m => m.SCID == scId).ToList();
                }
                if (!string.IsNullOrWhiteSpace(user))
                {
                    log = log.Where(l => l.CLSchema.Contains(user.ToUpper())).ToList();
                }
                if (!string.IsNullOrWhiteSpace(objtype))
                {
                    log = log.Where(l => l.CLObjectType == objtype).ToList();
                }
                if (!string.IsNullOrWhiteSpace(objname))
                {
                    log = log.Where(l => l.CLObjectName.Contains(objname.ToUpper())).ToList();
                }
                if (!string.IsNullOrWhiteSpace(begtime))
                {
                    var beg = DateTime.Parse(begtime);
                    log = log.Where(l => DateTime.Parse(l.CLChangeTime) >= beg).ToList();
                }
                if (!string.IsNullOrWhiteSpace(endtime))
                {
                    var end = DateTime.Parse(endtime);
                    log = log.Where(l => DateTime.Parse(l.CLChangeTime) < end).ToList();
                }
                //log = log//.OrderByDescending(l => new { time = DateTime.Parse(l.CLChangeTime) })
                //    .Skip((page - 1) * limit)
                //    .Take(limit)
                //    .ToList();
                int cnt = log.Count;
                log = log.OrderBy(l=>l.CLChangeTime).Skip((page - 1) * limit).Take(limit).ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = cnt,
                    data = log
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

        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {
            return DeleteModel<Domain.ChangeLog>(idList);
        }
    }
}