using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using DbMonitor.Domain;

namespace DbMonitor.WebUI.Extensions
{
    public static class HtmlHelperExtension
    {
        public static MvcHtmlString ModuleMenu(this HtmlHelper html, IEnumerable<Module> modules)
        {
            string menuHtml = ModuleTraverse(0, modules);
            return MvcHtmlString.Create(menuHtml);
        }

        //递归创建列表
        public static string ModuleTraverse(long moduleID, IEnumerable<Module> modules)
        {
            string result = "";
            if (modules == null)
                return result;
            var tops = from m in modules
                       where m.MParentID == moduleID
                       orderby m.MSortingNumber
                       select m;
            if (tops.ToList().Count == 0)
                return result;
            StringBuilder sbLINodes = new StringBuilder();
            TagBuilder tagUL = new TagBuilder("ul");
            if(moduleID != 0)
            {
                tagUL.AddCssClass("submenu");
            }
            foreach (var module in tops)
            {
                TagBuilder tagLI = new TagBuilder("li");
                TagBuilder tagLink = new TagBuilder("a");
                TagBuilder tagImage = new TagBuilder("img");
                TagBuilder tagIleft = new TagBuilder("i");
                TagBuilder tagIRight = new TagBuilder("i");
                TagBuilder tagSpan = new TagBuilder("span");

                tagSpan.SetInnerText(module.MName);
                tagLink.Attributes["href"] = module.MUrl;
                //左边图标
                if (module.MIconType == "font")
                {
                    tagIleft.AddCssClass("menu_icon fa " + module.MIcon + " fa-fw");
                    tagLink.InnerHtml = tagIleft.ToString();
                }
                else if (module.MIconType == "image")
                {
                    tagImage.Attributes["src"] = module.MIcon;
                    tagLink.InnerHtml = tagImage.ToString();
                }
                //标题
                tagLink.InnerHtml += tagSpan.ToString();
                //右边箭头
                bool hasChild = (from m in modules
                                 where m.MParentID == module.ID
                                 select m).ToList().Count > 0;
                if (hasChild)
                {
                    tagIRight.AddCssClass("right-tag fa fa-angle-down fa-fw");
                }
                tagLink.InnerHtml += tagIRight.ToString();

                tagLI.InnerHtml = tagLink.ToString();
                if (hasChild)
                {
                    tagLI.InnerHtml += ModuleTraverse(module.ID, modules);
                }
                sbLINodes.AppendLine(tagLI.ToString());
            }
            tagUL.InnerHtml = sbLINodes.ToString();
            result = tagUL.ToString();
            return result;
        }
    }
}