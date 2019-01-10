using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DbMonitor.Domain;
using System.Web.Security;
using DbMonitor.WebUI.Infrastructure.Abstract;

namespace DbMonitor.WebUI.Infrastructure.Concrete
{
    /// <summary>
    /// 表单认证器
    /// </summary>
    public class FormsAuthProvider : IAuthProvider
    {
        public bool Authenticate(string username, string password)
        {
            using (var context = new DbMonitorEntities())
            {
                var user = (from u in context.User
                            where u.ULoginName == username && u.UPassword == password
                            select u).FirstOrDefault();
                //用户存在
                if (user != null)
                {
                    return true;
                }
            }
            return false;
        }

        public void SetAuthCookie(HttpResponseBase response, string username)
        {
            if (response == null)
                return;
            //创建票据
            FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1, username,
                DateTime.Now, DateTime.Now.AddMinutes(1), true, "");
            //加密票据
            string authTicket = FormsAuthentication.Encrypt(ticket);
            //创建Cookie 
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, authTicket);
            cookie.Expires = ticket.Expiration;
            response.Cookies.Add(cookie);
        }

        public bool IsUserExisted(string username)
        {
            using (var context = new DbMonitorEntities())
            {
                var userCount = (from u in context.User
                                 where u.ULoginName == username
                                 select u).Count();
                //用户存在
                if (userCount > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsUserLocked(string username)
        {
            using (var context = new DbMonitorEntities())
            {
                var user = (from u in context.User
                            where u.ULoginName == username
                            select u).FirstOrDefault();
                if(user != null && user.UIsLock == 1)
                {
                    return true;
                }
            }
            return false;
        }
    }
}