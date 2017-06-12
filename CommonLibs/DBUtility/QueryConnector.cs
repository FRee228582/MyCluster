using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUtility
{
    class QueryConnector:AbstractDBQuery
    {
        public override bool Execute()
        {
            _cmd.CommandText = "SELECT NOW();";
            _cmd.CommandType = System.Data.CommandType.Text;
            try
            {
                _reader = _cmd.ExecuteReader();
                _result = 1;
            }
            catch (Exception e)
            {
                _result = null;
                m_strErrorText = ErrorLogText(e);
                return false;
            }
            finally
            {
                if (_reader!=null)
                {
                    _reader.Close();
                }
            }
            return true;
        }
    }
}
