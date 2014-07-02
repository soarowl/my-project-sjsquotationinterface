using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using ServiceManager.Utils;
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


        protected bool connected;

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
                this.receiveThreadExitEvent.Reset();
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
                    if (this.connected)
                        this.receiveThreadExitEvent.WaitOne();
                    this.tcpClient.Close();
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
            while (true)
            {
                Disconnect();
                TcpClient tcpClient = ConnectToServer();

                bool succeed = false;

                if (tcpClient != null)
                {
                    this.logHelper.LogInfoMsg(string.Format("Tcp连接成功，IP={0},Port={1}", this.config.IP, this.config.Port));
                    this.tcpClient = tcpClient;
                    this.tcpDisconnectedEvent.Reset();
                    Logon logonAns;
                    if (Logon(out logonAns))
                    {
                        succeed = true;
                    }
                }
                if (succeed)
                {
                    OnLogonSucceed();
                    int index = AutoResetEvent.WaitAny(new WaitHandle[] { this.stopEvent, this.tcpDisconnectedEvent });
                    if (index == 0)
                    {
                        break;
                    }
                }
                if (!succeed)
                {
                    this.logHelper.LogErrMsg(string.Format("Tcp连接失败，IP={0},Port={1}", this.config.IP, this.config.Port));
                    int index = AutoResetEvent.WaitAny(new WaitHandle[] { this.stopEvent }, this.config.ReconnectIntervalMS);
                    if (index == 0)
                    {
                        break;
                    }
                }
            }
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
            newTcpClient.ReceiveTimeout = 5;
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
            catch (Exception)
            {
                if (newTcpClient != null) newTcpClient.Close();
                if (result != null) result.AsyncWaitHandle.Close();
                return null;
            }
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
                this.logHelper.LogInfoMsg("收到Message，msgType={0},BodyLength={1}", messageData.Header.Type, messageData.Header.BodyLength);

                if (!Enum.IsDefined(typeof(MsgType), (int)messageData.Header.Type))
                    return null;

                if (messageData.Header.BodyLength > 0)
                {
                    messageData.BodyData = SocketReceive(this.tcpClient, (int)messageData.Header.BodyLength);
                    messageData.TailData = SocketReceive(this.tcpClient, tailSize);
                    if (messageData.Validate())
                    {
                        return messageData;
                    }
                    else
                    {
                        this.logHelper.LogWarnMsg("收到的Message校验错误");
                        return null;
                    }
                }
                else if (messageData.Header.Type == (UInt32)MsgType.Heartbeat)
                {
                    messageData.TailData = SocketReceive(this.tcpClient, tailSize);
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
        protected byte[] SocketReceive(TcpClient tcpClient, int length)
        {
            if (length < 0)
                return null;

            byte[] buffer = new byte[length];
            byte[] bodyBuff = new byte[length];
            int offset = 0;
            bool timeOut = false;
            System.Timers.Timer timer = null;
            int timeout = this.config.ConnectionTimeoutMS;
            if (timeout > 0)
            {
                timer = new System.Timers.Timer(timeout);
                timer.AutoReset = false;
                timer.Elapsed += (s, e) => { timeOut = true; };
                timer.Start();
            }
            bool succeed = false;
            while (true)
            {
                if (this.tcpDisconnectedEvent.WaitOne(0))//检查是否已断开连接
                    break;

                if (timeOut)
                {
                    this.logHelper.LogWarnMsg(string.Format("Tcp  接收数据超时，IP={0},Port={1}", this.config.IP, this.config.Port));
                    this.tcpDisconnectedEvent.Set();
                    break;
                }
                else if (tcpClient != null && tcpClient.Client != null && tcpClient.Client.Connected)
                {

                    if (tcpClient.Available > 0 && offset < length)
                    {

                        int received = tcpClient.Client.Receive(
                            bodyBuff,
                            offset,
                            Math.Min(tcpClient.Available, length - offset),
                             SocketFlags.None);

                        offset += received;
                        if (timeout > 0)
                        {
                            timer.Stop();
                            timer.Start();
                        }
                    }
                    else if (offset < length)
                    {
                        // Thread.Sleep(1);
                    }
                    else
                    {
                        succeed = true;
                        break;
                    }
                }
                else
                {
                    this.logHelper.LogWarnMsg(string.Format("行情网关连接断开，IP={0},Port={1}", this.config.IP, this.config.Port));
                    this.tcpDisconnectedEvent.Set();
                    break;
                }
            }
            if (timer != null)
            {
                try
                {
                    timer.Dispose();
                    timer = null;
                }
                catch { }
            }
            if (succeed)
                return bodyBuff;
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
        public event Action<IMarketData> OnMarketDataReceived;

        public RealTimeQuotConnection(ConnectionConfig config, Log4cb.ILog4cbHelper logHelper)
            : base(config, logHelper)
        {

        }

        /// <summary>
        /// 接收行情数据的线程
        /// </summary>
        protected Thread marketDataReceiveThread;


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
            while (true)
            {

                if (this.stopEvent.WaitOne(0))//检查停止事件是否发生
                    break;
                var msg = ReceiveMessage();
                if (msg != null)
                    ProcessMessage(msg);

            }
        }

        /// <summary>
        /// 处理行情数据
        /// </summary>
        /// <param name="msg"></param>
        private void ProcessMessage(MessagePack msg)
        {
            IMarketData marketData = null;
            if (msg.Header.Type == (uint)MsgType.QuotationSnap)
            {
                marketData = QuotSnap300111.Deserialize(msg.BodyData);

            }
            else if (msg.Header.Type == (uint)MsgType.Order)
            {
                marketData = Order300192.Deserialize(msg.BodyData);
            }
            RaiseEvent(marketData);
        }

        private void RaiseEvent(IMarketData marketData)
        {
            var handler = this.OnMarketDataReceived;
            if (handler != null)
                handler(marketData);
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
}
