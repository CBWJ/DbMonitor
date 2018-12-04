using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using DbMonitor.WebUI.Infrastructure.Concrete;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrMirrorExportController : BaseController
    {
        // GET: OrMirrorExport
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }

        public ActionResult List(long scId, int page = 1, int limit = 20, string option = "")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                var exports = db.MirrorExport.OrderBy(m => m.CreationTime)
                        .Skip((page - 1) * limit)
                        .Take(limit)
                        .ToList();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = exports.Count,
                    data = exports
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

        public ActionResult Create(long scId)
        {
            CreateAcion();
            var sc = db.SessionConnection.Find(scId);
            var dic = db.Dictionary.Where(d => d.DTypeCode == "OracleExport" && d.DCode == "backup_dir").FirstOrDefault();
            Domain.MirrorExport me = new Domain.MirrorExport()
            {
                SCID = scId,
                MEUser = sc.SCUser,
                MEPassword = sc.SCPassword,
                MEDirectory = dic.DName,
                MESchemas = sc.SCUser,
                MEExportTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")
            };
            return View(me);
        }
        [HttpPost]
        public ActionResult Create(Domain.MirrorExport mirror, FormCollection collection)
        {
            JsonResult ret = new JsonResult();
            try
            {
                var sc = db.SessionConnection.Find(mirror.SCID);
                //主机名_模式_日期时间
                var pureName = string.Format("{0}_{1}_{2}", sc.SCHostName, mirror.MESchemas,
                    mirror.MEExportTime.Replace("-", "").Replace(":","").Replace(" ",""));
                mirror.MEFileName = string.Format("{0}.dmp", pureName);
                mirror.MELogFile = string.Format("{0}.log",pureName);

                mirror.CreatorID = LoginUser.ID;
                mirror.CreationTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                mirror.MEStatus = "创建导出任务";

                db.MirrorExport.Add(mirror);
                db.SaveChanges();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });

                Task.Factory.StartNew(new Action(()=> {
                    OracleMirrorExport omr = new OracleMirrorExport();
                    omr.ExecuteExport(mirror.ID);
                }));
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