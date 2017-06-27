using CommonUtility;
using Logger;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Threading;


namespace DBUtility
{
    public class DBManager
    {
        private string _strIp;
        private string _strDatabase;
        private string _strUsername;
        private string _strPassword;
        private string _strPort;
        private bool m_bOpened = false;

        public string ConnStr = string.Empty;

        /// <summary>
        /// 存储
        /// </summary>
        private Queue<AbstractDBQuery> _saveQueue;
        /// <summary>
        /// 执行
        /// </summary>
        private Queue<AbstractDBQuery> _executionQueue = new Queue<AbstractDBQuery>();
        /// <summary>
        /// 更新
        /// </summary>
        private Queue<AbstractDBQuery> _postUpdateQueue = new Queue<AbstractDBQuery>();
        /// <summary>
        /// 异常日志
        /// </summary>
        private Queue<string> _exceptionLogQueue = new Queue<string>();


        public bool Init(string ip, string database, string username, string password, string port)
        {
            _saveQueue = new Queue<AbstractDBQuery>();
            _postUpdateQueue = new Queue<AbstractDBQuery>();

            this._strIp = ip;
            this._strDatabase = database;
            this._strUsername = username;
            this._strPassword = password;
            this._strPort = port;

            ConnStr = string.Format("data source={0}; database={1}; user id={2}; password={3}; port={4}", _strIp, _strDatabase, _strUsername, _strPassword, _strPort);

            return true;

        }
        public bool Exit()
        {
            return true;
        }

        public void AddDBQuery(AbstractDBQuery query)
        {
            query.Init(this);
            lock (_saveQueue)
            {
                _saveQueue.Enqueue(query);
            }
        }
        //Asynchronous
        private double _lasttime = 0;
        private double _totaltime = 0;
        public void Run()
        {
            var tempPostUpdateQueue = new Queue<AbstractDBQuery>();
            var time = new SystemTime();
            time.Init();
            while (true)
            {
                var dt = time.Update();
                _lasttime = dt.TotalMilliseconds;
                if (_lasttime > 1)
                {
                    Thread.Sleep(0);
                }
                else
                {
                    Thread.Sleep(1);
                }
                if (_totaltime > 10000)
                {
                    _totaltime = 0;
                }
                else
                {
                    _totaltime += _lasttime;
                }

                try
                {
                    lock (_saveQueue)
                    {
                        if (_saveQueue.Count == 0)
                        {
                            continue;
                        }
                        while (_saveQueue.Count > 0)
                        {
                            AbstractDBQuery query = _saveQueue.Dequeue();
                            _executionQueue.Enqueue(query);
                        }
                    }
                    while (_executionQueue.Count != 0)
                    {
                        var query = _executionQueue.Dequeue();
                        bool success = query.Execute();
                        if (success == false)
                        {
                            if (query.m_strErrorText != null)
                            {
                                AddExceptionLog(query.m_strErrorText);
                            }
                        }
                        else
                        {

                        }
                        tempPostUpdateQueue.Enqueue(query);
                    }
                    lock (_postUpdateQueue)
                    {
                        while (tempPostUpdateQueue.Count > 0)
                        {
                            _postUpdateQueue.Enqueue(tempPostUpdateQueue.Dequeue());
                        }
                    }
                    tempPostUpdateQueue.Clear();
                }
                catch (Exception e)
                {
                    LOG.Error(e.ToString());
                }
            }
        }

        public void Call(AbstractDBQuery query, DBCallback callback = null)
        {
            query.OnCall(callback);
            AddDBQuery(query);
        }

        public void AddExceptionLog(string log)
        {
            lock (_exceptionLogQueue)
            {
                _exceptionLogQueue.Enqueue(log);
            }
        }

        public Queue<AbstractDBQuery> GetPostUpdateQueue()
        {
            Queue<AbstractDBQuery> ret = new Queue<AbstractDBQuery>();
            lock (_postUpdateQueue)
            {
                while (_postUpdateQueue.Count > 0)
                {
                    AbstractDBQuery query = _postUpdateQueue.Dequeue();
                    ret.Enqueue(query);
                }
            }
            return ret;
        }
        public Queue<string> GetExceptionLogQueue()
        {
            Queue<string> result;
            lock (_exceptionLogQueue)
            {
                if (_exceptionLogQueue.Count > 0)
                {
                    result = _exceptionLogQueue;
                    _exceptionLogQueue = new Queue<string>();
                    return result;
                }
            }
            return null;
        }

        public MySqlConnection GetOneConnection()
        {
            try
            {
                MySqlConnection conn = new MySqlConnection(ConnStr);
                return conn;
            }
            catch (MySqlException e)
            {
                LOG.Error(e.ToString());
                return null;
            }

        }
    }
}
