using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbMonitor.Domain;

namespace DbMonitor.WebUI.Controllers
{
    public class HomeController : BaseController
    {
        // GET: Home
        public ActionResult Index()
        {
            if(LoginUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            ViewBag.Menu = db.Module.ToList();
            return View(LoginUser);
        }
    }
}