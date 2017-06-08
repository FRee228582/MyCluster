using log4net;
using System;

namespace SocketAsyncSvr
{
    public class Program
    {
        public static void Init()
        {
            DateTime now = DateTime.Now;
            log4net.GlobalContext.Properties["LogDir"] = now.ToString("yyyyMM");
            log4net.GlobalContext.Properties["LogFileName"] = "_SocketAsyncServer" + now.ToString("yyyyMMdd");
            logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }
        private static ILog logger;

        public static ILog Logger
        {
            get => logger;
        }
    }
}