using System;
using System.Collections.Generic;

namespace BattleServerLib
{
    public partial class Api
    {
        void InitProtocol()
        {
            Message.Server.Battle.Protocol.B2CM.Api.GenerateId();
            Message.Server.Battle.Protocol.B2BM.Api.GenerateId();

            Message.Server.BattleManager.Protocol.BM2B.Api.GenerateId();
            Message.Server.ClusterManager.Protocol.CM2B.Api.GenerateId();
        }

        //private Object lstLock;
        //List<BMServer> lstBMServers = new List<BMServer>();

        BMServer m_BMServer;

        void InitBattleManagerServer()
        {
            //lstLock = new Object();
            //lock (lstLock)
            //{
            //    for (int i = 0; i < 5000; i++)
            //    {
                    m_BMServer = new BMServer(this, "127.0.0.1", 9999);
                    m_BMServer.Connect();
                    //lstBMServers.Add(m_BMServer);
            //    }
            //}
        }

        CMServer m_CMServer;
        void InitClusterManagerServer()
        {
            m_CMServer = new CMServer(this, "127.0.0.1", 8003);
            m_CMServer.Connect();
        }

    }
}
