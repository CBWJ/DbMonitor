using System;
using System.Diagnostics;

namespace DbMonitor.WebUI.Utility
{
    /// <summary>
    /// CMD命令执行类
    /// </summary>
    public static class CmdHelper
    {
        public static void Execute(params string[] commands)
        {
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";         //要启动的应用程序或文档
            cmd.StartInfo.CreateNoWindow = true;        //是否在新窗口中启动该进程
            cmd.StartInfo.UseShellExecute = false;       //是否使用操作系统 shell 启动进程
            cmd.StartInfo.RedirectStandardError = true; //是否将应用程序的错误输出写入 Process.StandardError 流中
            cmd.StartInfo.RedirectStandardInput = true; //应用程序的输入是否从 Process.StandardInput 流中读取
            cmd.StartInfo.RedirectStandardOutput = true;//是否将应用程序的输出写入 Process.StandardOutput 流中

            cmd.Start();
            foreach (var c in commands)
            {
                cmd.StandardInput.WriteLine(c);
            }
            cmd.StandardInput.WriteLine("exit");
            cmd.WaitForExit();
            var output = cmd.StandardOutput.ReadToEnd();
            cmd.Close();
        }
    }
}