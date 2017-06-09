using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.IO;

namespace SocketAsyncSvr
{
    //异步Socket调用对象，所有的协议处理都从本类继承
    public class AsyncSocketInvokeElement
    {
        protected AsyncSocketServer m_asyncSocketServer;
        protected AsyncSocketUserToken m_asyncSocketUserToken;
        public AsyncSocketUserToken AsyncSocketUserToken { get { return m_asyncSocketUserToken; } }

        private bool m_netByteOrder;
        public bool NetByteOrder { get { return m_netByteOrder; } set { m_netByteOrder = value; } } //长度是否使用网络字节顺序

        //protected IncomingDataParser m_incomingDataParser; //协议解析器，用来解析客户端接收到的命令
        //protected OutgoingDataAssembler m_outgoingDataAssembler; //协议组装器，用来组织服务端返回的命令

        protected bool m_sendAsync; //标识是否有发送异步事件

        protected DateTime m_connectDT;
        public DateTime ConnectDT { get { return m_connectDT; } }
        protected DateTime m_activeDT;
        public DateTime ActiveDT { get { return m_activeDT; } }

        public AsyncSocketInvokeElement(AsyncSocketServer asyncSocketServer, AsyncSocketUserToken asyncSocketUserToken)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_asyncSocketUserToken = asyncSocketUserToken;

            m_netByteOrder = false;

            //m_incomingDataParser = new IncomingDataParser();
            //m_outgoingDataAssembler = new OutgoingDataAssembler();

            m_sendAsync = false;

            m_connectDT = DateTime.UtcNow;
            m_activeDT = DateTime.UtcNow;
        }

        public virtual void Close()
        { 
        }

        public virtual bool ProcessReceive(byte[] buffer, int offset, int count) //接收异步事件返回的数据，用于对数据进行缓存和分包
        {
            m_activeDT = DateTime.UtcNow;
            DynamicBufferManager receiveBuffer = m_asyncSocketUserToken.ReceiveBuffer;

            receiveBuffer.WriteBuffer(buffer, offset, count);
            if (receiveBuffer.DataCount > sizeof(int))
            {
                ////按照长度分包
                //int packetLength = BitConverter.ToUInt16(receiveBuffer.Buffer, 0); //获取包长度
                //if (NetByteOrder)
                //    packetLength = System.Net.IPAddress.NetworkToHostOrder(packetLength); //把网络字节顺序转为本地字节顺序


                //if ((packetLength > 10 * 1024 * 1024) | (receiveBuffer.DataCount > 10 * 1024 * 1024)) //最大Buffer异常保护
                //    return false;

                //if ((receiveBuffer.DataCount - sizeof(int)) >= packetLength) //收到的数据达到包长度
                {
                    //bool result = ProcessPacket(receiveBuffer.Buffer, sizeof(int), packetLength);
                    //if (result)
                    //    receiveBuffer.Clear(packetLength + sizeof(int)); //从缓存中清理
                    //return result;
                    MemoryStream transferred = new MemoryStream(receiveBuffer.Buffer, offset, count, true, true);

                    bool result = ProcessPacket(transferred);
                    if (result)
                        receiveBuffer.Clear(count); //从缓存中清理
                    return result;
                }
                //else
                //{
                //    return true;
                //}
            }
            else
            {
                return true;
            }
            return true;
        }
        public virtual bool ProcessPacket(byte[] buffer, int offset, int count) //处理分完包后的数据，把命令和数据分开，并对命令进行解析
        {
            while (count-offset >= sizeof(UInt16))
            {
                UInt16 size = BitConverter.ToUInt16(buffer, offset); //包长
                if (size + sizeof(UInt16) + sizeof(Int32) > count - offset)
                {
                    break;
                }
                UInt32 msg_id = BitConverter.ToUInt32(buffer, offset + sizeof(UInt16));
                MemoryStream msg = new MemoryStream(buffer, offset + sizeof(UInt16) + sizeof(Int32), size, true, true);
                lock (m_msgQueue)
                {
                    m_msgQueue.Enqueue(new KeyValuePair<uint, MemoryStream>(msg_id, msg));
                }
                offset += (size + sizeof(UInt16) + sizeof(Int32));
            }
            return true;
        }

        protected virtual bool ProcessPacket(MemoryStream stream)
        {
            int offset = 0;
            byte[] buffer = stream.GetBuffer();
            while ((stream.Length - offset) > sizeof(UInt16))
            {
                UInt16 size = BitConverter.ToUInt16(buffer, offset);
                if (size + sizeof(UInt16) + sizeof(Int32) > stream.Length - offset)
                {
                    break;
                }

                UInt32 msg_id = BitConverter.ToUInt32(buffer, offset + sizeof(UInt16));
                MemoryStream msg = new MemoryStream(buffer, offset + sizeof(UInt16) + sizeof(Int32), size, true, true);
                lock (m_msgQueue)
                {
                    m_msgQueue.Enqueue(new KeyValuePair<uint, MemoryStream>(msg_id, msg));
                }
                offset += (size + sizeof(UInt16) + sizeof(Int32));
            }
            stream.Seek(offset, SeekOrigin.Begin);
            return true;
        }


        #region 接收队列执行回调

        protected Queue<KeyValuePair<UInt32, MemoryStream>> m_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();
        protected Queue<KeyValuePair<UInt32, MemoryStream>> m_deal_msgQueue = new Queue<KeyValuePair<uint, MemoryStream>>();

