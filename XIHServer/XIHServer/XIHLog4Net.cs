
using log4net;
using System;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "Config/Log4Net.config", ConfigFileExtension = "config", Watch = true)]
namespace XIHServer
{
    public static class XIHLog4Net
    {
        /// <summary>
        /// 错误日志
        /// </summary>
        private static readonly ILog logError = LogManager.GetLogger("error");

        /// <summary>
        /// 信息日志
        /// </summary>
        private static readonly ILog logInfo = LogManager.GetLogger("info");

        /// <summary>
        /// 调试日志
        /// </summary>
        private static readonly ILog logDebug = LogManager.GetLogger("debug");

        /// <summary>
        /// 输出信息日志
        /// </summary>
        /// <param name="message">消息</param>
        public static void Info(string message)
        {
            if (logInfo.IsInfoEnabled)
            {
                logInfo.Info(message);
            }
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="message">调试信息</param>
        public static void Debug(string message)
        {
            if (logDebug.IsDebugEnabled)
            {
                logDebug.Debug(message);
            }
        }

        /// <summary>
        /// 输出调试日志
        /// </summary>
        /// <param name="ex">异常信息</param>
        public static void Debug(Exception ex)
        {
            if (logDebug.IsDebugEnabled)
            {
                logDebug.Debug(ex.Message.ToString() + "/r/n" + ex.Source.ToString() + "/r/n" +
                    ex.TargetSite.ToString() + "/r/n" + ex.StackTrace.ToString());
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="ex">错误信息</param>
        public static void Error(Exception ex)
        {
            if (logError.IsErrorEnabled)
            {
                logError.Error(ex.Message, ex);
            }
        }

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">消息</param>
        /// <param name="ex">错误信息</param>
        public static void Error(string message, Exception ex)
        {
            if (logError.IsErrorEnabled)
            {
                logError.Error(message, ex);
            }
        }
    }
}
