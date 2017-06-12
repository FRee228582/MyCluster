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

        private MySqlConnection _conn = null;
        public MySqlConnection Conn
        {
            get { return _conn; }
        }
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

        public ReconnectRecord m_cReconnectInfo;

        public bool Init(string ip, string database, string username, string password, string port)
        {
            m_cReconnectInfo = new ReconnectRecord();
            m_cReconnectInfo.Init(60 * 1000);
            _saveQueue = new Queue<AbstractDBQuery>();
            _postUpdateQueue = new Queue<AbstractDBQuery>();

            this._strIp = ip;
            this._strDatabase = database;
            this._strUsername = username;
            this._strPassword = password;
            this._strPort = port;

            string strConn = string.Format("data source={0}; database={1}; user id={2}; password={3}; port={4}", _strIp, _strDatabase, _strUsername, _strPassword, _strPort);
            try
            {
                _conn = new MySqlConnection(strConn);
                _conn.Open();
                m_bOpened = true;
                return true;
            }
            catch (MySqlException e)
            {
                LOG.Error(e.ToString());
                return false;
            }

        }
        public bool Exit()
        {
            try
            {
                if (_conn != null)
                {
                    Conn.Close();
                    m_bOpened = false;
                    _conn = null;
                }
                return true;
            }
            catch (MySqlException e)
            {
                LOG.Error(e.ToString());
                return false;
            }
        }
        public bool IsDisconnected()
        {
            return (Conn.State == System.Data.ConnectionState.Closed || Conn.State == System.Data.ConnectionState.Broken);
        }

        public void AddDBQuery(AbstractDBQuery query)
        {
            query.Init(Conn);
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
                lock (m_cReconnectInfo)
                {
                    if (IsDisconnected() || m_cReconnectInfo.NeedReconnect)
                    {
                        m_bOpened = false;
                        string log = string.Format("disconnect from db{0},eslplased {1} ms", _strDatabase, m_cReconnectInfo.TryConnectTime);
                        AddExceptionLog(log);
                        string strConn = string.Format("data source={0}; database={1}; user id={2}; password={3}; port={4}", _strIp, _strDatabase, _strUsername, _strPassword, _strPort);
                        try
                        {
                            _conn = new MySqlConnection(strConn);
                            _conn.Open();
                            m_bOpened = true;
                            m_cReconnectInfo.Reset();
                        }
                        catch (MySqlException e)
                        {
                            LOG.Error(e.ToString());
                            AddExceptionLog(e.ToString());
                        }
                    }
                    else
                    {
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
                            while (_exceptionLogQueue.Count!=0)
                            {
                                var query = _executionQueue.Dequeue();
                                bool success = query.Execute();
                                if (success==false)
                                {
                                    if (query.m_strErrorText!=null)
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
                                while (tempPostUpdateQueue.Count>0)
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
            }
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
            Queue<AbstractDBQuery> result;//= new Queue<AbstractDBQuery>();
            lock (_postUpdateQueue)
            {
                //while (_postUpdateQueue.Count>0)
                //{
                //    AbstractDBQuery query = _postUpdateQueue.Dequeue();
                //    result.Enqueue(query);
                //}
                if (_postUpdateQueue.Count > 0)
                {
                    result = _postUpdateQueue;
                    _postUpdateQueue = new Queue<AbstractDBQuery>();
                    return result;
                }
            }
            return null;
        }
        public Queue<string> GetExceptionLogQueue()
        {
            Queue<string> result; //= new Queue<string>();
            lock (_exceptionLogQueue)
            {
                if (_exceptionLogQueue.Count>0)
                {
                    result = _exceptionLogQueue;
                    _exceptionLogQueue = new Queue<string>();
                    return result;
                }
            }
            return null;
        }
        public void Call(AbstractDBQuery query,DBCallback callback=null)
        {
            query.OnCall(callback);
            AddDBQuery(query);
        }
    }
}
