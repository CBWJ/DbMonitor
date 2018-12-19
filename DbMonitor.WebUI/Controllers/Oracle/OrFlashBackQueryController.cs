using DbMonitor.DBAccess.Concrete;
using DbMonitor.DBAccess.Extensions;
using DbMonitor.WebUI.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrFlashBackQueryController : BaseController
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
        
        public ActionResult GetColumnName(long scId, string user, string objname)
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
        public ActionResult List(long scId, string user, string objname, string endtime, int page = 1, int limit = 20)
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            try
            {
                StringBuilder sbCount = new StringBuilder();
                StringBuilder sbSql = new StringBuilder();
                string tv = string.Format("{0}.{1}", user, objname);
                if (string.IsNullOrWhiteSpace(endtime))
                {
                    endtime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                sbCount.AppendFormat("SELECT COUNT(1) FROM {0} AS OF TIMESTAMP TO_TIMESTAMP('{1}','YYYY-MM-DD hh24:mi:ss')", tv, endtime);
                sbSql.AppendFormat("SELECT * FROM (SELECT ROWNUM AS ROWNO, t.* FROM {0} AS OF TIMESTAMP TO_TIMESTAMP('{1}','YYYY-MM-DD hh24:mi:ss') t WHERE ", tv, endtime);

                
                sbSql.AppendFormat("ROWNUM <= {0}) table_alias WHERE table_alias.ROWNO > {1}",
                    page * limit, (page - 1) * limit);

                int count = 0;
                DataTable dt = null;
                using ( OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    count = Convert.ToInt32(dal.ExecuteScalar(sbCount.ToString()));
                    dt = dal.ExecuteQuery(sbSql.ToString());
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = "",
                    total = count,
                    data = dt
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