        public void OnProcessProtocal()
        {
            lock (m_msgQueue)
            {
                while (m_msgQueue.Count > 0)
                {
                    var msg = m_msgQueue.Dequeue();
                    m_deal_msgQueue.Enqueue(msg);
                }
            }
            while (m_deal_msgQueue.Count > 0)
            {
                var msg = m_deal_msgQueue.Dequeue();
                OnResponse(msg.Key, msg.Value);
            }
        }

        private void OnResponse(uint id, MemoryStream stream)
        {
            try
            {
                Response(id, stream);
            }
            catch (Exception e)
            {
                Console.WriteLine("OnResponse:({0})[Error]{1}", id, e.ToString());
            }
        }
        protected virtual void Response(uint id, MemoryStream stream)
        {

        }

        public void Update()
        {
            OnProcessProtocal();
        }

        #endregion

        #region 发送队列
        IList<ArraySegment<byte>> _sendStreams = new List<ArraySegment<byte>>();
        IList<ArraySegment<byte>> _waitStreams = new List<ArraySegment<byte>>();

        public bool Send(MemoryStream stream)
        {
            //// Convert the string data to byte data using ASCII encoding.     
            //byte[] byteData = Encoding.ASCII.GetBytes(data);
            if (stream.Length == 0)
            {
                return true;
            }
            ArraySegment<byte> segment = new ArraySegment<byte>(stream.GetBuffer(), 0, (int)stream.Length);

            if (_sendStreams.Count == 0)
            {
                _sendStreams.Add(segment);
                try
                {
                    m_asyncSocketUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, m_asyncSocketUserToken.ConnectSocket);
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                _waitStreams.Add(segment);
            }
            return true;
        }

        public bool Send(MemoryStream head, MemoryStream body)
        {
            head.Seek(0, SeekOrigin.Begin);
            body.Seek(0, SeekOrigin.Begin);
            if (body.Length == 0)
            {
                return Send(head);
            }
            lock (this)
            {
                ArraySegment<byte> arrHead = new ArraySegment<byte>(head.GetBuffer(), 0, (int)head.Length);
                ArraySegment<byte> arrBody = new ArraySegment<byte>(body.GetBuffer(), 0, (int)body.Length);

                if (_sendStreams.Count == 0)
                {
                    _sendStreams.Add(arrHead);
                    _sendStreams.Add(arrBody);
                    try
                    {
                        m_asyncSocketUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, m_asyncSocketUserToken.ConnectSocket);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                else
                {
                    _waitStreams.Add(arrHead);
                    _waitStreams.Add(arrBody);
                }
            }
            return true;
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int len = handler.EndSend(ar);
                Console.WriteLine("Send {0} bytes.", len);

                _sendStreams.Clear();
                if (_waitStreams.Count > 0)
                {
                    IList<ArraySegment<byte>> temp = _sendStreams;
                    _sendStreams = _waitStreams;
                    _waitStreams = temp;
                    m_asyncSocketUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, handler);
                }
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion


        public virtual bool SendCompleted()
        {
            m_activeDT = DateTime.UtcNow;
            m_sendAsync = false;
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.ClearFirstPacket(); //清除已发送的包
            int offset = 0;
            int count = 0;
            if (asyncSendBufferManager.GetFirstPacket(ref offset, ref count))
            {
                m_sendAsync = true;
                return m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs,
                    asyncSendBufferManager.DynamicBufferManager.Buffer, offset, count);
            }
            else
                return SendCallback();
        }

        //发送回调函数，用于连续下发数据
        public virtual bool SendCallback()
        {
            return true;
        }

        public bool DoSendResult()
        {
            //string commandText = m_outgoingDataAssembler.GetProtocolText();
            //byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            //int totalLength = sizeof(int) + bufferUTF8.Length; //获取总大小
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            //asyncSendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            //asyncSendBufferManager.DynamicBufferManager.WriteInt(bufferUTF8.Length, false); //写入命令大小
            //asyncSendBufferManager.DynamicBufferManager.WriteBuffer(bufferUTF8); //写入命令内容
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs, 
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }                
            }
            return result;
        }

        public bool DoSendResult(byte[] buffer, int offset, int count)
        {
            //string commandText = m_outgoingDataAssembler.GetProtocolText();
            //byte[] bufferUTF8 = Encoding.UTF8.GetBytes(commandText);
            //int totalLength = sizeof(int) + bufferUTF8.Length + count; //获取总大小
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            //asyncSendBufferManager.DynamicBufferManager.WriteInt(totalLength, false); //写入总大小
            //asyncSendBufferManager.DynamicBufferManager.WriteInt(bufferUTF8.Length, false); //写入命令大小
            //asyncSendBufferManager.DynamicBufferManager.WriteBuffer(bufferUTF8); //写入命令内容
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count); //写入二进制数据
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs, 
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        public bool DoSendBuffer(byte[] buffer, int offset, int count)
        {
            AsyncSendBufferManager asyncSendBufferManager = m_asyncSocketUserToken.SendBuffer;
            asyncSendBufferManager.StartPacket();
            asyncSendBufferManager.DynamicBufferManager.WriteBuffer(buffer, offset, count);
            asyncSendBufferManager.EndPacket();

            bool result = true;
            if (!m_sendAsync)
            {
                int packetOffset = 0;
                int packetCount = 0;
                if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                {
                    m_sendAsync = true;
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncSocketUserToken.ConnectSocket, m_asyncSocketUserToken.SendEventArgs, 
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }
    }
}