using Engine.Foundation;
using IOCPLib;
using Message.Server.Battle.Protocol.B2BM;
using Message.Server.BattleManager.Protocol.BM2B;
using ServerFrameWork;
using SocketAsyncSvr;
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
        }
        protected override void BuildingSocketInvokeElement(AsyncSocketUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];
            //if (flag == (byte)SocketFlag.Upload)
            //    userToken.AsyncSocketInvokeElement = new UploadSocketProtocol(this, userToken);
            //else if (flag == (byte)SocketFlag.Download)
            //    userToken.AsyncSocketInvokeElement = new DownloadSocketProtocol(this, userToken);
            //else if (flag == (byte)SocketFlag.RemoteStream)
            //    userToken.AsyncSocketInvokeElement = new RemoteStreamSocketProtocol(this, userToken);
            //else if (flag == (byte)SocketFlag.Throughput)
            //    userToken.AsyncSocketInvokeElement = new ThroughputSocketProtocol(this, userToken);
            //else
            //if (flag == (byte)SocketFlag.Control)
            //    userToken.AsyncSocketInvokeElement = new ControlSocketProtocol(this, userToken);
            //else if (flag == (byte)SocketFlag.LogOutput)
            //    userToken.AsyncSocketInvokeElement = new LogOutputSocketProtocol(this, userToken);

            userToken.AsyncSocketInvokeElement = new BattleServerResponse(this, userToken);
            if (userToken.AsyncSocketInvokeElement != null)
            {
                Program.Logger.InfoFormat("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
                    userToken.AsyncSocketInvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            }
        }



    }
}
