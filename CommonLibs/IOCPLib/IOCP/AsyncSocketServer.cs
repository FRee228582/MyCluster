using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace IOCPLib.IOCP
{
    public abstract class AsyncSocketServer
    {
        #region Properties

        /// <summary>  
        /// 服务器是否正在运行  
        /// </summary>  
        public bool IsRunning { get; private set; }
        /// <summary>  
        /// 监听的IP地址  
        /// </summary>  
        public IPAddress Address { get; private set; }
        /// <summary>  
        /// 监听的端口  
        /// </summary>  
        public int Port { get; private set; }

        #endregion

        #region Fields

        public static int g_totalBytesRead=0;
        public static int g_totalStreamCount=0;
        public static DateTime start;
        public static DateTime stop;

        public void GetQPS()
        {
            stop = DateTime.Now;

            double t = (stop - start).TotalMilliseconds;
            double averageQPS = (g_totalStreamCount / t) * 1000.00;
            double averageBytePerSec = (g_totalStreamCount / t) * 1000.00;
            Console.WriteLine("QPS  streamCount={0} bytes ={1} ({2} ms average {3} client)", g_totalStreamCount, g_totalBytesRead,t, AsyncUserTokenList.GetLength());
            start = stop;
            g_totalBytesRead = 0;
            g_totalStreamCount = 0;
           
        }


        /// <summary>  
        /// 监听Socket，用于接受客户端的连接请求  
        /// </summary>  
        private Socket _listenSocket;

        /// <summary>  
        /// 服务器程序允许的最大客户端连接数  
        /// </summary>  
        private int _maxNumConnections;

        /// <summary>  
        /// 用于每个I/O Socket操作的缓冲区大小  
        /// </summary>  
        private int _receiveBufferSize = 1024 * 4;

        /// <summary>  
        /// 限制访问接收连接的线程数，用来控制最大并发数
        /// </summary>  
        Semaphore _maxNumberAcceptedClients;

        private int m_socketTimeOutMS; //Socket最大超时时间，单位为MS
        public int SocketTimeOutMS { get { return m_socketTimeOutMS; } set { m_socketTimeOutMS = value; } }
        //private DaemonThread m_daemonThread;

        private AsyncUserTokenPool _asyncUserTokenPool;
        private AsyncUserTokenList _asyncUserTokenConnectedList;  //正在连接列表
        public AsyncUserTokenList AsyncUserTokenList { get { return _asyncUserTokenConnectedList; } }

        #endregion

        #region Ctors

        /// <summary>  
        /// 异步IOCP SOCKET服务器  
        /// </summary>  
        /// <param name="listenPort">监听的端口</param>  
        /// <param name="maxClient">最大的客户端数量</param>  
        public AsyncSocketServer(int listenPort, int maxClients)
            : this(IPAddress.Any, listenPort, maxClients)
        {


        }

        /// <summary>  
        /// 异步Socket TCP服务器  
        /// </summary>  
        /// <param name="localEP">监听的终结点</param>  
        /// <param name="maxClient">最大客户端数量</param>  
        public AsyncSocketServer(IPEndPoint localEP, int maxClients)
            : this(localEP.Address, localEP.Port, maxClients)
        {
        }

        /// <summary>  
        /// 异步Socket TCP服务器  
        /// </summary>  
        /// <param name="localIPAddress">监听的IP地址</param>  
        /// <param name="listenPort">监听的端口</param>  
        /// <param name="maxClient">最大客户端数量</param>  
        public AsyncSocketServer(IPAddress localIPAddress, int listenPort, int maxClients)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;

            _maxNumConnections = maxClients;
            _receiveBufferSize = 1024 * 4;

            _asyncUserTokenPool = new AsyncUserTokenPool(_maxNumConnections);
            _asyncUserTokenConnectedList = new AsyncUserTokenList();
            _maxNumberAcceptedClients = new Semaphore(_maxNumConnections, _maxNumConnections);
        }

        #endregion

        #region 回调函数

        /// <summary>  
        /// 当Socket上的发送或接收请求被完成时，调用此函数  
        /// </summary>  
        /// <param name="sender">激发事件的对象</param>  
        /// <param name="e">与发送或接收完成操作相关联的SocketAsyncEventArg对象</param>  
        private void OnIOCompleted(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            AsyncUserToken userToken = asyncEventArgs.UserToken as AsyncUserToken;
            userToken.ActiveDateTime = DateTime.Now;
            try
            {
                lock (userToken)
                { // Determine which type of operation just completed and call the associated handler.  
                    switch (asyncEventArgs.LastOperation)
                    {
                        case SocketAsyncOperation.Receive:
                            ProcessReceive(asyncEventArgs);
                            break;
                        case SocketAsyncOperation.Send:
                            ProcessSend(asyncEventArgs);
                            break;
                        default:
                            throw new ArgumentException("The last operation completed on the socket was not a receive or send");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("IO_Completed {0} error, message: {1}", userToken.ConnectSocket, e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        #endregion

        #region Init

        /// <summary>  
        /// 初始化函数  
        /// </summary>  
        public void Init()
        {
            AsyncUserToken userToken;
            for (int i = 0; i < _maxNumConnections; i++) //按照连接数建立读写对象
            {
                userToken = new AsyncUserToken(_receiveBufferSize);
                userToken.ReceiveEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                userToken.SendEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnIOCompleted);
                _asyncUserTokenPool.Push(userToken);
            }
        }

        #endregion

        #region Start
        /// <summary>  
        /// 启动  
        /// </summary>  
        public void Start()
        {
            if (!IsRunning)
            {
                Init();
                IsRunning = true;
                IPEndPoint localEndPoint = new IPEndPoint(Address, Port);
                // 创建监听socket  
                _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                if (localEndPoint.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    // 配置监听socket为 dual-mode (IPv4 & IPv6)   
                    // 27 is equivalent to IPV6_V6ONLY socket option in the winsock snippet below,  
                    _listenSocket.SetSocketOption(SocketOptionLevel.IPv6, (SocketOptionName)27, false);
                    _listenSocket.Bind(new IPEndPoint(IPAddress.IPv6Any, localEndPoint.Port));
                }
                else
                {
                    _listenSocket.Bind(localEndPoint);
                }
                // 开始监听  
                //_listenSocket.Listen(100); //frTODO: 不同
                _listenSocket.Listen(this._maxNumConnections);
                // 在监听Socket上投递一个接受请求。  
                StartAccept(null);
            }
        }

        private void Start(IPEndPoint localEndPoint)
        {
            _listenSocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(localEndPoint);
            _listenSocket.Listen(_maxNumConnections);  //frTODO:这里的backlog设置多少？？有疑问。
            Console.WriteLine("Start listen socket {0} success", localEndPoint.ToString());
            StartAccept(null);
            //m_daemonThread = new DaemonThread(this); //frTODO: 这里这个守护进程 目前还没研究
        }
        #endregion

        #region Stop

        /// <summary>  
        /// 停止服务  
        /// </summary>  
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _listenSocket.Close();
                //frTODO：关闭对所有客户端的连接  

            }
        }

        #endregion

        #region Update
        public void Update()
        {
           _asyncUserTokenConnectedList.Update();
        }

        #endregion

        #region Accept

        /// <summary>  
        /// 从客户端开始接受一个连接操作  
        /// </summary>  
        private void StartAccept(SocketAsyncEventArgs acceptEventArg)
        {
            if (acceptEventArg == null)
            {
                acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);
            }
            else
            {
                //socket must be cleared since the context object is being reused  
                acceptEventArg.AcceptSocket = null;
            }
            _maxNumberAcceptedClients.WaitOne();
            bool willRaiseEvent = _listenSocket.AcceptAsync(acceptEventArg);
            if (!willRaiseEvent)
            {
                ProcessAccept(acceptEventArg);
                //frTODO:
                //如果I/O挂起等待异步则触发AcceptAsyn_Asyn_Completed事件  
                //此时I/O操作同步完成，不会触发Asyn_Completed事件，所以指定BeginAccept()方法  
            }
        }

        /// <summary>  
        /// accept 操作完成时回调函数  
        /// </summary>  
        /// <param name="sender">Object who raised the event.</param>  
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>  
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs acceptEventArgs)
        {
            try
            {
                ProcessAccept(acceptEventArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Accept client {0} error, message: {1}", acceptEventArgs.AcceptSocket, e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        /// <summary>  
        /// 监听Socket接受处理  
        /// </summary>  
        /// <param name="e">SocketAsyncEventArg associated with the completed accept operation.</param>  
        private void ProcessAccept(SocketAsyncEventArgs acceptEventArgs)
        {
            if (acceptEventArgs.SocketError == SocketError.Success)
            {
                Console.WriteLine("Client connection accepted. Local Address: {0},  Remote Client Address: {1}",
               acceptEventArgs.AcceptSocket.LocalEndPoint, acceptEventArgs.AcceptSocket.RemoteEndPoint);

                if (acceptEventArgs.AcceptSocket.Connected)//和客户端关联的socket  
                {
                    AsyncUserToken userToken = _asyncUserTokenPool.Pop();
                    userToken.ConnectDateTime = DateTime.Now;
                    userToken.ConnectSocket = acceptEventArgs.AcceptSocket;
                    _asyncUserTokenConnectedList.Add(userToken); //添加到已经保持连接列表

                    try
                    {
                        // As soon as the client is connected, post a receive to the connection
                        bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs);
                        if (!willRaiseEvent)
                        {
                            lock (userToken)
                            {
                                ProcessReceive(userToken.ReceiveEventArgs);
                            }
                        }
                    }
                    catch (Exception e)//catch (SocketException e)
                    {
                        Console.WriteLine("Accept client {0} error, message: {1}", userToken.ConnectSocket, e.Message);
                        Console.WriteLine(e.StackTrace);    //TODO 异常处理  
                    }
                    //投递下一个接受请求  
                    StartAccept(acceptEventArgs);
                }
            }
        }

        #endregion

        #region Recv

        /// <summary>  
        ///接收完成时处理函数  
        /// </summary>  
        /// <param name="e">与接收完成操作相关联的SocketAsyncEventArg对象</param>  
        private void ProcessReceive(SocketAsyncEventArgs receiveEventArgs)
        {
            // check if the remote host closed the connection
            AsyncUserToken userToken = receiveEventArgs.UserToken as AsyncUserToken;
            if (userToken.ConnectSocket == null)
            {
                Console.WriteLine("ProcessReceive userToken.ConnectSocket is null");
                return;
            }
            if (receiveEventArgs.SocketError == SocketError.Success)
            {
                // 检查远程主机是否关闭连接  
                if (userToken.ReceiveEventArgs.BytesTransferred > 0)
                {
                    //判断所有需接收的数据是否已经完成  
                    if (userToken.ConnectSocket.Available == 0)
                    {
                        userToken.ActiveDateTime = DateTime.Now;
                        //从侦听者获取接收到的消息。   
                        int offset = userToken.ReceiveEventArgs.Offset;
                        int count = userToken.ReceiveEventArgs.BytesTransferred;

                        if ((userToken.InvokeElement == null) & (userToken.ConnectSocket != null)) //存在Socket对象，并且没有绑定协议对象，则进行协议对象绑定
                        {
                            BuildingSocketInvokeElement(userToken);
                        }
                        if (userToken.InvokeElement == null) //如果没有解析对象，提示非法连接并关闭连接
                        {
                            Console.WriteLine("Illegal client connection. Local Address: {0}, Remote Address: {1}",
                                userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
                            CloseClientSocket(userToken);
                        }
                        else
                        {
                            if (count > 0)
                            {
                                if (!userToken.InvokeElement.ProcessReceive(userToken.ReceiveEventArgs.Buffer, offset, count))
                                {
                                    //如果处理数据返回失败，则断开连接
                                    CloseClientSocket(userToken);
                                }
                                else //否则投递下次接收数据请求
                                {
                                    bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                                    if (!willRaiseEvent)
                                    {
                                        ProcessReceive(userToken.ReceiveEventArgs);
                                    }
                                }
                            }
                            else
                            {
                                bool willRaiseEvent = userToken.ConnectSocket.ReceiveAsync(userToken.ReceiveEventArgs); //投递接收请求
                                if (!willRaiseEvent)
                                {
                                    ProcessReceive(userToken.ReceiveEventArgs);
                                }
                            }
                        }
                    }
                }
                else
                {
                    CloseClientSocket(userToken);
                }
            }
            else
            {
                CloseClientSocket(userToken);
            }
        }

        protected virtual void BuildingSocketInvokeElement(AsyncUserToken userToken)
        {
            byte flag = userToken.ReceiveEventArgs.Buffer[userToken.ReceiveEventArgs.Offset];

            switch (flag)
            {
                default:
                    userToken.InvokeElement = new InvokeElement(this, userToken); //frTODO:做一个基础的默认的协议处理类
                    break;
            }
            if (userToken.InvokeElement != null)
            {
                Console.WriteLine("Building socket invoke element {0}.Local Address: {1}, Remote Address: {2}",
                    userToken.InvokeElement, userToken.ConnectSocket.LocalEndPoint, userToken.ConnectSocket.RemoteEndPoint);
            }
        }

        #endregion

        #region Send

        private bool ProcessSend(SocketAsyncEventArgs sendEventArgs)
        {
            AsyncUserToken userToken = sendEventArgs.UserToken as AsyncUserToken;
            if (userToken.InvokeElement == null)
            {
                return false;
            }
            if (userToken.ConnectSocket == null)
            {
                Console.WriteLine("ProcessSend userToken.ConnectSocket is null");
                return false;
            }
            if (sendEventArgs.SocketError == SocketError.Success)
            {
                userToken.ActiveDateTime = DateTime.Now;
                return userToken.InvokeElement.SendCompleted(); //调用子类回调函数
            }
            else
            {
                CloseClientSocket(userToken);
                return false;
            }
        }

        public bool SendAsyncEvent(Socket connectSocket, SocketAsyncEventArgs sendEventArgs, byte[] buffer, int offset, int count)
        {
            if (connectSocket == null)
            {
                return false;
            }
            sendEventArgs.SetBuffer(buffer, offset, count);
            bool willRaiseEvent = connectSocket.SendAsync(sendEventArgs);
            if (!willRaiseEvent)
            {
                return ProcessSend(sendEventArgs);
            }
            else
                return true;
        }

        #endregion

        #region Close

        /// <summary>
        /// 关闭socket连接
        /// </summary>
        /// <param name="userToken"></param>
        private void CloseClientSocket(AsyncUserToken userToken)
        {
            if (userToken.ConnectSocket == null)
                return;
            string socketInfo = string.Format("Local Address: {0} Remote Address: {1}",
                userToken.ConnectSocket.LocalEndPoint,userToken.ConnectSocket.RemoteEndPoint);
            Console.WriteLine("Client connection disconnected. {0}", socketInfo);
            try
            {
                userToken.ConnectSocket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e)
            {
                Console.WriteLine("CloseClientSocket Disconnect client {0} error, message: {1}", socketInfo, e.Message);
            }
            userToken.ConnectSocket.Close();
            userToken.ConnectSocket = null; //释放引用，并清理缓存，包括释放协议对象等资源

            _maxNumberAcceptedClients.Release();
            _asyncUserTokenPool.Push(userToken);
            _asyncUserTokenConnectedList.Remove(userToken);
        }

        #endregion
    }
}
