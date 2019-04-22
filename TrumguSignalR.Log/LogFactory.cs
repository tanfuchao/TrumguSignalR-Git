using System;
using System.IO;
using System.Web;
using log4net;

namespace TrumguSignalR.Log
{
    /// <summary>
    /// 日志初始化
    /// </summary>
    public static class LogFactory
    {
        static LogFactory()
        {
//            var mapPath = HttpContext.Current.Server.MapPath("~/XmlConfig/log4net.config");
            var mapPath = AppDomain.CurrentDomain.BaseDirectory+ "XmlConfig\\log4net.config";
//            var currentDirectory = Environment.CurrentDirectory;
            FileInfo configFile = new FileInfo(mapPath);
            log4net.Config.XmlConfigurator.Configure(configFile);
        }
        public static Log GetLogger(Type type)
        {
            return new Log(LogManager.GetLogger(type));
        }
        public static Log GetLogger(string str)
        {
            return new Log(LogManager.GetLogger(str));
        }
    }
}
