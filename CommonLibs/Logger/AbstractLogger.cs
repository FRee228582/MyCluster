﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    abstract public class AbstractLogger : ILogger
    {
        protected string GetString(string format, params object[] args)
        {
            var sb = new StringBuilder();
            sb.AppendFormat(format, args);
            return sb.ToString();
        }
        public abstract void Write(object obj);
        public abstract void WriteLine(object obj);
        public abstract void Info(object obj);
        public abstract void InfoLine(object obj);
        public abstract void Error(object obj);
        public abstract void ErrorLine(object obj);
        public abstract void Warn(object obj);
        public abstract void WarnLine(object obj);
        public abstract void Close();

        public void Write(string format, params object[] args)
        {
            Write((object)GetString(format, args));
        }
        public void WriteLine(string format, params object[] args)
        {
            WriteLine((object)GetString(format, args));
        }

        public void Info(string format, params object[] args)
        {
            Info((object)GetString(format, args));
        }
        public void InfoLine(string format, params object[] args)
        {
            InfoLine((object)GetString(format, args));
        }

        public void Error(string format, params object[] args)
        {
            Error((object)GetString(format, args));
        }
        public void ErrorLine(string format, params object[] args)
        {
            ErrorLine((object)GetString(format, args));
        }

        public void Warn(string format, params object[] args)
        {
            Warn((object)GetString(format, args));
        }
        public void WarnLine(string format, params object[] args)
        {
            WarnLine((object)GetString(format, args));
        }


    }
}
