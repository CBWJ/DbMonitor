using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbMonitor.WebUI.Infrastructure.Abstract;
using DbMonitor.Domain;
using DbMonitor.DBAccess.Concrete;
using System.Configuration;
using System.Text;
using DbMonitor.WebUI.Utility;
using System.IO;
using System.Text.RegularExpressions;

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    public class OracleMirrorExport : IMirrorExport
    {
        private string connStr = ConfigurationManager.ConnectionStrings["LocalOracle"].ConnectionString;
        public void ExecuteExport(long id)
        {
            using(var ctx = new DbMonitorEntities())
            {
                var me = ctx.MirrorExport.Find(id);
                var dic = ctx.Dictionary.Where(d => d.DTypeCode == "OracleExport" && d.DEnable == 1).ToList();
                var sc = ctx.SessionConnection.Find(me.SCID);

                me.MEStatus = "开始导出";
                me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                ctx.SaveChanges();

                string dblink = string.Format("dblink{0}", id), 
                    user = me.MEUser, 
                    pwd = me.MEPassword,                    
                    host = sc.SCHostName, 
                    service = sc.SCServiceName;
                long port = sc.SCPort.Value;

                using (var dal = new OracleDAL(connStr))
                {
                    try
                    {
                        StringBuilder sbSql = new StringBuilder();
                        //存在就先删除
                        var cnt = Convert.ToInt32(dal.ExecuteScalar(string.Format("select count(1) from dba_db_links where db_link='{0}'", dblink.ToUpper())));
                        if (cnt > 0)
                            dal.ExecuteNonQuery(string.Format("drop public database link {0}", dblink.ToUpper()));

                        //再添加
                        sbSql.AppendFormat("create public database link {0} connect to {1} identified by {2} using",
                            dblink, user, pwd);
                        sbSql.AppendFormat("'(DESCRIPTION =(ADDRESS_LIST =(ADDRESS =(PROTOCOL = TCP)(HOST = {0})(PORT = {1})))(CONNECT_DATA =(SERVICE_NAME = {2})))'",
                            host, port, service);

                        dal.ExecuteNonQuery(sbSql.ToString());
                    }
                    catch(Exception ex)
                    {
                        me.MEStatus = "创建dblink失败";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();
                        return;
                    }
                }
                me.MEStatus = "创建dblink成功";
                me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                ctx.SaveChanges();

                List<string> cmds = new List<string>();
                //变量设置是必须的
                string directory = dic.Where(d => d.DCode == "directory").FirstOrDefault().DName,
                        oracle_base = dic.Where(d => d.DCode == "oracle_base").FirstOrDefault().DName,
                        oracle_home = dic.Where(d => d.DCode == "oracle_home").FirstOrDefault().DName,
                        oracle_sid = dic.Where(d => d.DCode == "oracle_sid").FirstOrDefault().DName,
                        user_local = dic.Where(d => d.DCode == "user").FirstOrDefault().DName,
                        pwd_local = dic.Where(d => d.DCode == "pwd").FirstOrDefault().DName;

                cmds.Add(string.Format("set ORACLE_BASE={0}", oracle_base));
                cmds.Add(string.Format("set ORACLE_HOME=%ORACLE_BASE%{0}", oracle_home));
                cmds.Add(string.Format("set ORACLE_SID={0}", oracle_sid));
                cmds.Add(@"set PATH=%path%;%ORACLE_HOME%\bin");

                string file = me.MEFileName, log = me.MELogFile, schemas = me.MESchemas, timestamp = me.MEExportTime;
                //使用StringBuilder注意参数之间的空格
                StringBuilder sbExp = new StringBuilder();
                sbExp.AppendFormat("%ORACLE_HOME%\\bin\\expdp {0}/{1} directory={2} dumpfile={3} logfile={4} network_link={5} schemas={6} ",
                    user_local, pwd_local, directory, file, log, dblink, schemas);
                sbExp.AppendFormat("flashback_time=\\\"to_timestamp('{0}','yyyy-mm-dd hh24:mi:ss')\\\"", timestamp);

                cmds.Add(sbExp.ToString());
                CmdHelper.Execute(cmds.ToArray());

                var backup_dir = dic.Where(d => d.DCode == "backup_dir").FirstOrDefault().DName;
                var logfile = Path.Combine(backup_dir, log);

                if (File.Exists(logfile))
                {
                    string text = File.ReadAllText(logfile, EncodingType.GetType(logfile));
                    Console.WriteLine(text);

                    string pattern = @"/\(于.*完成\)/";

                    var m = Regex.Match(text, pattern);
                    if (m != null)
                    {
                        me.MEStatus = "导出成功";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();
                    }
                    else
                    {
                        me.MEStatus = "导出失败";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();
                    }
                }
                else
                {
                    me.MEStatus = "导出失败";
                    me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    ctx.SaveChanges();
                }
            }
        }
    }
}