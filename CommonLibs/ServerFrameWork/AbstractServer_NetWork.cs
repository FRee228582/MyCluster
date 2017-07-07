using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerFrameWork
{
    partial class AbstractServer : INetWork
    {
        public abstract void InitNetWork();

        public abstract void InitProtocol();

    }
}
