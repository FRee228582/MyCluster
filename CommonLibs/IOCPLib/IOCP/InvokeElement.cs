using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace IOCPLib.IOCP
{
    //异步Socket调用对象，所有的协议处理都从本类继承
    public class InvokeElement
    {
        protected AsyncSocketServer m_asyncSocketServer;

        protected AsyncUserToken m_asyncUserToken;
        protected AsyncUserToken AsyncUserToken { get { return m_asyncUserToken; } }

        protected bool m_sendAsync; //标识是否有发送异步事件

        protected DateTime m_connectDT;
        public DateTime ConnectDT { get { return m_connectDT; } }
        protected DateTime m_activeDT;
        public DateTime ActiveDT { get { return m_activeDT; } }

        public InvokeElement(AsyncSocketServer asyncSocketServer, AsyncUserToken asyncUserToken)
        {
            m_asyncSocketServer = asyncSocketServer;
            m_asyncUserToken = asyncUserToken;

            //m_incomingDataParser = new IncomingDataParser();
            //m_outgoingDataAssembler = new OutgoingDataAssembler();

            m_sendAsync = false;

            m_connectDT = DateTime.UtcNow;
            m_activeDT = DateTime.UtcNow;
        }
        public void Update()
        {
            OnProcessProtocal();
        }

        public virtual void Close()
        {
            //frTODO：关闭
            throw new NotImplementedException();
        }

        internal bool ProcessReceive(byte[] buffer, int offset, int count)//接收异步事件返回的数据，用于对数据进行缓存和分包等处理
        {
            m_activeDT = DateTime.UtcNow;
            DynamicBufferManager receiveBuffer = m_asyncUserToken.ReceiveBuffer;

            receiveBuffer.WriteBuffer(buffer, offset, count);
            if (receiveBuffer.DataCount >= sizeof(UInt16)+sizeof(UInt32))
            {
                bool result = ProcessPacket(receiveBuffer.Buffer, offset,count);
                if (result)
                    receiveBuffer.Clear(count); //从缓存中清理
                return result;
            }
            return true;
        }

        protected virtual bool ProcessPacket(byte[] buffer, int offset, int count) //处理分完包后的数据，把命令和数据分开，并对命令进行解析
        {
            while (count - offset >= sizeof(UInt16))
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
            throw new NotImplementedException();
        }

        #endregion

        #region 发送SendAsync

        internal bool SendCompleted()
        {
            m_activeDT = DateTime.UtcNow;
            m_sendAsync = false;
            AsyncSendBufferManager asyncSendBufferManager = m_asyncUserToken.SendBuffer;
            asyncSendBufferManager.ClearFirstPacket(); //清除已发送的包
            int offset = 0;
            int count = 0;
            if (asyncSendBufferManager.GetFirstPacket(ref offset, ref count))
            {
                m_sendAsync = true;
                return m_asyncSocketServer.SendAsyncEvent(m_asyncUserToken.ConnectSocket, m_asyncUserToken.SendEventArgs,
                    asyncSendBufferManager.DynamicBufferManager.Buffer, offset, count);
            }
            else
            {
                return SendCallback();
            }
        }

        public bool Send(MemoryStream head , MemoryStream body)
        {
            head.Seek(0, SeekOrigin.Begin);
            body.Seek(0, SeekOrigin.Begin);
            if (body.Length == 0)
            {
                return Send(head);
            }
            lock (this)
            {
                AsyncSendBufferManager asyncSendBufferManager = m_asyncUserToken.SendBuffer;
                asyncSendBufferManager.StartPacket();
                asyncSendBufferManager.DynamicBufferManager.WriteBuffer(head.GetBuffer(), 0, (int)head.Length);
                asyncSendBufferManager.DynamicBufferManager.WriteBuffer(body.GetBuffer(), 0, (int)body.Length);
                asyncSendBufferManager.EndPacket();

                bool result = true;
                if (!m_sendAsync)
                {
                    int packetOffset = 0;
                    int packetCount = 0;
                    if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                    {
                        m_sendAsync = true;
                        result = m_asyncSocketServer.SendAsyncEvent(m_asyncUserToken.ConnectSocket, m_asyncUserToken.SendEventArgs,
                            asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                        return result;
                    }
                }
            }
            return true;
        }

        public bool Send(MemoryStream stream)
        { 
            if (stream.Length ==0)
            {
                return true;
            }
            else
            {
                AsyncSendBufferManager asyncSendBufferManager = m_asyncUserToken.SendBuffer;
                asyncSendBufferManager.StartPacket();
                asyncSendBufferManager.DynamicBufferManager.WriteBuffer(stream.GetBuffer(), 0, (int)stream.Length);
                asyncSendBufferManager.EndPacket();
                bool result = true;
                if (!m_sendAsync)
                {
                    int packetOffset = 0;
                    int packetCount = 0;
                    if (asyncSendBufferManager.GetFirstPacket(ref packetOffset, ref packetCount))
                    {
                        m_sendAsync = true;
                        result = m_asyncSocketServer.SendAsyncEvent(m_asyncUserToken.ConnectSocket, m_asyncUserToken.SendEventArgs,
                            asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                        return result;
                    }
                }

            }
            return true ;
        }
        
        public bool Send(byte[] buffer, int offset, int count)
        {
            AsyncSendBufferManager asyncSendBufferManager = m_asyncUserToken.SendBuffer;
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
                    result = m_asyncSocketServer.SendAsyncEvent(m_asyncUserToken.ConnectSocket, m_asyncUserToken.SendEventArgs,
                        asyncSendBufferManager.DynamicBufferManager.Buffer, packetOffset, packetCount);
                }
            }
            return result;
        }

        //发送回调函数，用于连续下发数据
        public virtual bool SendCallback()
        {
            return true;
        }

        #endregion

        #region 发送BeginSend

        IList<ArraySegment<byte>> _sendStreams = new List<ArraySegment<byte>>();
        IList<ArraySegment<byte>> _waitStreams = new List<ArraySegment<byte>>();

        public bool Write(MemoryStream stream)
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
                    m_asyncUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, m_asyncUserToken.ConnectSocket);
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

        public bool Write(MemoryStream head, MemoryStream body)
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
                        m_asyncUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, m_asyncUserToken.ConnectSocket);
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
                    m_asyncUserToken.ConnectSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, handler);
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

    }
}