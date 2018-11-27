using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbMonitor.Domain;

namespace DbMonitor.WebUI.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            User u = null;
            using(var ctx = new DbMonitorEntities())
            {
                u = ctx.User.FirstOrDefault();
            }
            return View(u);
        }
    }
}