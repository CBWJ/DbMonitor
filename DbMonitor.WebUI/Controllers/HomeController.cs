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
            //系统用户直接加载启用模块
            List<Module> modules = null;
            switch(LoginUser.UUserType)
            {
                case 0: //开发人员
                    modules = db.Module.Where(m => m.MType == "menu").ToList();
                    break;
                case 1: //系统管理员
                    modules = db.Module.Where(m => m.IsEnabled == 1 && m.MType == "menu").ToList();
                    break;
                case 2: //普通用户
                    modules = db.Module.Where(m => m.IsEnabled == 1 && m.MType == "menu" && m.MParentID == 0).ToList();

                    modules.AddRange((from ur in db.UserRole
                               join ra in db.RoleAuthority on ur.RID equals ra.RID
                               join ma in db.ModuleAuthority on ra.MAID equals ma.ID
                               join m in db.Module on ma.MID equals m.ID
                               where ur.UID == LoginUser.ID && m.IsEnabled == 1
                               select m).Distinct().ToList());
                    
                    break;
            }
            ViewBag.Menu = modules;
            return View(LoginUser);
        }
    }
}