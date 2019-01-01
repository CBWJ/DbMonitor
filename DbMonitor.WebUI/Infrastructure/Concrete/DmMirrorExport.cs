using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbMonitor.WebUI.Infrastructure.Abstract;
using System.Text;
using DbMonitor.WebUI.Utility;
using System.IO;
using System.Text.RegularExpressions;
using DbMonitor.Domain;
using DbMonitor.DBAccess.Concrete;

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    public class DmMirrorExport : IMirrorExport
    {
        private string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["LocalDm"].ConnectionString;
        public void ExecuteExport(long id)
        {
            try
            {
                MirrorExport me = null;
                List<Dictionary> dic = null;
                SessionConnection sc = null;
                using (var ctx = new DbMonitorEntities())
                {
                    me = ctx.MirrorExport.Find(id);
                    dic = ctx.Dictionary.Where(d => d.DTypeCode == "DmExport" && d.DEnable == 1).ToList();
                    sc = ctx.SessionConnection.Find(me.SCID);

                    me.MEStatus = "开始导出";
                    me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    ctx.SaveChanges();
                    /*
                    List<string> cmds = new List<string>();
                    cmds.Add(string.Format("set DM_HOME={0}", dic.Where(d => d.DCode == "dm_home").First().DName));
                    cmds.Add(@"set PATH=%path%;%DM_HOME%\bin");

                    var backup_dir = dic.Where(d => d.DCode == "backup_dir").FirstOrDefault().DName;

                    //使用StringBuilder注意参数之间的空格
                    StringBuilder sbExp = new StringBuilder();
                    sbExp.AppendFormat("%DM_HOME%\\bin\\dexp USERID={0}/{1}@{2}:{3} ",
                        me.MEUser, me.MEPassword, sc.SCHostName, sc.SCPort);
                    sbExp.AppendFormat("file={0} log={1} directory={2} schemas={3}",
                        me.MEFileName, me.MELogFile, backup_dir, me.MESchemas);
                    cmds.Add(sbExp.ToString());
                    CmdHelper.Execute(cmds.ToArray());

                    var logfile = Path.Combine(backup_dir, me.MELogFile);
                    bool bExportOK = false;
                    if (File.Exists(logfile))
                    {
                        string text = File.ReadAllText(logfile, EncodingType.GetType(logfile));
                        Console.WriteLine(text);

                        string pattern = @"共导出[\s\d]+个SCHEMA";                        
                        if (Regex.IsMatch(text, pattern))
                        {
                            bExportOK = true;
                        }
                    }                    
                    if (bExportOK)
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

                    //导入到本地
                    var local_user = dic.Where(d => d.DCode == "user").FirstOrDefault().DName;
                    var local_pwd = dic.Where(d => d.DCode == "pwd").FirstOrDefault().DName;
                    */
                    var home = dic.Where(d => d.DCode == "dm_home").First().DName;
                    var backup_dir = dic.Where(d => d.DCode == "backup_dir").FirstOrDefault().DName;
                    var exp_log1 = me.MELogFile.Replace(".log", "_exp1.log");
                    //使用StringBuilder注意参数之间的空格
                    StringBuilder sbExp = new StringBuilder();
                    sbExp.AppendFormat("%DM_HOME%\\bin\\dexp USERID={0}/{1}@{2}:{3} ",
                        me.MEUser, me.MEPassword, sc.SCHostName, sc.SCPort);
                    sbExp.AppendFormat("file={0} log={1} directory={2} schemas={3}",
                        me.MEFileName, exp_log1, backup_dir, me.MESchemas);

                    bool bExportOK = ExportTask(home, backup_dir, exp_log1, sbExp.ToString());
                    if (bExportOK)
                    {
                        me.MEStatus = "远程导出成功";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();

                        //导入到本地
                       /* var local_user = dic.Where(d => d.DCode == "user").FirstOrDefault().DName;
                        var local_pwd = dic.Where(d => d.DCode == "pwd").FirstOrDefault().DName;
                        var imp_log = me.MELogFile.Replace(".log", "_exp2.log");
                        using (var dal = new DmDAL(connStr))
                        {
                            var sql = string.Format("SELECT COUNT(1) FROM DBA_USERS WHERE USERNAME='{0}'", me.MESchemas);
                            var cnt = Convert.ToInt32(dal.ExecuteScalar(sql));
                            if(cnt == 1)
                            {
                                sql = string.Format("DROP USER {0} CASCADE", me.MESchemas);
                                dal.ExecuteNonQuery(sql);
                                
                            }
                            sql = string.Format("create user {0} identified by {0} default tablespace BACKUP", me.MESchemas);
                            dal.ExecuteNonQuery(sql);
                        }
                        sbExp.Clear();
                        sbExp.AppendFormat("%DM_HOME%\\bin\\dimp USERID={0}/{1} FILE={2} LOG={3} DIRECTORY={4}",
                           local_user, local_pwd, me.MEFileName, imp_log, backup_dir);

                        me.MEImportStatus = "开始自动导入";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();

                        bool bImportOK = false;
                        bImportOK = ImportTask(home, backup_dir, imp_log, sbExp.ToString());
                        if (bImportOK)
                        {
                            me.MEImportStatus = "自动导入成功";
                            me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            ctx.SaveChanges();
                        }
                        else
                        {
                            me.MEImportStatus = "自动导入失败";
                            me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            ctx.SaveChanges();
                        }*/
                    }
                    else
                    {
                        me.MEStatus = "远程导出失败";
                        me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex, string.Format("导出达梦数据库，会话ID：{0}", id));
            }
        }

        public void ExecuteImport(long id)
        {
            using (var ctx = new DbMonitorEntities())
            {
                var me = ctx.MirrorExport.Find(id);
                var dic = ctx.Dictionary.Where(d => d.DTypeCode == "DmExport" && d.DEnable == 1).ToList();

                //导入到本地
                var home = dic.Where(d => d.DCode == "dm_home").First().DName;
                var backup_dir = dic.Where(d => d.DCode == "backup_dir").FirstOrDefault().DName;
                var local_user = dic.Where(d => d.DCode == "user").FirstOrDefault().DName;
                var local_pwd = dic.Where(d => d.DCode == "pwd").FirstOrDefault().DName;
                var imp_log = me.MELogFile.Replace(".log", "_imp.log");
                using (var dal = new DmDAL(connStr))
                {
                    var sql = string.Format("SELECT COUNT(1) FROM DBA_USERS WHERE USERNAME='{0}'", me.MESchemas);
                    var cnt = Convert.ToInt32(dal.ExecuteScalar(sql));
                    if (cnt == 1)
                    {
                        sql = string.Format("DROP USER {0} CASCADE", me.MESchemas);
                        dal.ExecuteNonQuery(sql);

                    }
                    sql = string.Format("create user {0} identified by {0} default tablespace BACKUP", me.MESchemas);
                    dal.ExecuteNonQuery(sql);
                }
                StringBuilder sbExp = new StringBuilder();
                sbExp.AppendFormat("%DM_HOME%\\bin\\dimp USERID={0}/{1} FILE={2} LOG={3} DIRECTORY={4}",
                   local_user, local_pwd, me.MEFileName, imp_log, backup_dir);

                me.MEImportStatus = "开始导入";
                me.MEImportTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                ctx.SaveChanges();

                bool bImportOK = false;
                bImportOK = ImportTask(home, backup_dir, imp_log, sbExp.ToString());
                if (bImportOK)
                {
                    me.MEImportStatus = "导入成功";
                    me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    ctx.SaveChanges();
                }
                else
                {
                    me.MEImportStatus = "导入失败";
                    me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    ctx.SaveChanges();
                }
            }
        }

        public bool ImportTask(string home, string backup_dir, string log, string cmd)
        {
            bool bSuccess = false;
            List<string> cmds = new List<string>();
            cmds.Add(string.Format("set DM_HOME={0}", home));
            cmds.Add(@"set PATH=%path%;%DM_HOME%\bin");
            cmds.Add(cmd);
            CmdHelper.Execute(cmds.ToArray());
            var logfile = Path.Combine(backup_dir, log);
            if (File.Exists(logfile))
            {
                string text = File.ReadAllText(logfile, EncodingType.GetType(logfile));
                Console.WriteLine(text);

                string pattern = @"导入成功[\s\S]+整个导入过程共花费";
                if (Regex.IsMatch(text, pattern))
                {
                    bSuccess = true;
                }
            }
            return bSuccess;
        }
        public bool ExportTask(string home, string backup_dir, string log, string cmd)
        {
            bool bSuccess = false;
            List<string> cmds = new List<string>();
            cmds.Add(string.Format("set DM_HOME={0}", home));
            cmds.Add(@"set PATH=%path%;%DM_HOME%\bin");
            cmds.Add(cmd);
            CmdHelper.Execute(cmds.ToArray());
            var logfile = Path.Combine(backup_dir, log);
            if (File.Exists(logfile))
            {
                string text = File.ReadAllText(logfile, EncodingType.GetType(logfile));
                Console.WriteLine(text);

                string pattern = @"共导出[\s\d]+个SCHEMA";
                if (Regex.IsMatch(text, pattern))
                {
                    bSuccess = true;
                }
            }
            return bSuccess;
        }
    }
}