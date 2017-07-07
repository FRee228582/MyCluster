using DataProperty;
using DBUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrameWork
{
    public abstract partial class AbstractServer:AbstractBaseServer
    {
        public override void ExcuteCommand(string cmd)
        {
            throw new NotImplementedException();
        }

        public override void Exit()
        {

        }

        public override void Init(string[] args)
        {
            InitPath();
            InitData();
            InitLog();
            InitDB();
            InitProtocol();
            InitNetWork();
        }

        public override void Update()
        {
            UpdateProcessDB();
        }

      
    }
}
