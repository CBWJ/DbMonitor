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
            SetModuleAuthority();
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
            CreateAcion();
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
                RecordException(ex);
            }
            return ret;
        }

        // GET: User/Edit/5
        public ActionResult Edit(int id)
        {
            EditAcion();
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
                RecordException(ex);
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
                RecordException(ex);
            }
            return ret;
        }

        public ActionResult AllocateRole(int id)
        {
            var roles = db.Role.ToList();
            ViewBag.UID = id;
            ViewBag.OwnRoleID = (from ur in db.UserRole
                                 join r in db.Role on ur.RID equals r.ID
                                 where ur.UID == id
                                 select r.ID).ToList();
                                
            return View(roles);
        }
        [HttpPost]
        public ActionResult AllocateRole(int uId, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //对于只提交选中项的问题，全部删除，再增加
                /*foreach (var ma in db.ModuleAuthority)
                {
                    var exsit = (from ra in db.RoleAuthority
                                 where ra.RID == rId && ra.MAID == ma.ID
                                 select ra).ToList();
                    if (exsit != null)
                        db.RoleAuthority.RemoveRange(exsit);

                    var param = collection[string.Format("ma{0}", ma.ID)];
                    if (param == "1")
                    {
                        db.RoleAuthority.Add(new Domain.RoleAuthority
                        {
                            MAID = ma.ID,
                            RID = rId
                        });
                    }
                }*/
                foreach(var r in db.Role)
                {
                    var exsit = (from ur in db.UserRole
                                 where ur.UID == uId && ur.RID == r.ID
                                 select ur).ToList();
                    if (exsit != null)
                        db.UserRole.RemoveRange(exsit);

                    var param = collection[string.Format("role{0}", r.ID)];
                    if (param == "1")
                    {
                        db.UserRole.Add(new Domain.UserRole
                        {
                            UID = uId,
                            RID = r.ID
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
                RecordException(ex);
            }
            return ret;
        }
    }
}
