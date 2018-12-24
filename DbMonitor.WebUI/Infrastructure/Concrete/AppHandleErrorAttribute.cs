using DbMonitor.WebUI.Controllers;
using DbMonitor.WebUI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    public class AppHandleErrorAttribute : HandleErrorAttribute
    {
        public override void OnException(ExceptionContext filterContext)
        {
            int code = HttpContext.Current.Response.StatusCode;
            Exception error = filterContext.Exception;
            var message = error.Message;
            //设置为true，则说明过滤器处理了该异常
            filterContext.ExceptionHandled = true;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("code", code.ToString());
            dic.Add("msg", message);
            
            filterContext.Result = new ViewResult
            {
                ViewName = "Exception",
                ViewData = new ViewDataDictionary(dic)
            };
            LogHelper.WriteErrorFormFilter(filterContext.Exception);
        }
    }
}