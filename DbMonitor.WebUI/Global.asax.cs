using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;


namespace DbMonitor.WebUI
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            //RegisterView();
            Utility.DataGraber.Start();
        }

        private void RegisterView()
        {
            ViewEngines.Engines.Clear();//移除默认视图配置
            ViewEngines.Engines.Add(new Infrastructure.Concrete.ViewEngineEx());
        }
    }
}
