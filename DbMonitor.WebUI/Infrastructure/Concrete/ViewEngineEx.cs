using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    public class ViewEngineEx : RazorViewEngine
    {
        public ViewEngineEx()
        {
            //{1}表示Controller的名称，{0}表示视图名称，Shared是存放模板页的文件夹
            ViewLocationFormats = new[]
            {
                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml",
                "~/Views/Oracle/{1}/{0}.cshtml",//我们的规则
                "~/Views/Dm/{1}/{0}.cshtml"
            };
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return base.FindView(controllerContext, viewName, masterName, useCache);
        }
    }
}