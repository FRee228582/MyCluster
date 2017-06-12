using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Logger;

namespace DBUtility
{
    public class DBManagerPool
    {
        private string _strIp;
        private string _strDatabase;
        private string _strUsername;
        private string _strPassword;
        private string _strPort;

        private int _poolCount = 0;
        private int _index = 0;

        private List<DBManager> _dbMngList = new List<DBManager>();
        public List<DBManager> DBMngLst
        { get { return _dbMngList; } }
        private List<Thread> _dbThreadList = new List<Thread>();
        public Dictionary<int, int> m_dbCallCountList = new Dictionary<int, int>();
        public Dictionary<string, int> m_dbCallNameList = new Dictionary<string, int>();
        public DBManagerPool(int count)
        {
            _poolCount = count;
            for (int i = 0; i < _poolCount; i++)
            {
                DBManager db = new DBManager();
                _dbMngList.Add(db);
                m_dbCallCountList.Add(i, 0);
            }
        }
        public bool Init(string ip, string database, string username, string password, string port)
        {
            this._strIp = ip;
            this._strDatabase = database;
            this._strUsername = username;
            this._strPassword = password;
            this._strPort = port;
            foreach (var db in DBMngLst)
            {
                if (!db.Init(_strIp, _strDatabase, _strUsername, _strPassword, _strPort))
                {
                    return false;
                }
                Thread dbThread = new System.Threading.Thread(db.Run);
                _dbThreadList.Add(dbThread);
                dbThread.Start();
            }
            return true;
        }
        public int Call(AbstractDBQuery query,DBCallback callback = null)
        {
            int dbIndex = GetDBIndex();
            DBMngLst[dbIndex].Call(query,callback);
            return dbIndex;
        }
        public int Call(AbstractDBQuery query,int forceIndex,DBCallback callback = null)
        {
            int dbIndex = 0;
            if (forceIndex>0&&forceIndex<DBMngLst.Count)
            {
                DBMngLst[forceIndex].Call(query, callback);
                m_dbCallCountList[forceIndex]++;
            }
            else
            {
                DBMngLst[0].Call(query, callback);
                m_dbCallCountList[0]++;
            }
            if (!m_dbCallNameList.ContainsKey(query.ToString()))
            {
                m_dbCallNameList.Add(query.ToString(), 1);
            }
            else
            {
                m_dbCallNameList[query.ToString()]++;
            }
            return dbIndex;
        }
        public int GetDBIndex()
        {
            _index++;
            if (_index>=10000)
            {
                _index = 0;
            }
            return _index % DBMngLst.Count;
        }
        public int GeNextDBIndex()
        {
            return (_index + 1) % DBMngLst.Count;
        }
        public void Abort()
        {
            foreach (var thread in _dbThreadList)
            {
                thread.Abort();
            }
            _dbThreadList.Clear();
        }
        public bool Exit()
        {
            foreach (var db in _dbMngList)
            {
                try
                {
                    db.Exit();
                }
                catch (MySqlException e)
                {
                    LOG.Error(e.ToString());
                    return false;
                }
            }
            return true;
        }
        public DBManager GetOneDBManager()
        {
            int curIndex = GetDBIndex();
            return DBMngLst[curIndex];
        }
        
    }
}
