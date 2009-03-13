using log4net;
using log4net.Config;

namespace Azazel.Logging {
    public static class LogManager {
        private static readonly ILog logger = log4net.LogManager.GetLogger(typeof (LogManager));

        static LogManager() {
            XmlConfigurator.Configure();
        }

        public static void WriteLog(string log) {
            logger.Debug(log);
        }
    }
}