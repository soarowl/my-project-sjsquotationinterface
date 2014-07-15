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
        public QuotationSnapService(QuotationSnapServiceConfig config,QuotationMQPublisher quotPublisher, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.quotPublisher = quotPublisher;
            this.logHelper = logHelper;
            MQConnConfig mqConfig = new MQConnConfig() { Address =config.Address };
            this.stockInfoMQConsumer = new MQConsumer(mqConfig, logHelper);
            this.quotInfoMQConsumer =new MQConsumer (mqConfig,logHelper );
        }

        public void Start()
        {
            this.stockInfoMQConsumer.StartMQ();
            this.stockInfoMQConsumer.SubscribeMsg(MQMsgType.QUEUE, "SZ5_REQ_StkInfo");

            this.stockInfoMQConsumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(stockInfoMQConsumer_OnMessageReceived);

            this.quotInfoMQConsumer.StartMQ ();
            this.quotInfoMQConsumer.SubscribeMsg(MQMsgType.QUEUE, "SZ5_REQ_Quotation");
            this.quotInfoMQConsumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(quotInfoMQConsumer_OnMessageReceived);
        }

        void quotInfoMQConsumer_OnMessageReceived(string msgData, Dictionary<string, object> properties)
        {
            if (properties != null && properties.ContainsKey("clientId"))
            {
                this.logHelper.LogInfoMsg("收到SZ5_REQ_Quotation请求，clientId={0}", properties["clientId"]);
                PublishStockQuotSnap(properties["clientId"] as string);
            }
        }

        void stockInfoMQConsumer_OnMessageReceived(string msgData, Dictionary<string, object> properties)
        {
            if (properties!=null && properties.ContainsKey("clientId"))
            {
                this.logHelper.LogInfoMsg("收到SZ5_REQ_StkInfo请求，clientId={0}", properties["clientId"]);
                PublishStockSnap(properties["clientId"] as string);
            }
        }


        private void PublishStockSnap(string clientId)
        {
            try
            {
                var stockInfos = ProcessedDataSnap.StockInfo.Values.ToList();
                this.quotPublisher.Enqueue(stockInfos,false, clientId);
            }
            catch (Exception ex)
            {
                this.logHelper.LogErrMsg(ex,"发布StockInfo快照 clientId={0}",clientId);
            }
        }

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
