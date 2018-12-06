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
            return View();
        }

        public ActionResult List(long scId, string user, string obj, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var log = db.ChangeLog.Where(m => m.SCID == scId).ToList();
                if (!string.IsNullOrWhiteSpace(user))
                {
                    log = log.Where(l => l.CLOperator.Contains(user.ToUpper())).ToList();
                }
                if (!string.IsNullOrWhiteSpace(obj))
                {
                    log = log.Where(l => l.CLObjectName.Contains(obj.ToUpper())).ToList();
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