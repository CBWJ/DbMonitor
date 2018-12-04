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

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    public class DmMirrorExport : IMirrorExport
    {
        public void ExecuteExport(long id)
        {
            using (var ctx = new DbMonitorEntities())
            {
                var me = ctx.MirrorExport.Find(id);
                var dic = ctx.Dictionary.Where(d => d.DTypeCode == "DmExport" && d.DEnable == 1).ToList();
                var sc = ctx.SessionConnection.Find(me.SCID);

                me.MEStatus = "开始导出";
                me.EditingTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                ctx.SaveChanges();

                List<string> cmds = new List<string>();
                cmds.Add(string.Format("set DM_HOME={0}", dic.Where(d=>d.DCode== "dm_home").First().DName));
                cmds.Add(@"set PATH=%path%;%DM_HOME%\bin");

                var backup_dir = dic.Where(d => d.DCode == "backup_dir").FirstOrDefault().DName;

                //使用StringBuilder注意参数之间的空格
                StringBuilder sbExp = new StringBuilder();
                sbExp.AppendFormat("%DM_HOME%\\bin\\dexp USERID={0}/{1}@{2}:{3} ",
                    me.MEUser, me.MEPassword, sc.SCHostName, sc.SCPort);
                sbExp.AppendFormat("file={0} log={1} directory={2} owner={3}",
                    me.MEFileName, me.MELogFile, backup_dir, me.MESchemas);
                cmds.Add(sbExp.ToString());
                CmdHelper.Execute(cmds.ToArray());

                var logfile = Path.Combine(backup_dir, me.MELogFile);

                if (File.Exists(logfile))
                {
                    string text = File.ReadAllText(logfile, EncodingType.GetType(logfile));
                    Console.WriteLine(text);

                    string pattern = @"/成功导出/";

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