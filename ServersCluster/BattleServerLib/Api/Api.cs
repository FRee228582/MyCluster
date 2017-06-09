using ServerFrameWork;
using System;

namespace BattleServerLib
{
    public partial class Api : AbstractServer
    {
        public override void Init(string[] args)
        {
            if (args.Length == 3)
            {
                ApiTag.AreaId = ushort.Parse(args[0]);
                ApiTag.ServerId = ushort.Parse(args[1]);
                ApiTag.SubId = ushort.Parse(args[2]);
            }
            else
            {

            }
            InitProtocol();

            InitBattleManagerServer();
            //InitClusterManagerServer();
        }

        public override void Exit()
        {
        }

        public override void Update()
        {
            //lock(lstLock)
            //{
            //    foreach (var item in lstBMServers)
            //    {
            //        item.Update();
            //    }
            //}

            m_BMServer.Update();
            //m_CMServer.Update();
        }

        public override void ExcuteCommand(string cmd)
        {
            string[] cmdArr = cmd.Split(' ');
            if (cmdArr.Length == 0)
            {
                return;
            }
            string help = "";
            switch(cmdArr[0])
            {
                case "start":
                    m_BMServer.SetTestStart();
                    break;
                case "stop":
                    m_BMServer.SetTestStop();
                    break;
                default:
                    break;
            }
        }
    }
}
