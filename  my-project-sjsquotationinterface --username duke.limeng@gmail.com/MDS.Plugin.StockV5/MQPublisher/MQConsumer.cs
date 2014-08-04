using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;
using System.Collections.Concurrent;
using System.Threading;

namespace MDS.Plugin.SZQuotV5
{
    public class MQConsumer
    {
        public event Action<string, Dictionary<string, object>> OnMessageReceived;

        private MQConnConfig config;
        private Log4cb.ILog4cbHelper logHelper;

        /// <summary>
        /// ApacheMQ连接对象
        /// </summary>
        private IConnection connection;

        /// <summary>
        /// 接收数据队列
        /// </summary>
        private ConcurrentQueue<MQMsgPack> msgQueue = new ConcurrentQueue<MQMsgPack>();

        private AutoResetEvent newDataEvent = new AutoResetEvent(false);
        private ManualResetEvent stopEvent = new ManualResetEvent(false);
        private Thread processThread;

        public MQConsumer(MQConnConfig config, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;

        }


        private static object sync = new object();

        /// <summary>
        /// 实现启动MQ连接方法
        /// </summary>
        public void StartMQ()
        {
            lock (sync)
            {
                try
                {
                    //不重复启动
                    if (IsRuning())
                        return;

                    //创建MQ连接
                    string address = string.Format("failover:(tcp://{0})?randomize=false", this.config.Address);
                    ConnectionFactory connectionFactory = new ConnectionFactory(address);


                    connection = connectionFactory.CreateConnection();


                    connection.ExceptionListener += new ExceptionListener(OnConnectionException);
                    connection.ConnectionInterruptedListener += new ConnectionInterruptedListener(OnConnectionInterrupted);
                    connection.ConnectionResumedListener += new ConnectionResumedListener(OnConnectionResumed);

                    //判断connection状态，连接connection
                    if (!connection.IsStarted)
                        connection.Start();
                    this.stopEvent.Reset();
                    StartMsgProcessThread();
                    this.logHelper.LogInfoMsg("MQConsumer启动成功,Address={0}", this.config.Address);
                }
                catch (Exception ex)
                {
                    this.logHelper.LogErrMsg(ex, "MQConsumer启动异常,Address={0}", this.config.Address);
                    throw ex;
                }
            }
        }

        /// <summary>
        /// 实现停止MQ连接方法
        /// </summary>
        public void StopMQ()
        {
            try
            {
                if (!IsRuning())
                    return;

                //msgRecv.Clear();
                if (connection != null)
                {
                    if (connection.IsStarted)
                        connection.Stop();

                    connection.Close();

                    connection.Dispose();

                    //重新订阅时是判断对象是否为空来决定要不要重新连接，发现不为空没有重新连接ActiveMQ
                    //修改：将connection对象设置为null
                    connection = null;
                    this.stopEvent.Set();
                    while (!msgQueue.IsEmpty)
                    {
                        MQMsgPack msgPack;
                        msgQueue.TryDequeue(out msgPack);
                    }
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "MQConsumer停止异常");
                throw ex;
            }
        }


        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsRuning()
        {
            if (connection != null)
                return connection.IsStarted;

            return false;
        }


        private void StartMsgProcessThread()
        {
            this.processThread = new Thread(new ThreadStart(LoopProcess));
            this.processThread.Start();
        }

        private void LoopProcess()
        {

            while (true)
            {
                if (this.stopEvent.WaitOne(0))
                    break;
                if (!(this.msgQueue == null || this.msgQueue.IsEmpty)
                         || this.newDataEvent.WaitOne())
                {

                    ProcessMessage();
                }
            }
        }


        private void ProcessMessage()
        {
            if (this.msgQueue == null)
                return;
            MQMsgPack pack;
            if (this.msgQueue.TryDequeue(out pack))
            {
                var handler = this.OnMessageReceived;
                if (handler != null)
                    handler(pack.MsgData, pack.Properties);
            }
        }

        /// <summary>
        /// 相应接收MQ消息
        /// </summary>
        protected void OnMessage(IMessage message)
        {
            try
            {
                string str = string.Empty;
                if (message is ITextMessage)
                {
                    ITextMessage btMessage = message as ITextMessage;

                    //不同类型
                  str = btMessage.Text;
                }
                Dictionary<string, object> properties = new Dictionary<string, object>();
                //获取所有属性并保存到属性表中
                foreach (object obj in message.Properties.Keys)
                {
                    if (message.Properties.Contains(obj))
                    {
                        string key = obj.ToString();
                        properties.Add(key, message.Properties[key]);
                    }
                }
                OnRecvMsg(str, properties);

            }
            catch (Exception ex)
            {

            }
        }


        /// <summary>
        /// 响应接收数据
        /// </summary>
        protected void OnRecvMsg(string msgData, Dictionary<string, object> properties)
        {
            try
            {
                MQMsgPack msgPack = new MQMsgPack();

                msgPack.MsgData = msgData;



                msgPack.Properties = properties;
                //将接收到的消息压入接收队列
                msgQueue.Enqueue(msgPack);
                this.newDataEvent.Set();
            }
            catch (System.Exception ex)
            {

            }
        }


        /// <summary>
        /// 订阅主题信息
        /// </summary>
        public bool SubscribeMsg(MQMsgType msgType, string topicName, string filter = null)
        {
            try
            {
                if (!connection.IsStarted)
                {
                    return false;
                }


                //建立Session
                ISession session = connection.CreateSession();

                IMessageConsumer consumer = null;

                //创建Consumer接受消息
                if (msgType == MQMsgType.TOPIC)
                {
                    ITopic topic = SessionUtil.GetTopic(session, topicName);

                    if (String.IsNullOrEmpty(filter))
                        consumer = session.CreateConsumer(topic);
                    else
                        consumer = session.CreateConsumer(topic, filter);
                }
                else if (msgType == MQMsgType.QUEUE)
                {
                    IQueue queue = SessionUtil.GetQueue(session, topicName);

                    if (String.IsNullOrEmpty(filter))
                        consumer = session.CreateConsumer(queue);
                    else
                        consumer = session.CreateConsumer(queue, filter);
                }

                consumer.Listener += new MessageListener(OnMessage);

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 响应MQ连接异常消息
        /// </summary>
        protected void OnConnectionException(Exception exception)
        {

        }

        /// <summary>
        /// 响应MQ连接断开消息
        /// </summary>
        protected void OnConnectionInterrupted()
        {

        }

        /// <summary>
        /// 响应MQ连接恢复消息
        /// </summary>
        protected void OnConnectionResumed()
        {

        }


    }
    public class MQMsgPack
    {
        /// <summary>
        /// 发送/接收的数据包
        /// </summary>
        public string MsgData;

        /// <summary>
        /// 发送/接收的MQ消息属性队列
        /// </summary>
        public Dictionary<string, object> Properties;
    }
}
