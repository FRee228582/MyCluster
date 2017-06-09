using Engine.Foundation;
using IOCPLib;
using IOCPLib.IOCP;
using Message.Server.Battle.Protocol.B2BM;
using Message.Server.BattleManager.Protocol.BM2B;
using ServerFrameWork;
using System;
using System.Collections.Generic;
using System.IO;

namespace BattleManagerServerLib
{

    public class BattleServerListen : AsyncSocketServer
    {
        Api _api = null;


        public BattleServerListen(Api server, ushort port)
            : base(port,1024)
        {
            _api = server;
            TickTock tick = new TickTock(this);
            tick.Start();
        }
        protected override void BuildingSocketInvokeElement(AsyncUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];

            userToken.InvokeElement = new BattleServerResponse(this, userToken,_api);
            //if (userToken.AsyncSocketInvokeElement != null)
            //{
            //    Program.Logger.InfoFormat("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
            //        userToken.AsyncSocketInvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            //}
            if (userToken.InvokeElement != null)
            {
                Console.WriteLine("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
                    userToken.InvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            }
        }



    }
}
