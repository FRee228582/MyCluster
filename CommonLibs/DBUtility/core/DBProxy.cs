using Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBUtility
{
    public enum DBOperateType
    {
        Write = 1,
        Read = 2
    }
    public enum DBTableParamType
    {
        Character = 1,
    }
    public struct DBProxyDefault
    {
        public const string DefaultTableName = "charactor";
        public const DBOperateType DefaultOperateType = DBOperateType.Write;
        public const int TableBaseCount = 20;
    }
    public class DBProxy
    {
        /// <summary>
        /// key Label name ,value DBManagerPool
        /// </summary>
        public Dictionary<string, DBManagerPool> DBLabelNameList = new Dictionary<string, DBManagerPool>();
        /// <summary>
        /// key tablename ,value DBManagerPool
        /// </summary>
        public Dictionary<string, DBManagerPool> WriteTableList = new Dictionary<string, DBManagerPool>();
        public Dictionary<string, DBManagerPool> ReadTableList = new Dictionary<string, DBManagerPool>();
        public void AddTableDB(string tableName, DBManagerPool db, DBOperateType operateType)
        {
            switch (operateType)
            {
                case DBOperateType.Write:
                    WriteTableList.Add(tableName, db);
                    break;
                case DBOperateType.Read:
                    ReadTableList.Add(tableName, db);
                    break;
                default:
                    Console.WriteLine("add table db falied:got invalid opetate type {0}",operateType);
                    break;
            }
        }
        public void AddNameDB(string labelName,DBManagerPool db)
        {
            DBLabelNameList.Add(labelName, db);
        }
        public  DBManagerPool GetDbByLableName(string labelName)
        {
            DBManagerPool db = null;
            DBLabelNameList.TryGetValue(labelName, out db);
            return db;
        }
        public DBManagerPool GetWriteDbByTableName(string tableName)
        {
            DBManagerPool db = null;
            WriteTableList.TryGetValue(tableName, out db);
            return db;
        }
        public DBManagerPool GetReadDbByTableName(string tableName)
        {
            DBManagerPool db = null;
            ReadTableList.TryGetValue(tableName, out db);
            return db;
        }
        public DBManagerPool GetDbByTable(string tableName,DBOperateType type)
        {
            DBManagerPool db = null;
            switch (type)
            {
                case DBOperateType.Write:
                    WriteTableList.TryGetValue(tableName, out db);
                    break;
                case DBOperateType.Read:
                    ReadTableList.TryGetValue(tableName, out db);
                    break;
                default:
                    break;
            }
            return db;
        }
        public void Abort()
        {
            foreach (var db in DBLabelNameList)
            {
                try
                {
                    db.Value.Abort();
                }
                catch (Exception e)
                {
                    LOG.Error("db {0} Abort error {1}", db.Key.ToString(), e.ToString());
                    return;
                }
            }
        }
        public bool Exit()
        {
            foreach (var db in DBLabelNameList)
            {
                try
                {
                    db.Value.Exit();
                }
                catch (Exception e)
                {
                    LOG.Error("db {0} Exit error {1}", db.Key.ToString(), e.ToString());
                    return false;
                }
            }
            return true;
        }
        public string GetTableName(string prefixName,int param,DBTableParamType type)
        {
            string tableName = prefixName;
            switch (type)
            {   
                case DBTableParamType.Character:
                    int suffix = param % DBProxyDefault.TableBaseCount;
                    if (suffix<10)
                    {
                        tableName = string.Format("{0}_0{1}", prefixName, suffix.ToString());
                    }
                    else
                    {
                        tableName = string.Format("{0}_{1}", prefixName, suffix.ToString());
                    }
                    break;
                default:
                    LOG.Warn("get table name failed:invalid db table param type {0}",type);
                    break;
            }
            return tableName;
        }
        public void Call(AbstractDBQuery query,string tableName=DBProxyDefault.DefaultTableName,DBOperateType type =DBProxyDefault.DefaultOperateType,DBCallback callback=null)
        {
            DBManagerPool dbPool = GetDbByTable(tableName, type);
            if (dbPool==null)
            {
                LOG.Warn("db call {0} failed:can not find table{1} type {2} db", query.GetCmd(), tableName, type.ToString());
                return;
            }
            dbPool.Call(query, callback);
        }
        public void Call(AbstractDBQuery query,int index,string tableName=DBProxyDefault.DefaultTableName,DBOperateType type =DBProxyDefault.DefaultOperateType,DBCallback callback=null)
        {
            DBManagerPool dbPool = GetDbByTable(tableName, type);
            if (dbPool==null)
            {
                LOG.Warn("db call {0} failed:can not find table{1} type {2} db", query.GetCmd(), tableName, type.ToString());
                return;
            }
            dbPool.Call(query, index,callback);
        }
    }
}
