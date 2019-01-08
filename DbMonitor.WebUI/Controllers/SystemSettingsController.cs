using DbMonitor.Domain;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DbMonitor.WebUI.Controllers
{
    public class SystemSettingsController : BaseController
    {
        // GET: SystemSettings
        public ActionResult Index()
        {
            //系统设置
            List<Dictionary> dicSys = db.Dictionary.Where(d => d.DTypeCode == "SysSettings" && d.DEnable == 1).ToList();
            return View(dicSys);
        }
        [HttpPost]
        public ActionResult Edit(string sysname, string copyright, string Logo)
        {
            JsonResult ret = new JsonResult();
            try
            {
                var dic = db.Dictionary.Where(d => d.DTypeCode == "SysSettings" && d.DCode == "SystemName").FirstOrDefault();
                dic.DName = sysname;
                dic = db.Dictionary.Where(d => d.DTypeCode == "SysSettings" && d.DCode == "CopyRight").FirstOrDefault();
                dic.DName = copyright;
                dic = db.Dictionary.Where(d => d.DTypeCode == "SysSettings" && d.DCode == "Logo").FirstOrDefault();
                dic.DName = Logo;
                db.SaveChanges();
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 0,
                    message = ""
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    status = 1,
                    message = ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
        [HttpPost]
        public ActionResult UploadLogo(HttpPostedFileBase imageFile)
        {
            JsonResult ret = new JsonResult();
            try
            {
                var iconPath = Server.MapPath("~/images/logo");
                DirectoryInfo directoryInfo = new DirectoryInfo(iconPath);
                var imgFiles = directoryInfo.GetFiles();
                var subnetFiles = (from f in imgFiles
                                   where f.Name.ToLower().Contains("logo")
                                   select f).ToList();
                var fileType = imageFile.FileName.Substring(imageFile.FileName.IndexOf(".") + 1);
                var fileName = imageFile.FileName;//"logo" + (subnetFiles.Count + 100).ToString() + "." + fileType;
                var filePath = Path.Combine(iconPath, fileName);
                byte[] byteIcon = new byte[imageFile.ContentLength];
                imageFile.InputStream.Read(byteIcon, 0, byteIcon.Length);
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    fs.Write(byteIcon, 0, byteIcon.Length);
                }
                ret.Data = JsonConvert.SerializeObject(new
                {
                    code = 0,
                    msg = fileName
                });
            }
            catch (Exception ex)
            {
                ret.Data = JsonConvert.SerializeObject(new
                {
                    code = 1,
                    msg = ex.Message
                });
                RecordException(ex);
            }
            return ret;
        }
    }
}