using System;
using TrumguSignalR.Util.Config;
using TrumguSignalR.Util.Email;

namespace TrumguSignalR.Log
{
    public static class LogWrite
    {
        public static void WriteLogInfo(string info)
        {
            var log = LogFactory.GetLogger("service");
            LogMessage logMessage = new LogMessage
            {
                OperationTime = DateTime.Now,
                Content = info,

            };
            string strMessage = new LogFormat().InfoFormat(logMessage);
            log.Info(strMessage);
        }

        /// <summary>
        /// 异常日志
        /// </summary>
        /// <param name="context">提供使用</param>
        public static void WriteLogError(Exception context)
        {
            if (context == null)
                return;
            var log = LogFactory.GetLogger(context.ToString());
            Exception error = context;
            LogMessage logMessage = new LogMessage
            {
                OperationTime = DateTime.Now,
                Content = error.Message,
                ExceptionInfo = error.InnerException == null ? error.Message : error.InnerException.Message
            };
            string strMessage = new LogFormat().ExceptionFormat(logMessage);
            log.Error(strMessage);
            //SendMail(strMessage);
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        private static void SendMail(string body)
        {
            var errorToMail = Config.GetValue("ErrorToMail");
            if (errorToMail == "true")
            {
                MailHelper.Send("tanfc@trumgu.com", "推送服务 - 发生异常", body.Replace("-", ""));
            }
        }
    }
}
