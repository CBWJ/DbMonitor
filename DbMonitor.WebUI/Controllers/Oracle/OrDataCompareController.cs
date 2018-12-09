using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrDataCompareController : BaseController
    {
        // GET: OrDataCompare
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            
            List<Domain.Dictionary> dic = null;
            var sc = db.SessionConnection.Find(id);
            if(sc.SCDBType == "ORACLE")
            {
                using (var dal = new OracleDAL(GetSessionConnStr(id)))
                {
                    ViewBag.Users = dal.GetAllUsers().OrderBy(s => s).ToList();
                }
                dic = db.Dictionary.Where(d => d.DTypeCode == "OracleCompare").ToList();
            }
            else
            {
                using (var dal = new DmDAL(GetSessionConnStr(id)))
                {
                    ViewBag.Users = dal.GetAllUsers().OrderBy(s => s).ToList();
                }
                dic = db.Dictionary.Where(d => d.DTypeCode == "DmCompare").ToList();
            }
            return View(dic);
        }

        /// <summary>
        /// 获取对象名
        /// </summary>
        /// <param name="scId"></param>
        /// <param name="user"></param>
        /// <param name="objtype"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetObjectName(long scId, string user, string objtype)
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                var sc = db.SessionConnection.Find(scId);
                if (sc.SCDBType == "ORACLE")
                    using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                    {
                        objs = dal.GetObjectName(user, objtype).OrderBy(s=>s).ToList();
                    }
                else
                {
                    using (DmDAL dal = new DmDAL(GetSessionConnStr(scId)))
                    {
                        objs = dal.GetObjectName(user, objtype).OrderBy(s => s).ToList();
                    }
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    data = objs
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
        /// 表比较
        /// </summary>
        /// <param name="scId"></param>
        /// <param name="user"></param>
        /// <param name="objtype"></param>
        /// <param name="objname"></param>
        /// <param name="begtime"></param>
        /// <param name="endtime"></param>
        /// <returns></returns>
        public ActionResult CompareTable(long scId, string user, string objtype, string objname, string begtime, string endtime)
        {
            var log = db.ChangeLog.Where(l => l.SCID == scId && l.CLSchema.Contains(user) 
                && l.CLObjectType.Contains(objtype) && l.CLObjectName.Contains(objname)).ToList();
            if (!string.IsNullOrWhiteSpace(begtime))
            {
                var beg = DateTime.Parse(begtime);
                log = log.Where(l => DateTime.Parse(l.CLChangeTime) >= beg).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endtime))
            {
                var end = DateTime.Parse(endtime);
                log = log.Where(l => DateTime.Parse(l.CLChangeTime) < end).ToList();
            }
            return View(log);
        }

        public ActionResult CompareNormalObject(long scId, string user, string objtype, string objname, string begtime, string endtime)
        {
            var log = db.ChangeLog.Where(l => l.SCID == scId && l.CLSchema.Contains(user)
                && l.CLObjectType.Contains(objtype) && l.CLObjectName.Contains(objname)).ToList();
            if (!string.IsNullOrWhiteSpace(begtime))
            {
                var beg = DateTime.Parse(begtime);
                log = log.Where(l => DateTime.Parse(l.CLChangeTime) >= beg).ToList();
            }
            if (!string.IsNullOrWhiteSpace(endtime))
            {
                var end = DateTime.Parse(endtime);
                log = log.Where(l => DateTime.Parse(l.CLChangeTime) < end).ToList();
            }
            return View(log);
        }
    }
}