using ClusterManagerServerLib.Server;
using CommonUtility;
using DataProperty;
using DBUtility;
using System;
using System.IO;
using System.Windows.Forms;

namespace ClusterManagerServerLib
{
    public partial class Api
    {
        void InitPath()
        {
            string pathString = string.Empty;
            string rootPath = string.Empty;
            if (string.IsNullOrEmpty(pathString))
            {
                DirectoryInfo path = new DirectoryInfo(Application.StartupPath);
                if (path.Parent.Exists)
                {
                    rootPath = path.Parent.FullName;
                }
                else
                {
                    Console.WriteLine("Path is error!Please check the input path!");
                }
            }
            else
            {
                rootPath = pathString;
            }
            PathMng.SetPath(rootPath);
        }

        void InitData()
        {
            string[] files = Directory.GetFiles(PathMng.FullPathFromData("XML"), "*.xml", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                DataListManager.Inst.Parse(file);
            }
        }

        private DBProxy _db;
        public DBProxy Db { get => _db; }

        void InitDB()
        {
            _db = new DBProxy();
            DataList dbList = DataListManager.Inst.GetDataList("DBConfig");
            foreach (var item in dbList)
            {
                string nickName = item.Value.Name;
                string dbIp = item.Value.GetString("ip");
            }
        }

        void InitProtocol()
        {
            Message.Server.BattleManager.Protocol.BM2CM.Api.GenerateId();
            Message.Server.Battle.Protocol.B2CM.Api.GenerateId();

            Message.Server.ClusterManager.Protocol.CM2B.Api.GenerateId();
            Message.Server.ClusterManager.Protocol.CM2BM.Api.GenerateId();

        }

        BMServer m_BMServer;
        BMServer BMServer1;
        BMServer BMServer2;
        void InitBattleManagerServer()
        {
            m_BMServer = new BMServer(this, 8002);
            m_BMServer.StartListen();

            BMServer1 = new BMServer(this, 8002);
            BMServer1.StartListen();

            BMServer2 = new BMServer(this, 8002);
            BMServer2.StartListen();
        }

        BattleServer m_BattleServer;
        BattleServer BattleServer1;
        BattleServer BattleServer2;


        void InitBattleServer()
        {
            m_BattleServer = new BattleServer(this, 8003);
            m_BattleServer.StartListen();
            BattleServer1 = new BattleServer(this, 8003);
            BattleServer1.StartListen();
            BattleServer2 = new BattleServer(this, 8003);
            BattleServer2.StartListen();
        }
    }
}
