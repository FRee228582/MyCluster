﻿using ClusterManagerServerLib.Server;
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
       public override void InitProtocol()
        {
            Message.Server.BattleManager.Protocol.BM2CM.Api.GenerateId();
            Message.Server.Battle.Protocol.B2CM.Api.GenerateId();

            Message.Server.ClusterManager.Protocol.CM2B.Api.GenerateId();
            Message.Server.ClusterManager.Protocol.CM2BM.Api.GenerateId();
        }

        public override void InitNetWork()
        {
            
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
