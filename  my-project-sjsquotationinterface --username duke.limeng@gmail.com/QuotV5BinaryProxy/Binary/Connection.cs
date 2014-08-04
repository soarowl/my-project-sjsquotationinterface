using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ServiceManager.Utils;
using System.Collections.Concurrent;
using System.Diagnostics;
namespace QuotV5.Binary
{
    public class ConnectionBase
    {
        protected ConnectionConfig config;
        protected Log4cb.ILog4cbHelper logHelper;
        protected object connSyncObj = new object();
        protected static int tailSize = Marshal.SizeOf(typeof(Trailer));


        /// <summary>
        /// 消息包头长度
        /// </summary>
        protected static int messageHeaderLength = Marshal.SizeOf(typeof(StandardHeader));

        /// <summary>
        /// 控制连接的线程
        /// </summary>
        protected Thread connThread;


        protected Thread heartbeatThread;

        /// <summary>
        /// 停止
        /// </summary>
        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        /// <summary>
        /// 连接已断开
        /// </summary>
        protected ManualResetEvent tcpDisconnectedEvent = new ManualResetEvent(false);

        /// <summary>
        /// 接收行情的线程已退出
        /// </summary>
        protected ManualResetEvent receiveThreadExitEvent = new ManualResetEvent(false);

        /// <summary>
        /// TCP连接
        /// </summary>
        protected TcpClient tcpClient;


        protected bool marketDataReceiveThreadStarted = false;

        public ConnectionBase(ConnectionConfig config, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;
        }

        public void Start()
        {
            lock (this.connSyncObj)
            {
                if (this.CurrentStatus == Status.Started)
                    return;
                this.logHelper.LogInfoMsg("Connection Starting");
                this.CurrentStatus = Status.Starting;
                this.stopEvent.Reset();
                this.tcpDisconnectedEvent.Reset();
                // this.receiveThreadExitEvent.Reset();
                CreateConnectionThread();
                this.CurrentStatus = Status.Started;
                this.logHelper.LogInfoMsg("Connection Started");
            }
        }

        public void Stop()
        {
            lock (this.connSyncObj)
            {
                if (this.CurrentStatus == Status.Stopped)
                    return;
                try
                {
                    this.logHelper.LogInfoMsg("Connection Stopping");
                    this.CurrentStatus = Status.Stopping;
                    this.stopEvent.Set();
                    if (this.marketDataReceiveThreadStarted)
                        this.receiveThreadExitEvent.WaitOne();
                    Disconnect();
                }
                catch (Exception ex)
                {
                    this.logHelper.LogErrMsg(ex, "Connection Stopping Exception");
                }
                finally
                {
                    this.CurrentStatus = Status.Stopped;
                    this.logHelper.LogInfoMsg("Connection Stopped");
                }
            }
        }


        protected void CreateConnectionThread()
        {
            this.connThread = new Thread(new ThreadStart(ConnAndLogon));
            this.connThread.Name = "ConnThread";
            this.connThread.Start();
            Thread.Sleep(0);
        }


