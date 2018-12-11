using DbMonitor.DBAccess.Concrete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DbMonitor.WebUI.Extensions;
using Oracle.ManagedDataAccess.Client;
using DbMonitor.DBAccess.Extensions;

namespace DbMonitor.WebUI.Controllers.Oracle
{
    public class OrAuditPolicyController : BaseController
    {
        // GET: OrStatementAudit
        public ActionResult Index(long id)
        {
            ViewBag.SCID = id;
            SetModuleAuthority();
            return View();
        }

        public ActionResult List(long scId, int page = 1, int limit = 20, string user = "", string obj = "", string policy="")
        {
            JsonResult ret = new JsonResult();
            ret.JsonRequestBehavior = JsonRequestBehavior.AllowGet;

            try
            {
                StringBuilder sbCount = new StringBuilder();
                StringBuilder sbSql = new StringBuilder();
                string tv = "DBA_AUDIT_POLICIES";
                sbCount.AppendFormat("SELECT COUNT(1) FROM {0} ", tv);
                sbSql.AppendFormat("SELECT * FROM (SELECT ROWNUM AS ROWNO, t.* FROM {0} t WHERE ", tv);
                //筛选条件
                if (!string.IsNullOrWhiteSpace(user))
                {
                    sbSql.AppendFormat("OBJECT_SCHEMA LIKE '%{0}%' AND ", user.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_SCHEMA LIKE '%{0}%'", user.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(obj))
                {
                    sbSql.AppendFormat("OBJECT_NAME LIKE '%{0}%' AND ", obj.ToUpper());
                    sbCount.AddCondition(string.Format("OBJECT_NAME LIKE '%{0}%'", obj.ToUpper()));
                }
                if (!string.IsNullOrWhiteSpace(policy))
                {
                    sbSql.AppendFormat("POLICY_NAME LIKE '%{0}%' AND ", policy.ToUpper());
                    sbCount.AddCondition(string.Format("POLICY_NAME LIKE '%{0}%'", policy.ToUpper()));
                }
                sbSql.AppendFormat("ROWNUM <= {0}) table_alias WHERE table_alias.ROWNO > {1}",
                    page * limit, (page - 1) * limit);

                int count = 0;
                DataTable dt = null;
                //string connStr = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=ORCL)));Persist Security Info=True;User ID=sys;Password=sys;DBA Privilege=SYSDBA;";
                string connStr = GetSessionConnStr(scId);
                using (OracleDAL dal = new OracleDAL(connStr))
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
            }
            return ret;
        }

        public ActionResult Create(long scId)
        {            
            //用户
            using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
            {
                ViewBag.User = dal.GetAllUsers();
            }
            return View(scId);
        }
        
        [HttpPost]
        public ActionResult Create(long scId, string policyname, string user, string obj, string col, string condition, string sel, string ins, string upd, string del)
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<OracleParameter> policyParams = new List<OracleParameter>();

                //顺序不能乱
                /*policyParams.Add(new OracleParameter("object_schema", user));
                policyParams.Add(new OracleParameter("object_name", obj));
                policyParams.Add(new OracleParameter("policy_name", policyname));
                policyParams.Add(new OracleParameter("audit_condition", condition));
                policyParams.Add(new OracleParameter("audit_column", col));

                policyParams.Add(new OracleParameter("handler_schema", OracleDbType.Varchar2, "", ParameterDirection.Input));
                policyParams.Add(new OracleParameter("handler_module", OracleDbType.Varchar2, "", ParameterDirection.Input));*/
                /*policyParams.Add(new OracleParameter("enable", OracleDbType.Boolean, true, ParameterDirection.Input));
                List<string> statement = new List<string>();

                if (sel == "on")
                {
                    statement.Add("SELECT");
                }
                if (ins == "on")
                {
                    statement.Add("INSERT");
                }
                if (upd == "on")
                {
                    statement.Add("UPDATE");
                }
                if (del == "on")
                {
                    statement.Add("DELETE");
                }
                if (statement.Count > 0)
                {
                    policyParams.Add(new OracleParameter("statement_types", string.Join(",", statement.ToArray())));
                }
                else
                {
                    policyParams.Add(new OracleParameter("statement_types", OracleDbType.Varchar2, "NULL", ParameterDirection.Input));
                }
                */                
                var arrParam = policyParams.ToArray();
                List<string> statement = new List<string>();

                if (sel == "on")
                {
                    statement.Add("SELECT");
                }
                if (ins == "on")
                {
                    statement.Add("INSERT");
                }
                if (upd == "on")
                {
                    statement.Add("UPDATE");
                }
                if (del == "on")
                {
                    statement.Add("DELETE");
                }
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("begin DBMS_FGA.ADD_POLICY (");
                sbSql.AppendFormat("object_schema      =>  '{0}', ", user);
                sbSql.AppendFormat("object_name        =>  '{0}', ", obj);
                sbSql.AppendFormat("policy_name        =>  '{0}', ", policyname);
                sbSql.AppendFormat("audit_condition    =>  '{0}', ", condition);
                sbSql.AppendFormat("audit_column       =>  '{0}', ", col);
                sbSql.AppendFormat("handler_schema     =>   NULL, ");
                sbSql.AppendFormat("handler_module     =>   NULL, ");
                sbSql.AppendFormat("enable             =>   TRUE, ");
                sbSql.AppendFormat("statement_types    =>  '{0}'); ", string.Join(",", statement.ToArray()));
                sbSql.AppendFormat("end;");
                using (var dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    //dal.ExecuteProcedureNonQuery("dbms_fga.add_policy", arrParam);
                    dal.ExecuteNonQuery(sbSql.ToString());
                }
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
        public ActionResult EnablePolicy(long scId, string user, string objname, string policy, bool enable)
        {
            JsonResult ret = new JsonResult();
            try
            {
                var proc = "dbms_fga.disable_policy";
                if(enable)
                    proc = "dbms_fga.enable_policy";
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    dal.ExecuteProcedureNonQuery(proc,
                    new OracleParameter("object_schema", user),
                    new OracleParameter("object_name", objname),
                    new OracleParameter("policy_name", policy));
                }
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
        [HttpPost]
        public ActionResult Delete(long scId, string user, string objname,string policy)
        {
            JsonResult ret = new JsonResult();
            try
            {
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    dal.ExecuteProcedureNonQuery("dbms_fga.drop_policy",
                    new OracleParameter("object_schema", user),
                    new OracleParameter("object_name", objname),
                    new OracleParameter("policy_name", policy));
                }
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
        /// 获取对象名
        /// </summary>
        /// <param name="scId"></param>
        /// <param name="user"></param>
        /// <param name="objtype"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetObjectName(long scId, string user, string objtype = "TABLE")
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("select object_name from dba_objects where owner='{0}' and object_type = '{1}'",
                    user, objtype);
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    DataTable dt = dal.ExecuteQuery(sbSql.ToString());

                    foreach (DataRow row in dt.Rows)
                    {
                        var u = row.ItemArray[0].ToString();
                        objs.Add(u);
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
        /// 获取某个对象的所有列名
        /// </summary>
        /// <param name="scId"></param>
        /// <param name="user"></param>
        /// <param name="objname"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetColumnName(long scId, string user, string objname)
        {
            JsonResult ret = new JsonResult();
            try
            {
                List<string> objs = new List<string>();
                //查询语句结尾不要逗号，否则报错:ORA-00911: 无效字符
                StringBuilder sbSql = new StringBuilder();
                sbSql.AppendFormat("select column_name from dba_tab_columns where owner='{0}' and TABLE_NAME = '{1}'",
                    user, objname);
                using (OracleDAL dal = new OracleDAL(GetSessionConnStr(scId)))
                {
                    DataTable dt = dal.ExecuteQuery(sbSql.ToString());

                    foreach (DataRow row in dt.Rows)
                    {
                        var u = row.ItemArray[0].ToString();
                        objs.Add(u);
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
    }
}