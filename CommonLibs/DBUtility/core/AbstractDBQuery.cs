using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUtility
{
    public delegate void DBCallback(object msg);
    public abstract class AbstractDBQuery
    {
        protected MySqlCommand _cmd;
        protected MySqlDataReader _reader;
        protected object _result;
        public string m_strErrorText;

        internal void Init(MySqlConnection conn)
        {
            _cmd = conn.CreateCommand();
            _cmd.CommandTimeout = 0;
        }

        protected DBCallback m_callback;
        public void OnCall(DBCallback callback)
        {
            m_callback = callback;
        }
        public void PostUpdate()
        {
            if (m_callback != null)
            {
                m_callback(_result);
            }
        }
        abstract public bool Execute();
        public string ErrorLogText(Exception exception)
        {
            string logText = string.Empty;
            if (_cmd != null)
            {
                logText = "CommandText:" + _cmd.CommandText + "\r\n";

                for (int i = 0; i < _cmd.Parameters.Count; i++)
                {
                    var item = _cmd.Parameters[i];
                    if (item.Value == null)
                    {
                        logText += string.Format("{0}:null", item.ParameterName);
                    }
                    else
                    {
                        logText += string.Format("{0}:{1}", item.ParameterName, item.Value);
                    }
                }
            }
            else
            {
                logText = "MySqlCommand is null\r\n";
            }
            logText += "\r\n";
            logText += exception.ToString();
            return logText;
        }
        public string GetCmd()
        {
            return _cmd.ToString();
        }
    }
}
