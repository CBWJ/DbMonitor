using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrDatabaseStatusController : BaseController
    {
        // GET: OrDatabaseStatus
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }
        public ActionResult List(long scId, string time, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var status = db.DatabaseStatus.Where(m => m.SCID == scId).ToList();
                if (!string.IsNullOrWhiteSpace(time))
                {
                    var mm = db.MonitorManagement.Where(m => m.SCID == scId).FirstOrDefault();
                    DateTime query = DateTime.Parse(time);
                    DateTime dtBeg = query, dtEnd = query;
                    switch (mm.MMCycleUnit)
                    {
                        case "s":
                            dtBeg = query.AddSeconds(mm.MMRefreshCycle.Value * -1);
                            dtEnd = query.AddSeconds(mm.MMRefreshCycle.Value);
                            break;
                        case "m":
                            dtBeg = query.AddMinutes(mm.MMRefreshCycle.Value * -1);
                            dtEnd = query.AddMinutes(mm.MMRefreshCycle.Value);
                            break;
                        case "h":
                            dtBeg = query.AddHours(mm.MMRefreshCycle.Value * -1);
                            dtEnd = query.AddHours(mm.MMRefreshCycle.Value);
                            break;
                    }
                    status = status.Where(m => DateTime.Parse(m.CreationTime) >= dtBeg && DateTime.Parse(m.CreationTime) < dtEnd).ToList();
                }
                int cnt = status.Count;
                status = status.OrderByDescending(s => s.CreationTime).Skip((page - 1) * limit).Take(limit).ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = cnt,
                    data = status
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