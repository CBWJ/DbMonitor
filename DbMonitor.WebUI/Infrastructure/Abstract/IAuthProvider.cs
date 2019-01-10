using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DbMonitor.WebUI.Infrastructure.Abstract
{
    /// <summary>
    /// 用户认证接口
    /// </summary>
    public interface IAuthProvider
    {
        bool Authenticate(string username, string password);
        void SetAuthCookie(HttpResponseBase response, string username);
        bool IsUserExisted(string username);
        bool IsUserLocked(string username);
    }
}
