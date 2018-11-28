using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbMonitor.Domain;
using System.Web.Security;

namespace DbMonitor.WebUI.Controllers
{
    public class BaseController : Controller
    {
        protected DbMonitorEntities db = new DbMonitorEntities();
        protected User LoginUser
        {
            get
            {
                using(var ctx = new DbMonitorEntities())
                {
                    var loginUser = (from u in ctx.User
                                     where u.ULoginName == HttpContext.User.Identity.Name
                                     select u).FirstOrDefault();
                    return loginUser;
                }
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}