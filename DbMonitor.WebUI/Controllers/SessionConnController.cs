using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class SessionConnController : BaseController
    {
        // GET: SessionConn
        public ActionResult Index()
        {
            SetModuleAuthority();
            return View();
        }

        // GET: SessionConn/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: SessionConn/Create
        public ActionResult Create()
        {
            CreateAcion();
            Domain.SessionConnection sc = new Domain.SessionConnection
            {
                SCDBType = "ORACLE"
            };
            return View(sc);
        }

        // POST: SessionConn/Create
        [HttpPost]
        public ActionResult Create(Domain.SessionConnection sc,FormCollection collection)
        {
            return CreateModel(sc);
        }

        // GET: SessionConn/Edit/5
        public ActionResult Edit(int id)
        {
            EditAcion();
            Domain.SessionConnection sc = db.SessionConnection.Find(id);
            return View("Create", sc);
        }

        // POST: SessionConn/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.SessionConnection sc, FormCollection collection)
        {
            return EditModel(sc);
        }

        // GET: SessionConn/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: SessionConn/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {

            return DeleteModel<Domain.SessionConnection>(idList);
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
                    total = db.SessionConnection.Count(),
                    data = db.SessionConnection
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
