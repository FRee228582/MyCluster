using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;

namespace IOCPLib.IOCP
{
    internal class AsyncUserToken
    {
        internal Socket _Socket;
        internal SocketAsyncEventArgs _sendEventArgs;

        public AsyncUserToken()
        {

        }

        IList<MemoryStream> _sendStreams = new List<MemoryStream>();
        IList<MemoryStream> _waitStreams = new List<MemoryStream>();

        public bool Send(MemoryStream stream)
        {
            //// Convert the string data to byte data using ASCII encoding.     
            //byte[] byteData = Encoding.ASCII.GetBytes(data);
            if (stream.Length == 0)
            {
                return true;
            }

            if (_sendStreams.Count == 0)
            {
                _sendStreams.Add(stream);
                try
                {
                    //SocketAsyncEventArgs sendEventArgs = _readWritePool.Pop();
                    //Socket s = sendEventArgs.AcceptSocket;
                    _sendEventArgs.SetBuffer(stream.GetBuffer(), 0, (int)stream.Length);

                    if (!_Socket.SendAsync(_sendEventArgs))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件  
                    {
                        // 同步发送时处理发送完成事件  
                        ProcessSend(_sendEventArgs);
                    }
                    else
                    {
                        CloseClientSocket(_sendEventArgs);
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            }
            else
            {
                _waitStreams.Add(stream);
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
                    _sendStreams.Add(head);
                    _sendStreams.Add(body);
                    try
                    {
                        //_workSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, _workSocket);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
                else
                {
                    _waitStreams.Add(head);
                    _waitStreams.Add(body);
                }
            }
            return true;
        }

        //public bool Send(ArraySegment<byte> head, ArraySegment<byte> body)
        //{
        //    lock (this)
        //    {
        //        if (_sendStreams.Count == 0)
        //        {
        //            _sendStreams.Add(head);
        //            _sendStreams.Add(body);
        //            try
        //            {
        //                //_workSocket.BeginSend(_sendStreams, SocketFlags.None, SendCallback, null);
        //            }
        //            catch (Exception e)
        //            {
        //                return false;
        //            }
        //        }
        //        else
        //        {
        //            _waitStreams.Add(head);
        //            _waitStreams.Add(body);
        //        }
        //    }
        //    return true;
        //}

        //#region 发送数据

        ///// <summary>  
        ///// 异步的发送数据  
        ///// </summary>  
        ///// <param name="e"></param>  
        ///// <param name="data"></param>  
        //public void Send(SocketAsyncEventArgs e, byte[] data)
        //{
        //    if (e.SocketError == SocketError.Success)
        //    {
        //        Socket s = e.AcceptSocket;//和客户端关联的socket  
        //        if (s.Connected)
        //        {
        //            Array.Copy(data, 0, e.Buffer, 0, data.Length);//设置发送数据  

        //            //e.SetBuffer(data, 0, data.Length); //设置发送数据  
        //            if (!s.SendAsync(e))//投递发送请求，这个函数有可能同步发送出去，这时返回false，并且不会引发SocketAsyncEventArgs.Completed事件  
        //            {
        //                // 同步发送时处理发送完成事件  
        //                ProcessSend(e);
        //            }
        //            else
        //            {
        //                CloseClientSocket(e);
        //            }
        //        }
        //    }
        //}

        ///// <summary>  
        ///// 同步的使用socket发送数据  
        ///// </summary>  
        ///// <param name="socket"></param>  
        ///// <param name="buffer"></param>  
        ///// <param name="offset"></param>  
        ///// <param name="size"></param>  
        ///// <param name="timeout"></param>  
        //public void Send(Socket socket, byte[] buffer, int offset, int size, int timeout)
        //{
        //    socket.SendTimeout = 0;
        //    int startTickCount = Environment.TickCount;
        //    int sent = 0; // how many bytes is already sent  
        //    do
        //    {
        //        if (Environment.TickCount > startTickCount + timeout)
        //        {
        //            //throw new Exception("Timeout.");  
        //        }
        //        try
        //        {
        //            sent += socket.Send(buffer, offset + sent, size - sent, SocketFlags.None);
        //        }
        //        catch (SocketException ex)
        //        {
        //            if (ex.SocketErrorCode == SocketError.WouldBlock ||
        //            ex.SocketErrorCode == SocketError.IOPending ||
        //            ex.SocketErrorCode == SocketError.NoBufferSpaceAvailable)
        //            {
        //                // socket buffer is probably full, wait and try again  
        //                Thread.Sleep(30);
        //            }
        //            else
        //            {
        //                throw ex; // any serious error occurr  
        //            }
        //        }
        //    } while (sent < size);
        //}

        /// <summary>  
        /// 发送完成时处理函数  
        /// </summary>  
        /// <param name="e">与发送完成操作相关联的SocketAsyncEventArg对象</param>  
        private void ProcessSend(SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                Socket s = (Socket)e.UserToken;

                //TODO  
            }
            else
            {
                CloseClientSocket(e);
            }
        }

        private void CloseClientSocket(SocketAsyncEventArgs e)
        {
            throw new NotImplementedException();
        }

        //#endregion


    }
}