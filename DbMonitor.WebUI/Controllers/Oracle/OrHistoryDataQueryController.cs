using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrHistoryDataQueryController : BaseController
    {
        // GET: OrHistoryDataQuery
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            using(var dal = new OracleDAL(GetSessionConnStr(id)))
            {
                ViewBag.Users = dal.GetAllUsers();
            }
            return View();
        }
        
        public ActionResult GetColumnName(long scId, string user, string objname, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                List<string> colNames = new List<string>();
                if(!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(objname))
                {
                    string sql = string.Format("SELECT * FROM {0}.{1} WHERE ROWNUM <= 1", user, objname);
                    DataTable dt = null;
                    using(var dal = new OracleDAL(GetSessionConnStr(scId)))
                    {
                        dt = dal.ExecuteQuery(sql);
                    }
                    if(dt != null)
                    {
                        foreach(DataColumn col in dt.Columns)
                        {
                            colNames.Add(col.ColumnName);
                        }
                    }
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    data = colNames
                }); ;
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = "发生异常：" + ex.Message,
                    data = ""
                });
                RecordException(ex);
            }
            return ret;
        }
        //SELECT * FROM STUDENT AS OF TIMESTAMP TO_TIMESTAMP('2018-12-19 16:00:16','YYYY-MM-DD hh24:mi:ss')
        public ActionResult List(long scId, string user, string objname, string begtime, string endtime, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                var log = db.ChangeLog.Where(m => m.SCID == scId).ToList();
                if (!string.IsNullOrWhiteSpace(user))
                {
                    log = log.Where(l => l.CLSchema.Contains(user.ToUpper())).ToList();
                }
                if (!string.IsNullOrWhiteSpace(objname))
                {
                    log = log.Where(l => l.CLObjectName.Contains(objname.ToUpper())).ToList();
                }
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
                int cnt = log.Count;
                log = log.OrderBy(l => l.CLChangeTime).Skip((page - 1) * limit).Take(limit).ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = cnt,
                    data = log
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