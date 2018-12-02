using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrAuditManageController : BaseController
    {
        // GET: OrAuditManage
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            return View();
        }
    }
}