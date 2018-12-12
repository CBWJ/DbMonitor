using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;
using System.Text;

namespace DbMonitor.WebUI.Utility
{
    public class LogHelper
    {
        //Logger是直接和应用程序交互的组件,只是产生日志
        static ILog logger = LogManager.GetLogger(typeof(LogHelper));

        public static void WriteErrorFormFilter(Exception ex)
        {
            try
            {
                StringBuilder sbInfo = new StringBuilder();
                //sbInfo.Append("\r\n-----------------start----------------------\r\n");
                sbInfo.AppendFormat("请求地址：{0}\r\n", HttpContext.Current.Request.Url.AbsoluteUri);
                sbInfo.AppendFormat("请求类型：{0}\r\n", HttpContext.Current.Request.RequestType);
                sbInfo.AppendFormat("异常信息：{0}\r\n", ex);
                //sbInfo.Append("--------------------end---------------------\r\n");
                logger.Error(sbInfo);
            }
            catch
            {
                
            }
        }
    }
}