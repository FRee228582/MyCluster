using DataProperty;
using DBUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrameWork
{
    partial class AbstractServer:IDBAccess
    {
        private DBProxy _db;
        public DBProxy Db { get => _db; }
        public void InitDB()
        {
            _db = new DBProxy();
            DataList dbList = DataListManager.Inst.GetDataList("DBConfig");
            foreach (var item in dbList)
            {
                string nickName = item.Value.Name;
                string dbIp = item.Value.GetString("ip");
                string dbName = item.Value.GetString("db");
                string dbAccount = item.Value.GetString("account");
                string dbPassword = item.Value.GetString("password");
                string dbPort = item.Value.GetString("port");
                string type = item.Value.GetString("type");
                int poolCount = item.Value.GetInt("threads");

                DBManagerPool dbPool = new DBManagerPool(poolCount);
                dbPool.Init(dbIp, dbName, dbAccount, dbPassword, dbPort);

                _db.AddNameDB(nickName, dbPool);
            }

            DataList tableList = DataListManager.Inst.GetDataList("DBTables");
            foreach (var item in tableList)
            {
                string tableName = item.Value.Name;
                string writeDbName = item.Value.GetString("write");
                string readDbName = item.Value.GetString("read");
                DBManagerPool writeDb = Db.GetDbByLableName(writeDbName);
                _db.AddTableDB(tableName, writeDb, DBOperateType.Write);
                if (writeDbName == null)
                {
                    Console.WriteLine("can not get table {0} write db", tableName);
                }
                DBManagerPool readDb = Db.GetDbByLableName(readDbName);
                if (readDbName == null)
                {
                    Console.WriteLine("can not get table {0} read db", tableName);
                }
                _db.AddTableDB(tableName, readDb, DBOperateType.Read);
            }

            //测试连接
            foreach (var dbPool in Db.DBLabelNameList)
            {
                for (int i = 0; i < dbPool.Value.DBManagerList.Count; i++)
                {
                    try
                    {
                        dbPool.Value.Call(new QueryConnector(), i, ret =>
                        {
                            Console.WriteLine("Query Connector to db");
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
        public void UpdateProcessDB()
        {
            foreach (var dbPool in Db.DBLabelNameList)
            {
                foreach (var dbManager in dbPool.Value.DBManagerList)
                {
                    try
                    {
                        var queue = dbManager.GetPostUpdateQueue();
                        while (queue.Count != 0)
                        {
                            var query = queue.Dequeue();
                            query.PostUpdate();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
            }
        }
    }
}
