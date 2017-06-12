using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public static class LOG
    {
        static private ILogger _logger;
        static public void InitLogger(ILogger logger)
        {
            _logger = logger;
        }

        static public void Close()
        {
            _logger.Close();
        }

        static public void Write(object obj)
        {
            _logger.Write(obj);
        }
        static public void Write(string format, params object[] args)
        {
            _logger.Write(format, args);
        }
        static public void WriteLine(object obj)
        {
            _logger.WriteLine(obj);
        }
        static public void WriteLine(string format, params object[] args)
        {
            _logger.WriteLine(format, args);
        }

        static public void Info(object obj)
        {
            _logger.Info(obj);
        }
        static public void Info(string format, params object[] args)
        {
            _logger.Info(format, args);
        }
        static void InfoLine(object obj)
        {
            _logger.InfoLine(obj);
        }
        static void InfoLine(string format, params object[] args)
        {
            _logger.InfoLine(format, args);
        }

        static public void Error(object obj)
        {
            _logger.Error(obj);
        }
        static public void Error(string format, params object[] args)
        {
            _logger.Error(format, args);
        }
        static public void ErrorLine(object obj)
        {
            _logger.ErrorLine(obj);
        }
        static public void ErrorLine(string format, params object[] args)
        {
            _logger.ErrorLine(format, args);
        }

        static public void Warn(object obj)
        {
            _logger.Warn(obj);
        }
        static public void Warn(string format, params object[] args)
        {
            _logger.Warn(format, args);
        }
        static public void WarnLine(object obj)
        {
            _logger.WarnLine(obj);
        }
        static public void WarnLine(string format, params object[] args)
        {
            _logger.WarnLine(format, args);
        }


    }
}
