using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class ModuleController : BaseController
    {
        // GET: Module
        public ActionResult Index()
        {
            return View();
        }

        // GET: Module/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Module/Create
        public ActionResult Create()
        {
            Domain.Module m = new Domain.Module();
            return View(m);
        }

        // POST: Module/Create
        [HttpPost]
        public ActionResult Create(Domain.Module module, FormCollection collection)
        {
            return CreateModel(module);
            //JsonResult ret = new JsonResult();
            //try
            //{
            //    module.CreatorID = (int)LoginUser.ID;
            //    module.CreationTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            //    db.Module.Add(module);
            //    db.SaveChanges();
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 0,
            //        message = ""
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 1,
            //        message = ex.Message
            //    });
            //}
            //return ret;
        }

        // GET: Module/Edit/5
        public ActionResult Edit(int id)
        {
            var m = db.Module.Find(id);
            return View("Create", m);
        }

        // POST: Module/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.Module module, FormCollection collection)
        {
            return EditModel(module);
            //JsonResult ret = new JsonResult();
            //try
            //{
            //    var editModule = db.Module.Find(module.ID);

            //    editModule.MName = module.MName;
            //    editModule.MUrl = module.MUrl;
            //    editModule.MType = module.MType;
            //    editModule.IsEnabled = module.IsEnabled;
            //    editModule.MIconType = module.MIconType;
            //    editModule.MIcon = module.MIcon;
            //    editModule.MLevel = module.MLevel;
            //    editModule.MParentID = module.MParentID;
            //    editModule.MSortingNumber = module.MSortingNumber;

            //    editModule.EditorID = (int)LoginUser.ID;
            //    editModule.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

            //    db.SaveChanges();
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 0,
            //        message = ""
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 1,
            //        message = ex.Message
            //    });
            //}
            //return ret;
        }

        // GET: Module/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Module/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {
            return DeleteModel<Domain.Module>(idList);
            //JsonResult ret = new JsonResult();
            //try
            //{
            //    foreach (var id in idList)
            //    {
            //        var r = db.Module.Find(id);
            //        db.Module.Remove(r);
            //    }
            //    db.SaveChanges();
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 0,
            //        message = ""
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ret.Data = JsonConvert.SerializeObject(new
            //    {
            //        status = 1,
            //        message = ex.Message
            //    });
            //}
            //return ret;
        }

        public ActionResult List(int page = 1, int limit = 20, string username = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = db.Module.Count(),
                    data = db.Module
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
    }
}
