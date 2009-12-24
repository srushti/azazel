using System;
using log4net;
using log4net.Config;

namespace Azazel.Logging {
    public static class LogManager {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof (LogManager));

        static LogManager() {
            XmlConfigurator.Configure();
        }

        public static void WriteLog(object log) {
            logger.Debug(log.ToString());
        }

        public static void WriteLog(Exception exception) {
            logger.Error(exception.ToString());
        }

        public static void WriteLog(string log) {
            logger.Debug(log);
        }

        public static void WriteLog(string log, params object[] args) {
            WriteLog(string.Format(log, args));
        }
    }
}