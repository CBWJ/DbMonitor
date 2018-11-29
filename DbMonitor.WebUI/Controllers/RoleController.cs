using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class RoleController : BaseController
    {
        // GET: Role
        public ActionResult Index()
        {
            SetModuleAuthority();
            return View();
        }

        // GET: Role/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: Role/Create
        public ActionResult Create()
        {
            CreateAcion();
            Domain.Role r = new Domain.Role();
            return View(r);
        }

        // POST: Role/Create
        [HttpPost]
        public ActionResult Create(Domain.Role role, FormCollection collection)
        {
            return CreateModel(role);
            /*
            JsonResult ret = new JsonResult();
            try
            {
                role.CreatorID = (int)LoginUser.ID;
                role.CreationTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

                db.Role.Add(role);
                db.SaveChanges();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
            }
            return ret;*/
        }

        // GET: Role/Edit/5
        public ActionResult Edit(int id)
        {
            EditAcion();
            var r = db.Role.Find(id);
            return View("Create", r);
        }

        // POST: Role/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.Role role, FormCollection collection)
        {
            return EditModel(role);
            //JsonResult ret = new JsonResult();
            //try
            //{
            //    var editRole = db.Role.Find(role.ID);

            //    editRole.RName = role.RName;
            //    editRole.RCode = role.RCode;
            //    editRole.EditorID = (int)LoginUser.ID;
            //    editRole.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                
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

        // GET: Role/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Role/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {
            return DeleteModel<Domain.Role>(idList);
            //JsonResult ret = new JsonResult();
            //try
            //{
            //    foreach (var id in idList)
            //    {
            //        var r = db.Role.Find(id);
            //        db.Role.Remove(r);
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
                    total = db.Role.Count(),
                    data = db.Role
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

        public ActionResult Athorize(int id)
        {
            //可以授权的所有模块
            ViewBag.AllAuthModule = (from m in db.v_RoleAuth
                                     select m.MName).Distinct().ToList();
            ViewBag.RID = id;
            ViewBag.OwnMAID = (from m in db.RoleAuthority
                               where m.RID == id
                               select m.MAID.Value).ToList();
            return View(db.v_RoleAuth.ToList());
        }
        [HttpPost]
        public ActionResult Athorize(int rId, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //对于只提交选中项的问题，全部删除，再增加
                foreach(var ma in db.ModuleAuthority)
                {
                    var exsit = (from ra in db.RoleAuthority
                                 where ra.RID == rId && ra.MAID == ma.ID
                                 select ra).ToList();
                    if (exsit != null)
                        db.RoleAuthority.RemoveRange(exsit);

                    var param = collection[string.Format("ma{0}", ma.ID)];
                    if(param == "1")
                    {
                        db.RoleAuthority.Add(new Domain.RoleAuthority {
                            MAID = ma.ID,
                            RID = rId
                        });
                    }
                }
                db.SaveChanges();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
            }
            return ret;
        }
    }
}
