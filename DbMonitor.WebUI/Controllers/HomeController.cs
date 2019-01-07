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
            SetModuleAuthority();
            if (LoginUser == null)
            {
                return RedirectToAction("Login", "Account");
            }
            //系统设置
            List<Dictionary> dicSys = db.Dictionary.Where(d => d.DTypeCode == "SysSettings" && d.DEnable == 1).ToList();
            ViewBag.SystemSettings = dicSys;
            return View(LoginUser);
        }

        public ActionResult Menu()
        {
            List<Module> modules = GenerateMenuModules();
            return View(modules);
        }

        private List<Module> GenerateMenuModules()
        {
            //系统用户直接加载启用模块
            List<Module> modules = new List<Module>();
            switch (LoginUser.UUserType)
            {
                case 0: //开发人员
                    modules = db.Module.Where(m => m.MType == "menu").ToList();
                    break;
                case 1: //系统管理员
                    modules = db.Module.Where(m => m.IsEnabled == 1 && m.MType == "menu").ToList();
                    break;
                case 2: //普通用户
                    //modules = db.Module.Where(m => m.IsEnabled == 1 && m.MType == "menu" && m.MParentID == 0).ToList();
                    modules = ((from ur in db.UserRole
                                join ra in db.RoleAuthority on ur.RID equals ra.RID
                                join ma in db.ModuleAuthority on ra.MAID equals ma.ID
                                join m in db.Module on ma.MID equals m.ID
                                where ur.UID == LoginUser.ID && m.IsEnabled == 1 && m.MType == "menu"
                                select m).Distinct().ToList());
                    //搜索父模块
                    var parents = SearchParentModule(modules);
                    var hasId = (from m in modules
                                 select m.ID).ToList();
                    foreach (var p in parents)
                    {
                        if (!hasId.Contains(p.ID))
                        {
                            modules.Add(p);
                        }
                    }
                    break;
            }
            //以上是常规模块，下面根据会话创建虚拟菜单
            //Oracle
            OracleMenu(ref modules);
            DmMenu(ref modules);
            return modules;
        }

        private void OracleMenu(ref List<Module> modules)
        {
            var scORCL = (from m in db.SessionConnection
                          where m.SCDBType == "ORACLE"
                          select m).ToList();
            long seed = 100000;
            if (scORCL.Count > 0)
            {
                //一级菜单
                var mList = new Module
                {
                    ID = seed++,
                    MName = "Oracle连接",
                    MUrl = "#",
                    MIconType = "font",
                    MIcon = "fa-database",
                    MParentID = 0,
                    MSortingNumber = 10
                };
                modules.Add(mList);

                long sort = 1;
                foreach (var orcl in scORCL)
                {
                    //每个连接是一个二级菜单                    
                    var mLink = new Module
                    {
                        ID = seed++,
                        MName = orcl.SCName,
                        MUrl = "#",
                        MIconType = "font",
                        MIcon = "fa-database",
                        MParentID = mList.ID,
                        MSortingNumber = sort++
                    };
                    //三级菜单
                    List<Module> orclMenu = null;
                    if (LoginUser.UUserType < 2)
                    {
                        orclMenu = (from m in db.Module
                                    where m.MType == "oraclemenu"
                                    select m).ToList();
                    }
                    else
                    {
                        orclMenu = (from ur in db.UserRole
                                    join ra in db.RoleAuthority on ur.RID equals ra.RID
                                    join ma in db.ModuleAuthority on ra.MAID equals ma.ID
                                    join m in db.Module on ma.MID equals m.ID
                                    where ur.UID == LoginUser.ID && m.IsEnabled == 1 && m.MType == "oraclemenu"
                                    select m).Distinct().ToList();
                    }
                    foreach (var m in orclMenu)
                    {
                        var mSubMenu = new Module
                        {
                            ID = m.ID,
                            MName = m.MName,
                            MUrl = m.MUrl + "/" + orcl.ID,
                            MIconType = m.MIconType,
                            MIcon = m.MIcon,
                            MParentID = mLink.ID,
                            MSortingNumber = m.MSortingNumber
                        };
                        modules.Add(mSubMenu);
                    }
                    if (orclMenu.Count > 0)
                        modules.Add(mLink);
                }
            }
        }
        private void DmMenu(ref List<Module> modules)
        {
            var scORCL = (from m in db.SessionConnection
                          where m.SCDBType == "DM"
                          select m).ToList();
            long seed = 200000;
            if (scORCL.Count > 0)
            {
                //一级菜单
                var mList = new Module
                {
                    ID = seed++,
                    MName = "达梦连接",
                    MUrl = "#",
                    MIconType = "font",
                    MIcon = "fa-table",
                    MParentID = 0,
                    MSortingNumber = 10
                };
                modules.Add(mList);

                long sort = 1;
                foreach (var orcl in scORCL)
                {
                    //每个连接是一个二级菜单                    
                    var mLink = new Module
                    {
                        ID = seed++,
                        MName = orcl.SCName,
                        MUrl = "#",
                        MIconType = "font",
                        MIcon = "fa-table",
                        MParentID = mList.ID,
                        MSortingNumber = sort++
                    };
                    //三级菜单
                    List<Module> orclMenu = null;
                    if (LoginUser.UUserType < 2)
                    {
                        orclMenu = (from m in db.Module
                                    where m.MType == "dmmenu"
                                    select m).ToList();
                    }
                    else
                    {
                        orclMenu = (from ur in db.UserRole
                                    join ra in db.RoleAuthority on ur.RID equals ra.RID
                                    join ma in db.ModuleAuthority on ra.MAID equals ma.ID
                                    join m in db.Module on ma.MID equals m.ID
                                    where ur.UID == LoginUser.ID && m.IsEnabled == 1 && m.MType == "dmmenu"
                                    select m).Distinct().ToList();
                    }
                    foreach (var m in orclMenu)
                    {
                        var mSubMenu = new Module
                        {
                            ID = m.ID,
                            MName = m.MName,
                            MUrl = m.MUrl + "/" + orcl.ID,
                            MIconType = m.MIconType,
                            MIcon = m.MIcon,
                            MParentID = mLink.ID,
                            MSortingNumber = m.MSortingNumber
                        };
                        modules.Add(mSubMenu);
                    }
                    if (orclMenu.Count > 0)
                        modules.Add(mLink);
                }
            }
        }

        public List<Module> SearchParentModule(List<Module> mods)
        {
            List<Module> parents = new List<Module>();
            foreach (var m in mods)
            {
                TraverseParent(m.MParentID.Value, ref parents);
            }
            return parents;
        }

        public void TraverseParent(long mId, ref List<Module> parents)
        {
            if (mId > 0)
            {
                var mp = db.Module.Find(mId);
                if (mp != null && !parents.Contains(mp))
                {
                    parents.Add(mp);
                    TraverseParent(mp.MParentID.Value, ref parents);
                }
            }
        }
    }
}