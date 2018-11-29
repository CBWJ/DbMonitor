using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbMonitor.Domain;
using System.Web.Security;
using Newtonsoft.Json;

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

        protected ActionResult CreateModel<T>(T model)
        {
            JsonResult ret = new JsonResult();
            try
            {

                var t = model.GetType();
                //设置模型值
                var prop = t.GetProperty("CreatorID");
                prop.SetValue(model, LoginUser.ID);
                prop = t.GetProperty("CreationTime");
                prop.SetValue(model, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));


                var tDB = db.GetType();
                //获取DbSet属性
                var modelSet = tDB.GetProperty(t.Name);
                //属性实例
                var modelSetInst = modelSet.GetValue(db);
                //获取Add方法
                var mAdd = modelSet.PropertyType.GetMethod("Add");
                //用实例调用Add
                mAdd.Invoke(modelSetInst, new object[] { model });

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

        protected ActionResult EditModel<T>(T model)
        {
            JsonResult ret = new JsonResult();
            try
            {

                var t = model.GetType();
                //设置模型值
                var prop = t.GetProperty("EditorID");
                prop.SetValue(model, LoginUser.ID);
                prop = t.GetProperty("EditingTime");
                prop.SetValue(model, DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss"));

                //获取ID
                prop = t.GetProperty("ID");
                var id = prop.GetValue(model);
                var tDB = db.GetType();
                //获取DbSet属性
                var modelSet = tDB.GetProperty(t.Name);
                //属性实例
                var modelSetInst = modelSet.GetValue(db);
                //获取Find方法
                var mFind = modelSet.PropertyType.GetMethod("Find");
                //用实例调用
                var editModel = mFind.Invoke(modelSetInst, new object[] { new object[] { id } });

                //修改Entity
                string[] expectProp = { "ID", "CreatorID", "CreationTime" };
                foreach(var p in t.GetProperties())
                {
                    if (!expectProp.Contains(p.Name))
                    {
                        var value = p.GetValue(model);
                        p.SetValue(editModel, value);
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

        protected ActionResult DeleteModel<T>(List<int> idList)
        {
            JsonResult ret = new JsonResult();
            try
            {

                var t = typeof(T);
                
                var tDB = db.GetType();
                //获取DbSet属性
                var modelSet = tDB.GetProperty(t.Name);
                //属性实例
                var modelSetInst = modelSet.GetValue(db);
                //获取Find方法
                var mFind = modelSet.PropertyType.GetMethod("Find");
                //获取Remove方法
                var mRemove = modelSet.PropertyType.GetMethod("Remove");
                foreach (var id in idList)
                {
                    //用实例调用
                    var delModel = mFind.Invoke(modelSetInst, new object[] { new object[] { id } });
                    mRemove.Invoke(modelSetInst, new object[] { delModel});
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
        /// <summary>
        /// 新建条目
        /// </summary>
        protected void CreateAcion()
        {
            ViewBag.Action = "Create";
        }
        /// <summary>
        /// 编辑条目
        /// </summary>
        protected void EditAcion()
        {
            ViewBag.Action = "Edit";
        }
        /// <summary>
        /// 设置模块权限
        /// </summary>
        protected void SetModuleAuthority()
        {
            var controller = RouteData.Values["controller"];
            var action = RouteData.Values["action"];
            string url = string.Format("{0}/{1}", controller, action);
            long mId = (from m in db.Module
                        where m.MUrl == url
                        select m.ID).FirstOrDefault();
            List<Authority> auths = null;
            if (LoginUser.UUserType < 2)
            {
                auths = (from ma in db.ModuleAuthority
                         join a in db.Authority on ma.AID equals a.ID
                         where ma.MID == mId
                         select a).ToList();
            }
            else
            {                
                auths = (from ur in db.UserRole
                              join r in db.Role on ur.RID equals r.ID
                              join ra in db.RoleAuthority on r.ID equals ra.RID
                              join ma in db.ModuleAuthority on ra.MAID equals ma.ID
                              join a in db.Authority on ma.AID equals a.ID
                              where ma.MID == mId
                              select a).ToList();

                
            }
            if (auths == null)
                auths = new List<Authority>();
            ViewBag.Authority = auths;
        }
        
    }
}