using DbMonitor.DBAccess.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrMonitorSwitchController : BaseController
    {
        // GET: OrMonitorSwitch
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();

            var mm = db.MonitorManagement.Where(m => m.SCID == id).FirstOrDefault();
            //不存在则添加
            if(mm == null)
            {
                mm = db.MonitorManagement.Create();
                mm.SCID = id;
                mm.MMCycleUnit = "s";
                mm.MMRefreshCycle = 30;
                mm.MMOpen = 0;
                mm.MMLastTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                db.MonitorManagement.Add(mm);
                db.SaveChanges();
            }
            ViewBag.ID = mm.ID;
            return View(mm);
        }
        [HttpPost]
        public ActionResult Edit(Domain.MonitorManagement manage, FormCollection collection)
        {
            if(manage.MMOpen == 1)
            {
                JsonResult ret = new JsonResult();
                try
                {
                    //加载数据
                    var dmExample = db.Dictionary.Where(d => d.DTypeCode == "DmExample" && d.DEnable == 1).ToList();
                    var sql = "";
                    using (var dal = new OracleDAL(GetSessionConnStr(manage.SCID.Value)))
                    {
                        sql = dmExample.Where(d => d.DCode == "临时表").FirstOrDefault().DName;
                        dal.ExecuteNonQuery(sql);
                        sql = dmExample.Where(d => d.DCode == "包说明").FirstOrDefault().DName;
                        dal.ExecuteNonQuery(sql);
                        sql = dmExample.Where(d => d.DCode == "包体").FirstOrDefault().DName;
                        dal.ExecuteNonQuery(sql);
                    }
                }
                catch (Exception ex)
                {
                    ret.Data = JsonConvert.SerializeObject(new
                    {
                        status = 1,
                        message = ex.Message
                    });
                    RecordException(ex);
                    return ret;
                }
            }
            return EditModel(manage);
        }
    }
}