        /// <summary>
        /// 连接到行情网关
        /// </summary>
        protected void ConnAndLogon()
        {
            this.logHelper.LogInfoMsg("ConnThread线程启动");
            while (true)
            {
                Disconnect();
                if (this.stopEvent.WaitOne(0))
                    break;
                if (this.marketDataReceiveThreadStarted)
                {
                    int index = WaitHandle.WaitAny(new WaitHandle[] { this.stopEvent, this.receiveThreadExitEvent });
                    if (index == 0)
                        break;
                }

                TcpClient tcpClient = ConnectToServer();

                if (tcpClient != null)
                {
                    this.logHelper.LogInfoMsg(string.Format("Tcp连接成功，IP={0},Port={1}", this.config.IP, this.config.Port));
                    this.tcpClient = tcpClient;
                    this.tcpDisconnectedEvent.Reset();
                    Logon logonAns;
                    if (Logon(out logonAns))
                    {
                        OnLogonSucceed();
                        int index = AutoResetEvent.WaitAny(new WaitHandle[] { this.stopEvent, this.tcpDisconnectedEvent });
                        if (index == 0)
                        {
                            break;
                        }
                    }
                    else
                    {
                        int index = AutoResetEvent.WaitAny(new WaitHandle[] { this.stopEvent, this.tcpDisconnectedEvent }, this.config.ReconnectIntervalMS);
                        if (index == 0)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    this.logHelper.LogWarnMsg(string.Format("Tcp连接失败，IP={0},Port={1}", this.config.IP, this.config.Port));
                    int index = AutoResetEvent.WaitAny(new WaitHandle[] { this.stopEvent }, this.config.ReconnectIntervalMS);
                    if (index == 0)
                    {
                        break;
                    }
                }
            }
            this.logHelper.LogInfoMsg("ConnThread线程退出");
        }


        protected virtual void OnLogonSucceed()
        {
            CreateHeartbeatThread();
        }

        #region 心跳发送
        protected void CreateHeartbeatThread()
        {
            if (this.heartbeatThread != null)
            {
                try
                {
                    this.heartbeatThread.Abort();
                }
                catch { }
            }
            this.heartbeatThread = new Thread(new ThreadStart(SendHeartbeat));
            this.heartbeatThread.Name = "HeartbeatThread";
            this.heartbeatThread.Start();
            Thread.Sleep(0);
        }
        protected void SendHeartbeat()
        {
            Heartbeat heartbeat = new Heartbeat() { };
            int heartbeatInterval = this.config.HeartbeatIntervalS * 1000;
            byte[] msgBytes = MessageHelper.ComposeMessage<Heartbeat>(heartbeat);
            while (true)
            {
                if (this.tcpDisconnectedEvent.WaitOne(heartbeatInterval))//检查是否已断开连接
                    break;
                bool succeed = SocketSend(msgBytes);
                this.logHelper.LogDebugMsg("发送心跳 Succeed={0}", succeed);
            }
        }
        #endregion


        /// <summary>
        /// 关闭TCP连接
        /// </summary>
        protected void Disconnect()
        {
            if (this.tcpClient != null)
            {
                try
                {
                    this.tcpClient.Client.Disconnect(false);
                    this.tcpClient.Close();
                    this.tcpClient = null;
                }
                catch (Exception ex)
                {
                    this.logHelper.LogWarnMsg(ex, "关闭Tcp连接异常，Ip={0},Port={1}", this.config.IP, this.config.Port);
                }
            }
        }


        /// <summary>
        /// 连接服务器
        ///     采用异步方式等待连接事件，避免因服务器未开启而耗费过长时间等待
        /// </summary>
        protected TcpClient ConnectToServer()
        {
            int milliSecondsTimeout = this.config.ConnectionTimeoutMS;
            TcpClient newTcpClient = new TcpClient();
            newTcpClient.Client.ReceiveBufferSize = 1024 * 1024 * 100;
            newTcpClient.NoDelay = true;
            newTcpClient.ReceiveTimeout = milliSecondsTimeout;
            newTcpClient.SendTimeout = milliSecondsTimeout;
            newTcpClient.Client.IOControl(IOControlCode.KeepAliveValues, KeepAlive(1, 10000, 3000), null);
            IAsyncResult result = null;

            try
            {
                result = newTcpClient.BeginConnect(this.config.IP, this.config.Port, null, null);
                result.AsyncWaitHandle.WaitOne(milliSecondsTimeout, false);

                if (result.IsCompleted)
                {
                    newTcpClient.EndConnect(result);
                    result.AsyncWaitHandle.Close();
                    return newTcpClient;
                }
                else
                {
                    newTcpClient.EndConnect(result);

                    newTcpClient.Close();
                    result.AsyncWaitHandle.Close();
                    return null;
                }
            }
            catch (Exception ex)
            {
                if (newTcpClient != null) newTcpClient.Close();
                if (result != null) result.AsyncWaitHandle.Close();
                return null;
            }
        }

        private byte[] KeepAlive(int onOff, int keepAliveTime, int keepAliveInterval)
        {
            byte[] buffer = new byte[12];
            BitConverter.GetBytes(onOff).CopyTo(buffer, 0);
            BitConverter.GetBytes(keepAliveTime).CopyTo(buffer, 4);
            BitConverter.GetBytes(keepAliveInterval).CopyTo(buffer, 8);
            return buffer;
        }
        #region 登录

        protected bool Logon(out Logon logonAnswer)
        {
            bool sendLogonSucceed = SendLogonRequest();

            if (sendLogonSucceed)
            {
                var answer = ReceiveLogonAnswer();
                if (answer != null)
                {
                    this.logHelper.LogInfoMsg("行情网关登录成功");
                    logonAnswer = answer.Value;
                    return true;
                }
                else
                {
                    this.logHelper.LogInfoMsg("行情网关登录失败");
                    logonAnswer = default(Logon);
                    return false;
                }
            }
            else
            {
                logonAnswer = default(Logon);
                return false;
            }
        }


        /// <summary>
        /// 发送登录请求
        /// </summary>
        /// <returns></returns>
        protected bool SendLogonRequest()
        {
            Logon logonReq = new Binary.Logon()
            {
                SenderCompID = this.config.SenderCompID,
                TargetCompID = this.config.TargetCompID,
                Password = this.config.Password,
                HeartBtInt = this.config.HeartbeatIntervalS,
                DefaultApplVerID = "1.00            "
                //DefaultApplVerID = "1.00"
            };
            var bytes = MessageHelper.ComposeMessage<Logon>(logonReq);
            return SocketSend(bytes);
        }


        /// <summary>
        /// 接收登录应答
        /// </summary>
        /// <returns></returns>
        protected Logon? ReceiveLogonAnswer()
        {
            var ans = ReceiveMessage();


            if (ans != null && ans.Header.Type == (UInt32)MsgType.Logon)
            {
                var loginAnswer = BigEndianStructHelper<Logon>.BytesToStruct(ans.BodyData, MsgConsts.MsgEncoding);
                return loginAnswer;
            }
            else if (ans != null && ans.Header.Type == (UInt32)MsgType.Logout)
            {
                var logout = BigEndianStructHelper<Logout>.BytesToStruct(ans.BodyData, MsgConsts.MsgEncoding);
                this.logHelper.LogErrMsg("登录失败：{0}", logout.Text);
                return null;
            }
            else
                return null;
        }







        protected bool Logout()
        {
            bool sendLogoutSucceed = SendLogoutRequest();

            if (sendLogoutSucceed)
            {
                var answer = ReceiveLogoutAnswer();
                if (answer != null)
                {
                    this.logHelper.LogInfoMsg("行情网关登出成功");
                    return true;
                }
                else
                {
                    this.logHelper.LogInfoMsg("行情网关登出失败");
                    return false;
                }
            }
            else
            {

                return false;
            }
        }

        /// <summary>
        /// 发送登录请求
        /// </summary>
        /// <returns></returns>
        protected bool SendLogoutRequest()
        {
            Logout logoutReq = new Binary.Logout()
            {
            };
            var bytes = MessageHelper.ComposeMessage<Logout>(logoutReq);
            bool succeed = SocketSend(bytes);
            this.logHelper.LogInfoMsg("发送Logout,Succeed={0}", succeed);
            return succeed;
        }
        /// <summary>
        /// 接收登录应答
        /// </summary>
        /// <returns></returns>
        protected Logout? ReceiveLogoutAnswer()
        {
            var ans = ReceiveMessage();


            if (ans != null && ans.Header.Type == (UInt32)MsgType.Logout)
            {
                var logout = BigEndianStructHelper<Logout>.BytesToStruct(ans.BodyData, MsgConsts.MsgEncoding);
                this.logHelper.LogErrMsg("登出成功：{0}", logout.Text);
                return null;
            }
            else
                return null;
        }

        #endregion






        /// <summary>
        /// 接收消息
        /// </summary>
        /// <returns></returns>
        protected MessagePack ReceiveMessage()
        {
            MessagePack messageData = null;
            try
            {
                byte[] headerBytes = SocketReceive(this.tcpClient, messageHeaderLength);

                if (headerBytes == null || headerBytes.Length == 0)
                    return null;

                messageData = new MessagePack(headerBytes);
                this.logHelper.LogInfoMsg("收到Message，msgType={0},BodyLength={1},threadId={2},Port={3}", messageData.Header.Type, messageData.Header.BodyLength, Thread.CurrentThread.ManagedThreadId, this.config.Port);

                //if (!Enum.IsDefined(typeof(MsgType), (int)messageData.Header.Type))
                //    return null;

                if (messageData.Header.BodyLength > 2000)
                {
                    this.logHelper.LogWarnMsg("数据包头中指示的数据长度异常");
                    this.tcpDisconnectedEvent.Set();
                    return null;
                }

                if (messageData.Header.BodyLength > 0)
                {
                    messageData.BodyData = SocketReceive(this.tcpClient, (int)messageData.Header.BodyLength);
                    messageData.TrailerData = SocketReceive(this.tcpClient, tailSize);
                    string msg;
                    if (messageData.Validate(out msg))
                    {
                        return messageData;
                    }
                    else
                    {
                        this.logHelper.LogWarnMsg("验证消息：{0},Message:\r\n{1}", msg, messageData.ToLogString());
                        return null;
                    }
                }
                else if (messageData.Header.Type == (UInt32)MsgType.Heartbeat)
                {
                    messageData.TrailerData = SocketReceive(this.tcpClient, tailSize);
                    return messageData;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                this.logHelper.LogWarnMsg(ex, string.Format("行情网关连接断开，IP={0},Port={1}", this.config.IP, this.config.Port));
                this.tcpDisconnectedEvent.Set();
                return null;
            }
        }

        #region Socket收发数据
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="data"></param>
        protected bool SocketSend(byte[] data)
        {
            try
            {
                this.tcpClient.Client.Send(data);
                return true;
            }
            catch (Exception ex)
            {
                this.logHelper.LogWarnMsg(ex, "TCP发送数据异常");
                return false;
            }
        }

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="length"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        private byte[] SocketReceive(TcpClient tcpClient, int length)
        {
            byte[] buffer = new byte[length];
            byte[] bodyBuff = new byte[length];
            int offset = 0;

            Stopwatch sw = Stopwatch.StartNew();

            Exception ex = null;
            int timeout = this.config.ConnectionTimeoutMS;

            bool succeed = false;
            while (true)
            {
                if (timeout > 0 && sw.ElapsedMilliseconds > timeout)
                {
                    ex = new TimeoutException(string.Format("TCP 接收数据超时，IP={0},Port={1}", this.config.IP, this.config.Port));
                    break;
                }
                else if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
                {
                    int available = tcpClient.Available;
                    if (available > 0 && offset < length)
                    {
                        int received = tcpClient.Client.Receive(
                            bodyBuff,
                            offset,
                            Math.Min(available, length - offset),
                             SocketFlags.None);

                        offset += received;
                        if (timeout > 0)
                        {
                            sw.Restart();
                        }
                    }
                    else if (offset < length)
                    {
                    }
                    else
                    {
                        succeed = true;
                        break;
                    }
                }
                else
                {
                    ex = new Exception(string.Format("TCP 连接断开，IP={0},Port={1}", this.config.IP, this.config.Port));
                    break;
                }
            }

            if (succeed)
                return bodyBuff;
            else if (ex != null)
                throw ex;
            else
                return null;

        }

        #endregion


        public Status CurrentStatus { get; private set; }

        /// <summary>
        /// 表示当前状态
        /// </summary>
        public enum Status
        {
            Starting,
            Started,
            Stopping,
            Stopped
        }
    }

    public class RealTimeQuotConnection : ConnectionBase
    {
        public event Action<MarketDataEx> OnMarketDataReceived;

        private ConcurrentQueue<MessagePackEx> messageQueue = new ConcurrentQueue<MessagePackEx>();

        private IMessagePackRecorder msgPackRecorder;
        /// <summary>
        /// 接收行情数据的线程
        /// </summary>
        protected Thread marketDataReceiveThread;



        public RealTimeQuotConnection(ConnectionConfig config, Log4cb.ILog4cbHelper logHelper, IMessagePackRecorder msgPackRecorder = null)
            : base(config, logHelper)
        {
            this.msgPackRecorder = msgPackRecorder;
        }




        /// <summary>
        /// 成功登录后
        /// </summary>
        protected override void OnLogonSucceed()
        {
            base.OnLogonSucceed();
            StartMarketDataReceiveThread();
        }


        /// <summary>
        /// 启动线程接收行情数据
        /// </summary>
        protected void StartMarketDataReceiveThread()
        {
            if (this.marketDataReceiveThread != null)
            {
                try
                {
                    this.marketDataReceiveThread.Abort();
                }
                catch { }
            }

            this.marketDataReceiveThread = new Thread(new ThreadStart(ReceiveMarketData));
            this.marketDataReceiveThread.Name = "MarketDataReceiveThread";
            this.marketDataReceiveThread.Start();
        }

        /// <summary>
        /// 接收行情数据
        /// </summary>
        private void ReceiveMarketData()
        {
            this.receiveThreadExitEvent.Reset();
            this.marketDataReceiveThreadStarted = true;
            this.logHelper.LogInfoMsg("MarketDataReceiveThread线程启动");
            while (true)
            {
                var index = WaitHandle.WaitAny(new WaitHandle[] { this.stopEvent, this.tcpDisconnectedEvent }, 0);
                if (index == 0 || index == 1)
                    break;
                var msg = ReceiveMessage();
                if (msg != null)
                    // EnqueueMessage(msg);
                    ProcessMessage(msg);
            }

            this.logHelper.LogInfoMsg("MarketDataReceiveThread线程退出");
            this.receiveThreadExitEvent.Set();
            this.marketDataReceiveThreadStarted = false;
        }


        private void EnqueueMessage(MessagePack msg)
        {
            this.messageQueue.Enqueue(new MessagePackEx(msg, DateTime.Now));
        }

        /// <summary>
        /// 处理行情数据
        /// </summary>
        /// <param name="msg"></param>
        /// TODO:考虑把收到的数据包加入队列异步处理
        private void ProcessMessage(MessagePack msg)
        {
            DateTime now = DateTime.Now;
            IMarketData marketData = null;
            if (msg.Header.Type == (uint)MsgType.ChannelHeartbeat)
            {
                ChannelHeartbeat heartbeat = ChannelHeartbeat.Deserialize(msg.BodyData);
                logMarketData<ChannelHeartbeat>(heartbeat);
            }
            else if (msg.Header.Type == (uint)MsgType.RealtimeStatus)
            {
                marketData = RealtimeStatus.Deserialize(msg.BodyData);
                logMarketData<RealtimeStatus>(marketData as RealtimeStatus);
            }
            else if (msg.Header.Type == (uint)MsgType.QuotationSnap)
            {
                marketData = QuotSnap300111.Deserialize(msg.BodyData);
                logMarketData<QuotSnap300111>(marketData as QuotSnap300111);
            }
            else if (msg.Header.Type == (uint)MsgType.IndexQuotationSnap)
            {
                marketData = QuotSnap309011.Deserialize(msg.BodyData);
                logMarketData<QuotSnap309011>(marketData as QuotSnap309011);
            }
            else if (msg.Header.Type == (uint)MsgType.Order)
            {
                marketData = Order300192.Deserialize(msg.BodyData);
                logMarketData<Order300192>((Order300192)marketData);
            }
            if (marketData != null)
            {
                logMarketData(marketData);
                RaiseEvent(marketData, now);
            }
            if (this.msgPackRecorder != null)
            {
                this.msgPackRecorder.Record(new MessagePackEx(msg, now));
            }
        }



        private void logMarketData<TMarketData>(TMarketData marketData)
        {
            string str = ObjectLogHelper<TMarketData>.ObjectToString(marketData);
            logHelper.LogDebugMsg("收到数据：\r\n{0}", str);
        }

        private void RaiseEvent(IMarketData marketData, DateTime receiveTime)
        {
            var handler = this.OnMarketDataReceived;
            if (handler != null)
                handler(new MarketDataEx(marketData, receiveTime));
        }
    }

    public class ResendQuotConnection : ConnectionBase
    {

        public ResendQuotConnection(ConnectionConfig config, Log4cb.ILog4cbHelper logHelper)
            : base(config, logHelper)
        {

        }

        protected override void OnLogonSucceed()
        {
            base.OnLogonSucceed();
        }
    }

    public class ConnectionConfig
    {
        public IPAddress IP { get; set; }
        public int Port { get; set; }
        public string SenderCompID { get; set; }
        public string TargetCompID { get; set; }
        public string Password { get; set; }
        public int ConnectionTimeoutMS { get; set; }
        public int ReconnectIntervalMS { get; set; }
        public int HeartbeatIntervalS { get; set; }
    }

    public class MessagePackEx : Tuple<MessagePack, DateTime>
    {
        public MessagePackEx(MessagePack messagePack, DateTime receiveTime) : base(messagePack, receiveTime) { }

        public MessagePack MessagePack { get { return this.Item1; } }

        public DateTime ReceiveTime { get { return this.Item2; } }
    }
    public class MarketDataEx : Tuple<IMarketData, DateTime>
    {
        public MarketDataEx(IMarketData marketData, DateTime receiveTime)
            : base(marketData, receiveTime)
        { }

        public IMarketData MarketData { get { return this.Item1; } }
        public DateTime ReceiveTime { get { return this.Item2; } }
    }
}
