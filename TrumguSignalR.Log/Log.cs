using log4net;

namespace TrumguSignalR.Log
{
    /// <summary>
    /// 日志
    /// </summary>
    public class Log
    {
        private readonly ILog _logger;
        public Log(ILog log)
        {
            _logger = log;
        }
        public void Debug(object message)
        {
            _logger.Debug(message);
        }
        public void Error(object message)
        {
            _logger.Error(message);
        }
        public void Info(object message)
        {
            _logger.Info(message);
        }
        public void Warn(object message)
        {
            _logger.Warn(message);
        }
    }
}
