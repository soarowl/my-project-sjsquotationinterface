using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
namespace QuotPlayer
{
    class SocketServer
    {
        public event EventHandler Connected;
        public event EventHandler Disconnected;

        private List<SocketProcessor> _socketProcessors = new List<SocketProcessor>();
        private TcpListener _serverListener;
        private int _port;
        private Thread _lisenThread;
        private Log4cb.ILog4cbHelper logHelper;
        public SocketServer(string ipAddress,int port,Log4cb.ILog4cbHelper logHelper)
        {
            this._port = port;
            this.ServerIPEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            this.logHelper = logHelper;
        }

        public bool Start()
        {
            try
            {
                this._serverListener = new TcpListener(ServerIPEndPoint);
                this._serverListener.ExclusiveAddressUse = true;
                this._serverListener.Start();
                createListenThread();
                return true;
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "Start server listener failed :{0}", ServerIPEndPoint);

                if (this._serverListener != null)
                {try
                    {
                    this._serverListener.Stop();
}
                    catch { }
                    this._serverListener = null;
                }
                return false;
            }
        }
        public void Stop()
        {
            if (this._serverListener != null)
            {try
                {
                this._serverListener.Stop();
                 }
                catch { }
            }
            lock (this._socketProcessors)
            {
                foreach (var p in this._socketProcessors)
                {
                    p.Stop();
                }
            }
        }

        public void Broadcast(byte[] data)
        {
            foreach (var p in this._socketProcessors)
            {
                try
                {
                    p.SendData(data);
                }
                catch (Exception ex)
                {
                    this.logHelper.LogErrMsg(ex, "Broadcast data failed");
                }
            }
        }

        private void createListenThread()
        {
            ThreadStart ts = new ThreadStart(listenProcessor);
            this._lisenThread = new Thread(ts);
            this._lisenThread.Start();
        }

        private void listenProcessor()
        {
            try
            {
                while (true)
                {
                    //本地TcpListener 接受新的请求
                    TcpClient newTcpClient = this._serverListener.AcceptTcpClient();

                    //检查client连接状态是否正常
                    if (newTcpClient == null || newTcpClient.Client == null)
                        continue;

                    //如果没有连接正常，则等待一下，再检查
                    if (!newTcpClient.Client.Connected)
                    {
                        Thread.Sleep(0);
                        try { newTcpClient.Client.Close(); }
                        catch (Exception) { }
                        continue;
                    }
                    onAccepted(newTcpClient);
                }
            }
            catch (SocketException err)
            {
                this.logHelper.LogErrMsg(err, "Server listener exception, ErrorCode :{0}", err.ErrorCode);
            }
            catch (Exception err)
            {
                this.logHelper.LogErrMsg(err, "Server listener exception");
            }
        }

        private void onAccepted(TcpClient tcpClient)
        {
            SocketProcessor processor = new SocketProcessor(tcpClient, logHelper);
            processor.Disconnected += new EventHandler(processor_Disconnected);
            processor.Start();
            lock (this._socketProcessors)
            {
                this._socketProcessors.Add(processor);
            }
            onConnected();
        }

        void processor_Disconnected(object sender, EventArgs e)
        {
            SocketProcessor p = sender as SocketProcessor;
            p.Disconnected -= processor_Disconnected;
            lock (this._socketProcessors)
            {
                if (this._socketProcessors.Contains(p))
                    this._socketProcessors.Remove(p);
            }
            onDisconnected();
        }



        private void onConnected()
        {
            EventHandler handler = this.Connected;
            if (handler != null)
                handler(this, null);
        }
        private void onDisconnected()
        {
            EventHandler handler = this.Disconnected;
            if (handler != null)
                handler(this, null);
        }

        public IPEndPoint ServerIPEndPoint { get; private set; }
        public int ConnectionCount
        {
            get
            {
                return _socketProcessors.Count;
            }
        }
    }
}
