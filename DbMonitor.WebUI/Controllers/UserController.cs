using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace DbMonitor.WebUI.Controllers
{
    public class UserController : BaseController
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        // GET: User/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: User/Create
        public ActionResult Create()
        {
            DbMonitor.Domain.User u = new Domain.User();
            return View(u);
        }

        // POST: User/Create
        [HttpPost]
        public ActionResult Create(Domain.User user, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                user.CreatorID = (int)LoginUser.ID;
                user.CreationTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                if (user.UIsLock == null)
                    user.UIsLock = 0;
                db.User.Add(user);
                db.SaveChanges();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch(Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
            }
            return ret;
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            var user = db.User.Find(id);
            return View("Create", user);
        }

        // POST: User/Edit/5
        [HttpPost]
        public ActionResult Edit(Domain.User user, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //layui中check的input值为0时不提交数据
                if (user.UIsLock == null)
                    user.UIsLock = 0;

                var editUser = db.User.Find(user.ID);

                editUser.ULoginName = user.ULoginName;
                editUser.UPassword = user.UPassword;
                editUser.UName = user.UName;
                editUser.UUserType = user.UUserType;
                editUser.UIsLock = user.UIsLock;

                editUser.EditorID = (int)LoginUser.ID;
                editUser.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");

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

        // GET: User/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: User/Delete/5
        [HttpPost]
        public ActionResult Delete(List<int> idList)
        {
            JsonResult ret = new JsonResult();
            try
            {
                foreach(var id in idList)
                {
                    var u = db.User.Find(id);
                    db.User.Remove(u);
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
                    total = db.User.Count(),
                    data = db.User
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
