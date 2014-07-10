using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MDS.Plugin.SZQuotV5
{
    public class QuotationMQPublisher : IDisposable
    {
        private QuotationMQPublisherConfig config;
        private Log4cb.ILog4cbHelper logHelper;
        private MQProducer mqProducer;
        private System.Collections.Concurrent.ConcurrentQueue<Msg> msgQueue = new System.Collections.Concurrent.ConcurrentQueue<Msg>();

        private AutoResetEvent newDataEvent = new AutoResetEvent(false);
        private ManualResetEvent stopEvent = new ManualResetEvent(false);
        private Thread processThread;
        public QuotationMQPublisher(QuotationMQPublisherConfig config, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;
            this.mqProducer = new MQProducer(new MQConnConfig() { Address = config.Address }, logHelper);
            this.StartProcessThread();
            this.mqProducer.StartMQ();
        }

        //public void Enqueue(StockInfo stockInfo, bool isRealtimeQuotation, string clientId = null)
        //{
        //    var msg = new Msg() { ClientId = clientId, Data = stockInfo };
        //    if (isRealtimeQuotation)
        //        msg.MsgType = MQMsgType.TOPIC;
        //    else
        //        msg.MsgType = MQMsgType.QUEUE;
        //    msgQueue.Enqueue(msg);
        //    this.newDataEvent.Set();
        //}

        public void Enqueue(QuotationInfo quotInfo, bool isRealtimeQuotation, string clientId = null)
        {
            var msg = new Msg() { ClientId = clientId, Data = quotInfo };
            if (isRealtimeQuotation)
            {
                msg.MsgType = MQMsgType.TOPIC;
                msg.Topic = "SZ5_Quotation_RealTime";
            }
            else
            {
                msg.MsgType = MQMsgType.QUEUE;
                msg.Topic = "SZ5_Quotation_Image";
            }
            msgQueue.Enqueue(msg);
            this.newDataEvent.Set();
        }

        public void Enqueue(IEnumerable<StockInfo> stockInfos, string clientId = null)
        {
            var msg = new Msg() { ClientId = clientId, Data = stockInfos };
            msg.MsgType = MQMsgType.QUEUE;
            msg.Topic = "SZ5_StkInfo_Image";
            msgQueue.Enqueue(msg);
            this.newDataEvent.Set();
        }

        public void Enqueue(IEnumerable<QuotationInfo> quotInfos, bool isRealtimeQuotation, string clientId = null)
        {
            var msg = new Msg() { ClientId = clientId, Data = quotInfos.ToList() };
            if (isRealtimeQuotation)
            {
                msg.MsgType = MQMsgType.TOPIC;
                msg.Topic = "SZ5_Quotation_RealTime";
            }
            else
            {
                msg.MsgType = MQMsgType.QUEUE;
                msg.Topic = "SZ5_Quotation_Image";
            }
            msgQueue.Enqueue(msg);
            this.newDataEvent.Set();
        }


        public void Dispose()
        {
            this.stopEvent.Set();
            this.processThread = null;
            this.msgQueue = null;
        }

        private void StartProcessThread()
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
                    PublishMsg();
                }
            }
        }

        private void PublishMsg()
        {
            if (this.msgQueue == null)
                return;
            Msg msg;
            if (this.msgQueue.TryDequeue(out msg))
            {
                if (msg.Data is List<StockInfo>)
                    Publish<StockInfo>(msg.Data as List<StockInfo>, msg.ClientId, msg.MsgType, msg.Topic);
                else if (msg.Data is List<QuotationInfo>)
                    Publish<QuotationInfo>(msg.Data as List<QuotationInfo>, msg.ClientId, msg.MsgType, msg.Topic);
                else if (msg.Data is StockInfo)
                {
                    List<StockInfo> stockList = new List<StockInfo>() { msg.Data as StockInfo };
                    Publish<StockInfo>(stockList, msg.ClientId, msg.MsgType, msg.Topic);
                }
                else if (msg.Data is QuotationInfo)
                {
                    List<QuotationInfo> quotList = new List<QuotationInfo>() { msg.Data as QuotationInfo };
                    Publish<QuotationInfo>(quotList, msg.ClientId, msg.MsgType, msg.Topic);
                }
            }
        }

        private void Publish<T>(List<T> data, string clientId, MQMsgType msgType, string topic, bool sendFinishMsg = false)
        {
            if (data == null || data.Count == 0)
                return;
            Dictionary<string, object> properties = new Dictionary<string, object>();
            properties["refreshType"] = "STK";
            if (!string.IsNullOrEmpty(clientId))
            {
                properties["clientId"] = clientId;
            }
            string msgId;
            for (int index = 0; index < data.Count; index += this.config.RecordsPerPackage)
            {
                var subList = data.Skip(index).Take(this.config.RecordsPerPackage).ToList();
                // var dataBytes = JsonSerializer.ObjectToBytes<List<T>>(subList);
                var dataStr = JsonSerializer.ObjectToString<List<T>>(subList);
                bool succeed = this.mqProducer.SendMsg(msgType, topic, dataStr, properties, 2000, out msgId);
                if (succeed)
                {
                    this.logHelper.LogInfoMsg("向MQ发送数据成功，MsgId={0},数据类型={1},数量={2}", msgId, typeof(T).Name, subList.Count);
                }
                else
                {
                    this.logHelper.LogInfoMsg("向MQ发送数据失败，MsgId={0},数据类型={1},数量={2}", msgId, typeof(T).Name, subList.Count);
                }
            }
            properties["refreshSignal"] = "Finish";

            bool s = this.mqProducer.SendMsg(msgType, topic, string.Empty, properties, 2000, out msgId);
            this.logHelper.LogInfoMsg("向MQ发送数据结束消息，MsgId={0},数据类型={1},Succeed={2}", msgId, typeof(T).Name, s);

        }



        class Msg
        {
            public string ClientId { get; set; }
            public object Data { get; set; }
            public MQMsgType MsgType { get; set; }
            public string Topic { get; set; }
        }


    }

    public class QuotationMQPublisherConfig
    {
        public string Address { get; set; }
        public int RecordsPerPackage { get; set; }
    }
}
