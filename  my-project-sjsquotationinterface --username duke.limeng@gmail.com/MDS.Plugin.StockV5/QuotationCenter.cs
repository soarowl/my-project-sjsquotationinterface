using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    class QuotationCenter
    {
        private Log4cb.ILog4cbHelper logHelper;
        private QuotationManager quotManager;
        private QuotationSnapService quotSnapService;
        public QuotationCenter(Log4cb.ILog4cbHelper logHelper)
        {
            this.logHelper = logHelper;
            ConstructQuotManager();
        }


        internal void ConstructQuotManager()
        {
            var connConfig = PluginContext.Configuration.BinaryConnections.Connections.FirstOrDefault(c => c.Key == "Realtime");
            if (connConfig == null)
                throw new Exception();

            QuotV5.Binary.ConnectionConfig realtimeConnConfig = new QuotV5.Binary.ConnectionConfig()
            {
                IP = System.Net.IPAddress.Parse(connConfig.IP),
                TargetCompID = connConfig.TargetCompID,
                Port = connConfig.Port,
                HeartbeatIntervalS = connConfig.HeartbeatIntervalS,
                ConnectionTimeoutMS = connConfig.ConnectionTimeoutMS,
                ReconnectIntervalMS = connConfig.ReconnectIntervalMS,
                SenderCompID = connConfig.SenderCompID,
                Password = connConfig.Password
            };
            //QuotV5.Binary.ConnectionConfig realtimeConnConfig = new QuotV5.Binary.ConnectionConfig() 
            //{
            //    IP = System.Net.IPAddress.Parse("172.16.1.99"),
            //    TargetCompID = "N4005Q0001          ",
            //    Port = 8016,
            //    HeartbeatIntervalS = 3,
            //    ConnectionTimeoutMS = 10000,
            //    ReconnectIntervalMS = 10000,
            //    SenderCompID = "realtime            ",
            //    Password = "                "
            //};
            QuotV5.Binary.RealTimeQuotConnection realtimeQuotConn = new QuotV5.Binary.RealTimeQuotConnection(realtimeConnConfig, this.logHelper);

            QuotationRepository quotRepository = new QuotationRepository();


            QuotationMQPublisherConfig publisherConfig = new QuotationMQPublisherConfig()
            {
                Address = PluginContext.Configuration.ActiveMQ.Address,
                RecordsPerPackage = PluginContext.Configuration.ActiveMQ.RecordsPerPackage
            };
            QuotationMQPublisher quotPublisher = new QuotationMQPublisher(publisherConfig, this.logHelper);


            StaticInfoProviderConfig securityInfoProviderConfig = GetStaticInfoProviderConfig("Securities");
            SecurityInfoProvider securityInfoProvider = new SecurityInfoProvider(securityInfoProviderConfig, this.logHelper);



            StaticInfoProviderConfig indexInfoProviderConfig = GetStaticInfoProviderConfig("IndexInfo");
            IndexInfoProvider indexInfoProvider = new IndexInfoProvider(indexInfoProviderConfig, this.logHelper);

            StaticInfoProviderConfig cashAuctionParamsProviderConfig = GetStaticInfoProviderConfig("CashAuctionParams");
            CashAuctionParamsProvider cashAuctionParamsProvider = new CashAuctionParamsProvider(cashAuctionParamsProviderConfig, this.logHelper);

            StaticInfoProviderConfig derivativeAuctionParamsProviderConfig = GetStaticInfoProviderConfig("DerivativeAuctionParams");
            DerivativeAuctionParamsProvider derivativeAuctionParamsProvider = new DerivativeAuctionParamsProvider(derivativeAuctionParamsProviderConfig, this.logHelper);

            StaticInfoProviderConfig negotiationParamsProviderConfig = GetStaticInfoProviderConfig("NegotiationParams");
            NegotiationParamsProvider negotiationParamsProvider = new NegotiationParamsProvider(negotiationParamsProviderConfig, this.logHelper);

            StaticInfoProviderConfig securityCloseMDProviderConfig = GetStaticInfoProviderConfig("SecurityCloseMD");
            SecurityCloseMDProvider securityCloseMDProvider = new SecurityCloseMDProvider(securityCloseMDProviderConfig, this.logHelper);



            this.quotManager = new QuotationManager(
               realtimeQuotConn,
               quotRepository,
               quotPublisher,
              // null,
               securityInfoProvider,
               indexInfoProvider,
               cashAuctionParamsProvider,
               derivativeAuctionParamsProvider,
               negotiationParamsProvider,
               securityCloseMDProvider,
               this.logHelper
               );

            QuotationSnapServiceConfig sc = new QuotationSnapServiceConfig() { Address = PluginContext.Configuration.ActiveMQ.Address, RecordsPerPackage = PluginContext.Configuration.ActiveMQ.RecordsPerPackage };
            this.quotSnapService = new QuotationSnapService(sc, quotPublisher, this.logHelper);
        }


        private StaticInfoProviderConfig GetStaticInfoProviderConfig(string key)
        {
            var cfg = PluginContext.Configuration.StaticInfoFiles.Files.FirstOrDefault(f => f.Key == key);
            return new StaticInfoProviderConfig()
            {
                PathFormat = cfg.Path,
                ScanInterval = TimeSpan.Parse(cfg.ScanInterval)
            };
        }

        internal void Stop()
        {
            this.quotManager.Stop();
            this.quotSnapService.Stop();
        }

        internal void Start()
        {
            this.quotManager.Start();
           this.quotSnapService.Start();
        }
    }
}
