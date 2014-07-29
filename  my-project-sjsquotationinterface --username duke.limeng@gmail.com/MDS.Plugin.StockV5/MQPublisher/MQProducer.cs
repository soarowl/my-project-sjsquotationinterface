using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.Util;

namespace MDS.Plugin.SZQuotV5
{
    public class MQProducer
    {
        private MQConnConfig config;
        private Log4cb.ILog4cbHelper logHelper;
        /// <summary>
        /// ApacheMQ连接对象
        /// </summary>
        private IConnection Connection;

        /// <summary>
        /// ApacheMQ消息生成器
        /// </summary>
        private IMessageProducer Producer;
        public MQProducer(MQConnConfig config, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;

        }

        /// <summary>
        /// 实现启动MQ连接方法
        /// </summary>
        public void StartMQ()
        {
            try
            {
                //连接工厂创建连接
                string address = string.Format("failover:(tcp://{0})?randomize=false", this.config.Address);
                ConnectionFactory connectionFactory = new ConnectionFactory(address);

                //JMS 客户端到JMS Provider 的连接,是客户端与消息服务的活动连接
                Connection = connectionFactory.CreateConnection();


                Connection.ExceptionListener += new ExceptionListener(OnConnectionException);
                Connection.ConnectionInterruptedListener += new ConnectionInterruptedListener(OnConnectionInterrupted);
                Connection.ConnectionResumedListener += new ConnectionResumedListener(OnConnectionResumed);

                //判断connection状态，连接connection
                if (!Connection.IsStarted)
                    Connection.Start();
                this.logHelper.LogInfoMsg("MQProducer启动成功");
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "MQProducer启动异常}");
                throw ex;
            }
        }

        /// <summary>
        /// 实现停止MQ连接方法
        /// </summary>
        public void StopMQ()
        {
            try
            {
                if (Connection != null)
                {
                    //停止
                    if (Connection.IsStarted)
                        Connection.Stop();
                    //关闭
                    Connection.Close();

                    //释放
                    Connection.Dispose();

                    Connection = null;

                    this.logHelper.LogInfoMsg("MQProducer停止成功");
                }
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "MQProducer停止异常");
                throw ex;
            }
        }


        /// <summary>
        /// 实现发送数据方法
        /// </summary>
        public bool SendMsg(MQMsgType msgType, string topicName, byte[] data, IDictionary<string, object> properties, int timeToLiveMS, out string msgId)
        {
            try
            {
                msgId = "";
                if (Connection == null)
                {
                    this.logHelper.LogInfoMsg("MQProducer获取到的连接为空，可能是该MQ控件没有启动");
                    return false;
                }

                if (!Connection.IsStarted)
                {
                    this.logHelper.LogInfoMsg("MQProducer没有连接到MQ");
                    return false;
                }

                //建立ISession，会话，一个发送或接收消息的线程
                ISession session = Connection.CreateSession();

                //创建Producer接受消息
                if (msgType == MQMsgType.TOPIC)
                {

                    ITopic topic = SessionUtil.GetTopic(session, topicName);
                    Producer = session.CreateProducer(topic);
                }
                else if (msgType == MQMsgType.QUEUE)
                {
                    IQueue queue = SessionUtil.GetQueue(session, topicName);
                    Producer = session.CreateProducer(queue);
                }

                //持久化
                Producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                //创建消息
                IBytesMessage ibytesMessage = session.CreateBytesMessage();
                if (properties != null)
                {
                    //设置消息属性
                    foreach (KeyValuePair<string, object> pair in properties)
                    {
                        ibytesMessage.Properties[pair.Key] = pair.Value;
                    }
                }
                if (data != null && data.Length > 0)
                    ibytesMessage.WriteBytes(data);

                //发送超时时间,如果timeToLive == 0 则永远不过期
                if (timeToLiveMS != 0)
                {
                    Producer.TimeToLive = TimeSpan.FromMilliseconds((double)timeToLiveMS);
                }

                //向MQ发送消息
                Producer.Send(ibytesMessage);
                msgId = ibytesMessage.NMSMessageId;
                return true;
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "MQProducer发送消息异常");
                msgId = string.Empty;
                return false;
            }
        }


        /// <summary>
        /// 实现发送数据方法
        /// </summary>
        public bool SendMsg(MQMsgType msgType, string topicName, string data, IDictionary<string, object> properties, int timeToLiveMS, out string msgId)
        {
            try
            {
                msgId = "";
                if (Connection == null)
                {
                    this.logHelper.LogInfoMsg("MQProducer获取到的连接为空，可能是该MQ控件没有启动");
                    return false;
                }

                if (!Connection.IsStarted)
                {
                    this.logHelper.LogInfoMsg("MQProducer没有连接到MQ");
                    return false;
                }

                //建立ISession，会话，一个发送或接收消息的线程
                ISession session = Connection.CreateSession();

                //创建Producer接受消息
                if (msgType == MQMsgType.TOPIC)
                {

                    ITopic topic = SessionUtil.GetTopic(session, topicName);
                    Producer = session.CreateProducer(topic);
                }
                else if (msgType == MQMsgType.QUEUE)
                {
                    IQueue queue = SessionUtil.GetQueue(session, topicName);
                    Producer = session.CreateProducer(queue);
                }

                //持久化
                Producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                //创建消息
                ITextMessage iTextMessage = session.CreateTextMessage();
                if (properties != null)
                {
                    //设置消息属性
                    foreach (KeyValuePair<string, object> pair in properties)
                    {
                        iTextMessage.Properties[pair.Key] = pair.Value;
                    }
                }

                iTextMessage.Text = data;

             
               
                //向MQ发送消息
                if (timeToLiveMS == 0)
                {
                    Producer.Send(iTextMessage);
                }
                else
                {
                    Producer.Send(iTextMessage, MsgDeliveryMode.NonPersistent, MsgPriority.Normal, TimeSpan.FromMilliseconds((double)timeToLiveMS));
                }
                msgId = iTextMessage.NMSMessageId;
                return true;
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "MQProducer发送消息异常");
                msgId = string.Empty;
                return false;
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
}
