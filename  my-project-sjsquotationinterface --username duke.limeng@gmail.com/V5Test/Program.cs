using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4cb;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
namespace V5Test
{
    class Program
    {
        static Log4cb.ILog4cbHelper logHelper = new Log4cb.Log4cbHelper("V5");
        static void Main(string[] args)
        {
            //testMQConsumer2();
           // Thread.Sleep(3000);
            // testMQProducer();
            //testObjToString();
             testRealtimeQuotConn();
            // testStaticInfo_Index();
            // testStaticInfo_Security();
            //  testStaticInfo_CashAuctionParams();
            //testStaticInfo_DerivativeAuctionParams();
            //testFastReflection();
            //testObjectCreator();

            // testStaticInfo_NegotiationParamsParser();


            //QuotV5.StaticInfo.CashAuctionParams para = new QuotV5.StaticInfo.CashAuctionParams();
            //QuotV5.StaticInfo.PriceLimitSetting setting = new QuotV5.StaticInfo.PriceLimitSetting();
            //List<object> list = new List<object>();
            //list.Add(setting);
            //var prop = typeof(QuotV5.StaticInfo.CashAuctionParams).GetProperty("PriceLimitSetting");
            //QuotV5.FastReflection.SetListPropertyValue(para, prop, list);
            //list.Cast<QuotV5.StaticInfo.PriceLimitSetting>().ToList();
            // testExp();


            //testAddList();


            Console.Read();
        }


        private static void testMQProducer()
        {
            MDS.Plugin.SZQuotV5.MQConnConfig cfg = new MDS.Plugin.SZQuotV5.MQConnConfig() { Address = "192.168.1.195:61616" };
            MDS.Plugin.SZQuotV5.MQProducer mp = new MDS.Plugin.SZQuotV5.MQProducer(cfg, logHelper);
            mp.StartMQ();
            var properties = new Dictionary<string, object>();
            properties["clientId"] = "test1";
            string msgId;
            bool succeed = mp.SendMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_REQ_StkInfo", string.Empty, properties, 2000, out msgId);
            Console.WriteLine(string.Format("SZ5_REQ_StkInfo succeed={1},msgId={0}", msgId, succeed));

            succeed = mp.SendMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_REQ_Quotation", string.Empty, properties, 2000, out msgId);
            Console.WriteLine(string.Format("SZ5_REQ_Quotation succeed={1},msgId={0}", msgId, succeed));
            //MDS.Plugin.SZQuotV5.MQConnConfig cfg = new MDS.Plugin.SZQuotV5.MQConnConfig() { Address = "192.168.1.164:61616" };
            //MDS.Plugin.SZQuotV5.MQProducer producer = new MDS.Plugin.SZQuotV5.MQProducer(cfg, logHelper);
            //producer.StartMQ();
            //MDS.Plugin.SZQuotV5.StockInfo stockInfo=new MDS.Plugin.SZQuotV5.StockInfo ()
            //{
            //stkId ="000001"
            //};
            //byte[] data=ObjectToBytes(stockInfo);
            //string msgId;
            //bool succeed = producer.SendMsg(MDS.Plugin.SZQuotV5.MQMsgType.TOPIC, "1503-ActiveMQURI-3", data, null, 5000, out msgId);

        }

        private static void testMQConsumer()
        {
            MDS.Plugin.SZQuotV5.MQConnConfig cfg = new MDS.Plugin.SZQuotV5.MQConnConfig() { Address = "192.168.1.195:61616" };
            MDS.Plugin.SZQuotV5.MQConsumer consumer = new MDS.Plugin.SZQuotV5.MQConsumer(cfg, logHelper);
            consumer.StartMQ();
            consumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(consumer_OnMessageReceived);
            consumer.SubscribeMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_StkInfo_Image", null);

            MDS.Plugin.SZQuotV5.MQConsumer consumer2 = new MDS.Plugin.SZQuotV5.MQConsumer(cfg, logHelper);
            consumer2.StartMQ();
            consumer2.OnMessageReceived += new Action<string, Dictionary<string, object>>(consumer2_OnMessageReceived);
            consumer2.SubscribeMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_Quotation_Image", null);
        }
        private static void testMQConsumer2()
        {
            MDS.Plugin.SZQuotV5.MQConnConfig cfg = new MDS.Plugin.SZQuotV5.MQConnConfig() { Address = "192.168.1.195:61616" };
            MDS.Plugin.SZQuotV5.MQConsumer consumer = new MDS.Plugin.SZQuotV5.MQConsumer(cfg, logHelper);
            consumer.StartMQ();
            consumer.OnMessageReceived += new Action<string, Dictionary<string, object>>(consumer_OnMessageReceived);
            consumer.SubscribeMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_REQ_StkInfo", null);

            MDS.Plugin.SZQuotV5.MQConsumer consumer2 = new MDS.Plugin.SZQuotV5.MQConsumer(cfg, logHelper);
            consumer2.StartMQ();
            consumer2.OnMessageReceived += new Action<string, Dictionary<string, object>>(consumer2_OnMessageReceived);
            consumer2.SubscribeMsg(MDS.Plugin.SZQuotV5.MQMsgType.QUEUE, "SZ5_REQ_Quotation", null);
        }

