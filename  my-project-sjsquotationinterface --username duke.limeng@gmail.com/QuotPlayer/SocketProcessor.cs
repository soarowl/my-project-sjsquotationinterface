using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics.Contracts;
using QuotV5.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
namespace QuotPlayer
{
    class SocketProcessor
    {

        public event EventHandler Disconnected;

        private TcpClient _tcpClient;
        private DateTime _lastSendDatetime;

        private ConcurrentQueue<byte[]> _sendingDataQueue = new ConcurrentQueue<byte[]>();

        private AutoResetEvent _newDataEvent = new AutoResetEvent(false);
        private ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private ManualResetEvent _sendThreadExitEvent = new ManualResetEvent(false);
        /// <summary>
        /// 接收行情的线程已退出
        /// </summary>
        protected ManualResetEvent receiveThreadExitEvent = new ManualResetEvent(false);

        private Thread _sendThread;
        private Thread _receiveThread;

        private string _ip;
        private int _port;
        private Log4cb.ILog4cbHelper logHelper;
        /// <summary>
        /// 消息包头长度
        /// </summary>
        protected static int messageHeaderLength = Marshal.SizeOf(typeof(StandardHeader));
        protected static int tailSize = Marshal.SizeOf(typeof(Trailer));

        /// <summary>
        /// 连接已断开
        /// </summary>
        protected ManualResetEvent tcpDisconnectedEvent = new ManualResetEvent(false);

        public SocketProcessor(TcpClient tcpClient, Log4cb.ILog4cbHelper logHelper)
        {
            Contract.Requires(tcpClient != null);
            this._tcpClient = tcpClient;
            this._ip = (tcpClient.Client.RemoteEndPoint as System.Net.IPEndPoint).Address.ToString();
            this._port = (tcpClient.Client.RemoteEndPoint as System.Net.IPEndPoint).Port;
            this.logHelper = logHelper;
        }

        public void Start()
        {
            this._stopEvent.Reset();
            this._newDataEvent.Reset();
            this._sendThreadExitEvent.Reset();
            createSendThread();
            createReceiveThread();
        }

       
        public void Stop()
        {
            this._stopEvent.Set();
            this._sendThreadExitEvent.WaitOne();
            if (this._tcpClient != null)
                this._tcpClient.Close();
        }
        private void createSendThread()
        {
            ThreadStart ts = new ThreadStart(sendProcessor);
            _sendThread = new Thread(ts);
            _sendThread.Start();
        }
        private void createReceiveThread()
        {
            ThreadStart ts = new ThreadStart(receiveProcessor);
            this._receiveThread = new Thread(ts);
            this._receiveThread.Start();
        }
        private void sendProcessor()
        {
            while (true)
            {
                if (this._stopEvent.WaitOne(0))
                    break;
                try
                {
                    if (!this._sendingDataQueue.IsEmpty
                        || this._newDataEvent.WaitOne())
                    {
                        var data = dequeue();
                        if (data != null && data.Any())
                            socketSend(data, data.Length);
                    }
                }
                catch (Exception ex)
                {

                }

            }
            _sendThreadExitEvent.Set();
        }
        
        private void receiveProcessor()
        {
            this.receiveThreadExitEvent.Reset();
            while (true)
            {
                var index = WaitHandle.WaitAny(new WaitHandle[] { this.tcpDisconnectedEvent }, 0);
                if (index == 0 )
                    break;
                var msg = ReceiveMessage();
                if (msg != null)
                    // EnqueueMessage(msg);
                    ProcessMessage(msg);
            }
            this.receiveThreadExitEvent.Set();
        }

        private void ProcessMessage(MessagePack msg)
        {
            throw new NotImplementedException();
        }

        private byte[] dequeue()
        {
            byte[] data;
            if (this._sendingDataQueue.TryDequeue(out data))
            {
                return data;
            }
            else
                return null;
        }

        private bool socketSend(byte[] data, Int32 bufferSize)
        {
            lock (_tcpClient)
            {
                if (_tcpClient.Client == null || !_tcpClient.Client.Connected)
                    throw new SocketException();

                Int32 offset = 0;

                while (offset < data.Length)
                {
                    if (!_tcpClient.Client.Poll(1000, SelectMode.SelectWrite))
                    {
                        if (_tcpClient.Client == null || !_tcpClient.Client.Connected)
                            throw new SocketException();

                        continue;
                    }
                    int size = Math.Min(bufferSize, data.Length - offset);
                    Int32 nSend = _tcpClient.Client.Send(data, offset, size, SocketFlags.None);
                    offset += nSend;
                    _lastSendDatetime = DateTime.Now;
                }

                return true;
            }
        }

        private void onDisconnected()
        {
            EventHandler handler = this.Disconnected;
            if (handler != null)
                handler(this, null);
        }
        
        public void SendData(byte[] data)
        {
            if (!IsConnected)
                return;
            _sendingDataQueue.Enqueue(data);
            this._newDataEvent.Set();
        }
        
        public bool IsConnected
        {
            get
            {
                if (this._tcpClient != null && this._tcpClient.Client != null && this._tcpClient.Connected)
                    return true;
                else
                    return false;
            }
        }






        /// <summary>
        /// 接收消息
        /// </summary>
        /// <returns></returns>
        protected MessagePack ReceiveMessage()
        {
            MessagePack messageData = null;
            try
            {
                byte[] headerBytes = SocketReceive(this._tcpClient, messageHeaderLength);

                if (headerBytes == null || headerBytes.Length == 0)
                    return null;

                messageData = new MessagePack(headerBytes);
                this.logHelper.LogInfoMsg("收到Message，msgType={0},BodyLength={1},threadId={2}", messageData.Header.Type, messageData.Header.BodyLength, Thread.CurrentThread.ManagedThreadId);

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
                    messageData.BodyData = SocketReceive(this._tcpClient, (int)messageData.Header.BodyLength);
                    messageData.TrailerData = SocketReceive(this._tcpClient, tailSize);
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
                    messageData.TrailerData = SocketReceive(this._tcpClient, tailSize);
                    return messageData;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                this.logHelper.LogWarnMsg(ex, string.Format("行情网关连接断开，IP={0},Port={1}", this._ip, this._port));
                this.tcpDisconnectedEvent.Set();
                return null;
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
            int timeout = 6000;

            bool succeed = false;
            while (true)
            {
                if (timeout > 0 && sw.ElapsedMilliseconds > timeout)
                {
                    ex = new TimeoutException(string.Format("TCP 接收数据超时，IP={0},Port={1}", this._ip, this._port));
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
                    ex = new Exception(string.Format("TCP 连接断开，IP={0},Port={1}", this._ip, this._port));
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

    }
}
