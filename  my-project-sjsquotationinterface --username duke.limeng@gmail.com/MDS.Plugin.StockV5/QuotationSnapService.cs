using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace MDS.Plugin.SZQuotV5
{
    public class QuotationSnapService
    {
        private Log4cb.ILog4cbHelper logHelper;
        private QuotationSnapServiceConfig config;
        private ManualResetEvent stopEvent = new ManualResetEvent(false);
        private MQConsumer stockInfoMQConsumer;
        private MQConsumer quotInfoMQConsumer;
        private QuotationMQPublisher quotPublisher;
        public QuotationSnapService(QuotationSnapServiceConfig config, QuotationMQPublisher quotPublisher, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.quotPublisher = quotPublisher;
            this.logHelper = logHelper;
            MQConnConfig mqConfig = new MQConnConfig() { Address = config.Address };
            this.stockInfoMQConsumer = new MQConsumer(mqConfig, logHelper);
            this.quotInfoMQConsumer = new MQConsumer(mqConfig, logHelper);
        }

        public void Start()
        {
            this.stockInfoMQConsumer.StartMQ();
            this.stockInfoMQConsumer.SubscribeMsg(MQMsgType.QUEUE, "SZ5_REQ_StkInfo");

            this.stockInfoMQConsumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(stockInfoMQConsumer_OnMessageReceived);

            this.quotInfoMQConsumer.StartMQ();
            this.quotInfoMQConsumer.SubscribeMsg(MQMsgType.QUEUE, "SZ5_REQ_Quotation");
            this.quotInfoMQConsumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(quotInfoMQConsumer_OnMessageReceived);
        }

        void quotInfoMQConsumer_OnMessageReceived(string msgData, Dictionary<string, object> properties)
        {
            if (properties != null && properties.ContainsKey("clientId") && properties.ContainsKey("refreshType"))
            {
                this.logHelper.LogInfoMsg("收到SZ5_REQ_Quotation请求，clientId={0}，refreshType={1}", properties["clientId"], properties["refreshType"]);
                if ((properties["refreshType"] as string) == "FUT")
                {
                    PublishFutureQuotSnap(properties["clientId"] as string);
                }
                else if ((properties["refreshType"] as string) == "STK")
                {
                    PublishStockQuotSnap(properties["clientId"] as string);
                }
            }
        }
        /// <summary>
        /// 从MQ收到消息
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="properties"></param>
        void stockInfoMQConsumer_OnMessageReceived(string msgData, Dictionary<string, object> properties)
        {
            if (properties != null && properties.ContainsKey("clientId") && properties.ContainsKey("refreshType"))
            {
                this.logHelper.LogInfoMsg("收到SZ5_REQ_StkInfo请求，clientId={0}，refreshType={1}", properties["clientId"], properties["refreshType"]);

                if ((properties["refreshType"] as string) == "FUT")
                {
                    PublishFutureQuotSnap(properties["clientId"] as string);
                }
                else if ((properties["refreshType"] as string) == "STK")
                {
                    PublishStockSnap(properties["clientId"] as string);
                }
            }
        }

        /// <summary>
        /// 发布现货基本信息快照
        /// </summary>
        /// <param name="clientId"></param>
        private void PublishStockSnap(string clientId)
        {
            try
            {
                var stockInfos = ProcessedDataSnap.StockInfo.Values.ToList();
                this.quotPublisher.Enqueue(stockInfos, false, clientId);
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "发布StockInfo快照 clientId={0}", clientId);
            }
        }

        /// <summary>
        /// 发布现货行情快照
        /// </summary>
        /// <param name="clientId"></param>
        private void PublishStockQuotSnap(string clientId)
        {
            try
            {
                var quotInfos = ProcessedDataSnap.StockQuotation.Values.ToList();
                this.quotPublisher.Enqueue(quotInfos, false, clientId);
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "发布QuotationInfo快照 clientId={0}", clientId);
            }
        }

        /// <summary>
        /// 发布期货行情快照
        /// </summary>
        /// <param name="clientId"></param>
        private void PublishFutureQuotSnap(string clientId)
        {
            try
            {
                var quotInfos = ProcessedDataSnap.StockQuotation.Values.ToList();
                this.quotPublisher.Enqueue(quotInfos, false, clientId);
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex, "发布QuotationInfo快照 clientId={0}", clientId);
            }
        }


        public void Stop()
        {
            this.stockInfoMQConsumer.StopMQ();
            this.quotInfoMQConsumer.StopMQ();
        }

    }
    public class QuotationSnapServiceConfig
    {
        public string Address { get; set; }
        public int RecordsPerPackage { get; set; }

    }
}
