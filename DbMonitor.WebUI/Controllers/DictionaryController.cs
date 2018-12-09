using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class DictionaryController : BaseController
    {
        // GET: Dictionary
        public ActionResult Index()
        {
            var subSet = (from d in db.Dictionary
                         select new { value = d.DTypeCode, name = d.DTypeName }).ToList();
            Dictionary<string, string> dicType = new Dictionary<string, string>();
            foreach(var m in subSet)
            {
                if (!dicType.ContainsKey(m.value))
                {
                    dicType.Add(m.value, m.name);
                }
            }
            SetModuleAuthority();
            return View(dicType);
        }

        // GET: Dictionary/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Dictionary/Create
        public ActionResult Create()
        {
            CreateAcion();
            var dic = db.Dictionary.Create();
            return View(dic);
        }

        // POST: Dictionary/Create
        [HttpPost]
        public ActionResult Create(Domain.Dictionary dic, FormCollection collection)
        {
            return CreateModel(dic);
        }

        // GET: Dictionary/Edit/5
        public ActionResult Edit(int id)
        {
            EditAcion();
            var dic = db.Dictionary.Find(id);
            return View("Create", dic);
        }

        // POST: Dictionary/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.Dictionary dic, FormCollection collection)
        {
            return EditModel(dic);
        }

        // GET: Dictionary/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Dictionary/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public ActionResult List(int page = 1, int limit = 20, string type = "", string code = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var dic = db.Dictionary.ToList();
                if (!string.IsNullOrWhiteSpace(type))
                {
                    dic = dic.Where(d => d.DTypeCode == type).ToList();
                }
                if (!string.IsNullOrWhiteSpace(code))
                {
                    dic = dic.Where(d => d.DCode.Contains(code)).ToList();
                }
                var cnt = dic.Count;
                dic = dic.Skip((page - 1) * limit)
                        .Take(limit)
                        .ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = cnt,
                    data = dic
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
