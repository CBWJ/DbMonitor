using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class AuthorityController : BaseController
    {
        // GET: Authority
        public ActionResult Index()
        {
            return View();
        }

        // GET: Authority/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Authority/Create
        public ActionResult Create()
        {
            CreateAcion();
            Domain.Authority a = new Domain.Authority();
            return View(a);
        }

        // POST: Authority/Create
        [HttpPost]
        public ActionResult Create(Domain.Authority authority, FormCollection collection)
        {
            return CreateModel(authority);
        }

        // GET: Authority/Edit/5
        public ActionResult Edit(int id)
        {
            EditAcion();
            var a = db.Authority.Find(id);
            return View("Create", a);
        }

        // POST: Authority/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.Authority authority, FormCollection collection)
        {

            return EditModel(authority);
        }

        // GET: Authority/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Authority/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {

            return DeleteModel<Domain.Authority>(idList);
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
                    total = db.Authority.Count(),
                    data = db.Authority
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
