using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrMonitorSwitchController : BaseController
    {
        // GET: OrMonitorSwitch
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();

            var mm = db.MonitorManagement.Where(m => m.SCID == id).FirstOrDefault();
            //不存在则添加
            if(mm == null)
            {
                mm = db.MonitorManagement.Create();
                mm.SCID = id;
                mm.MMCycleUnit = "s";
                mm.MMRefreshCycle = 30;
                mm.MMOpen = 0;
                mm.MMLastTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                db.MonitorManagement.Add(mm);
                db.SaveChanges();
            }
            ViewBag.ID = mm.ID;
            return View(mm);
        }
        [HttpPost]
        public ActionResult Edit(Domain.MonitorManagement manage, FormCollection collection)
        {
            return EditModel(manage);
        }
    }
}