        static List<MDS.Plugin.SZQuotV5.StockQuotation> qoutList;
        static List<MDS.Plugin.SZQuotV5.StockInfo> stockList;
        static void consumer2_OnMessageReceived(string arg1, Dictionary<string, object> arg2)
        {
            qoutList = MDS.Plugin.SZQuotV5.JsonSerializer.StringToObject<List<MDS.Plugin.SZQuotV5.StockQuotation>>(arg1);
            Console.WriteLine("SZ5_Quotation_Image Count={0},clientID={1}", qoutList.Count, arg2["clientId"]);
        }

        static void consumer_OnMessageReceived(string arg1, Dictionary<string, object> arg2)
        {
            stockList = MDS.Plugin.SZQuotV5.JsonSerializer.StringToObject<List<MDS.Plugin.SZQuotV5.StockInfo>>(arg1);
            Console.WriteLine("SZ5_StkInfo_Image Count={0},clientID={1}", stockList.Count, arg2["clientId"]);
        }

        private static byte[] ObjectToBytes<T>(T obj)
        {
            var str = ServiceStack.Text.JsonSerializer.SerializeToString<T>(obj);
            return Encoding.UTF8.GetBytes(str);
        }
        private static void testObjToString()
        {
            QuotV5.Binary.QuotSnap300111 quotSnap = new QuotV5.Binary.QuotSnap300111()
            {
                CommonInfo = new QuotV5.Binary.QuotSnapCommonInfo() { ChannelNo = 1, MDStreamID = "3" },

                ExtInfo = new QuotV5.Binary.QuotSnapExtInfo300111()
                {
                    NoMDEntries = 5,
                    MDEntries = new QuotV5.Binary.QuotSnapExtInfo300111.MDEntry[]{
                new  QuotV5.Binary.QuotSnapExtInfo300111.MDEntry(){ Entry =new QuotV5.Binary.QuotSnapExtInfo300111.MDEntryWithoutOrders (){ MDEntryPx=3, MDEntrySize =5, NoOrders =1},Orders =new  QuotV5.Binary.QuotSnapExtInfo300111.Order[]{new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){ OrderQty =1212},new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){OrderQty =1212} } },
                 new  QuotV5.Binary.QuotSnapExtInfo300111.MDEntry(){ Entry =new QuotV5.Binary.QuotSnapExtInfo300111.MDEntryWithoutOrders (){ MDEntryPx=3, MDEntrySize =5, NoOrders =1},Orders =new  QuotV5.Binary.QuotSnapExtInfo300111.Order[]{new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){ OrderQty =1212},new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){OrderQty =1213} } },
             new  QuotV5.Binary.QuotSnapExtInfo300111.MDEntry(){ Entry =new QuotV5.Binary.QuotSnapExtInfo300111.MDEntryWithoutOrders (){ MDEntryPx=3, MDEntrySize =5, NoOrders =1},Orders =new  QuotV5.Binary.QuotSnapExtInfo300111.Order[]{new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){ OrderQty =1212},new  QuotV5.Binary.QuotSnapExtInfo300111.Order(){OrderQty =1214} } }
                }
                }

            };
            string str = QuotV5.ObjectLogHelper<QuotV5.Binary.QuotSnap300111>.ObjectToString(quotSnap);
        }

        private static void testAddList()
        {
            List<QuotV5.StaticInfo.IndexInfo> list = new List<QuotV5.StaticInfo.IndexInfo>();
            addList1(list);
            addList2(list);
            CodeTimer.CodeTimer.Initialize();
            CodeTimer.CodeTimer.Time("addList1", 100000, () => { addList1(list); });
            CodeTimer.CodeTimer.Time("addList2", 100000, () => { addList2(list); });
        }

        private static void addList1(object list)
        {
            object item = new QuotV5.StaticInfo.IndexInfo();
            QuotV5.FastReflection.AddObjectToList(list, item);
        }
        private static void addList2(List<QuotV5.StaticInfo.IndexInfo> list)
        {
            var item = new QuotV5.StaticInfo.IndexInfo();
            list.Add(item);
        }


        private static void testObjectCreator()
        {
            testObjectCreator1();
            testObjectCreator2();
            CodeTimer.CodeTimer.Initialize();
            CodeTimer.CodeTimer.Time("testObjectCreator1", 100000, testObjectCreator1);
            CodeTimer.CodeTimer.Time("testObjectCreator2", 100000, testObjectCreator2);
        }

        private static void testObjectCreator1()
        {
            var indexInfo = QuotV5.ObjectCreator.Create(typeof(QuotV5.StaticInfo.IndexInfo), Type.EmptyTypes, null);

        }
        private static void testObjectCreator2()
        {
            var indexInfo = QuotV5.ObjectCreator<QuotV5.StaticInfo.IndexInfo>.Instance.Create(Type.EmptyTypes, null);

        }

        private static void testFastReflection()
        {
            testFastReflection1();
            testFastReflection2();
            CodeTimer.CodeTimer.Initialize();
            CodeTimer.CodeTimer.Time("testFastReflection1", 100000, testFastReflection1);
            CodeTimer.CodeTimer.Time("testFastReflection2", 100000, testFastReflection2);
        }

        private static void testFastReflection1()
        {
            QuotV5.StaticInfo.IndexInfo ii = new QuotV5.StaticInfo.IndexInfo();
            var properties = typeof(QuotV5.StaticInfo.IndexInfo).GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(string))
                {
                    QuotV5.FastReflection.SetPropertyValue<string>(ii, p, "4");
                }
                else if (p.PropertyType == typeof(decimal))
                {
                    QuotV5.FastReflection.SetPropertyValue<decimal>(ii, p, 123.34m);
                }
            }
        }

        private static void testFastReflection2()
        {
            QuotV5.StaticInfo.IndexInfo ii = new QuotV5.StaticInfo.IndexInfo();
            var properties = typeof(QuotV5.StaticInfo.IndexInfo).GetProperties();
            foreach (var p in properties)
            {
                if (p.PropertyType == typeof(string))
                {
                    QuotV5.FastReflection<QuotV5.StaticInfo.IndexInfo>.SetPropertyValue<string>(ii, p, "4");
                }
                else if (p.PropertyType == typeof(decimal))
                {
                    QuotV5.FastReflection<QuotV5.StaticInfo.IndexInfo>.SetPropertyValue<decimal>(ii, p, 123.34m);
                }
            }
        }

        private static void testStaticInfo_Security()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/securities_20140703.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.SecurityInfoParser parser = new QuotV5.StaticInfo.SecurityInfoParser();
            var securities = parser.Parse(fileContent);

            var sec150167 = securities.FirstOrDefault(s => s.CommonInfo.SecurityID == "150167");
            CodeTimer.CodeTimer.Time("ParseSecurity", 10, () => { var ss = parser.Parse(fileContent); });

        }
        private static void testStaticInfo_Index()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/indexinfo_20140121.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.IndexInfoParser parser = new QuotV5.StaticInfo.IndexInfoParser();
            var securities = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("ParseIndex", 10, () => { var ss = parser.Parse(fileContent); });

        }
        private static void testStaticInfo_NegotiationParamsParser()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/negotiationparams_20140121.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.NegotiationParamsParser parser = new QuotV5.StaticInfo.NegotiationParamsParser();
            var securities = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("NegotiationParamsParser", 10, () => { var ss = parser.Parse(fileContent); });

        }

        private static void testStaticInfo_CashAuctionParams()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/cashauctionparams_20140121.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.CashAuctionParamsParser parser = new QuotV5.StaticInfo.CashAuctionParamsParser();
            var paras = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("ParseCashAuctionParams", 10, () => { var ss = parser.Parse(fileContent); });

        }

        private static void testStaticInfo_DerivativeAuctionParams()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/derivativeauctionparams_20140121.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.DerivativeAuctionParamsParser parser = new QuotV5.StaticInfo.DerivativeAuctionParamsParser();
            var paras = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("ParseDerivativeAuctionParams", 10, () => { var ss = parser.Parse(fileContent); });

        }

        private static void testRealtimeQuotConn()
        {
            QuotV5.Binary.ConnectionConfig cfg = new QuotV5.Binary.ConnectionConfig()
            {
                IP = System.Net.IPAddress.Parse("172.16.1.99"),
                TargetCompID = "N4005Q0001          ",
                Port = 8016,
                HeartbeatIntervalS = 3,
                ConnectionTimeoutMS = 10000,
                ReconnectIntervalMS = 5000,
                SenderCompID = "realtime            ",
                Password = "                "
            };
            QuotV5.Binary.RealTimeQuotConnection conn = new QuotV5.Binary.RealTimeQuotConnection(cfg, logHelper);

            conn.OnMarketDataReceived += new Action<QuotV5.Binary.MarketDataEx>(conn_OnMarketDataReceived);
            conn.Start();
        }

        static void conn_OnMarketDataReceived(QuotV5.Binary.MarketDataEx obj)
        {
            //if (obj is QuotV5.Binary.Order300192)
            //{
            //    logHelper.LogInfoMsg("收到Order300192");
            //}
            //else if (obj is QuotV5.Binary.QuotSnap300111)
            //{
            //    logHelper.LogInfoMsg("收到QuotSnap300111");
            //}

        }


    }

}
