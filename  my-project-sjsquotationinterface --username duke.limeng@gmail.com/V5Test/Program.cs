using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Log4cb;
using System.IO;
namespace V5Test
{
    class Program
    {
        static Log4cb.ILog4cbHelper logHelper = new Log4cb.Log4cbHelper("V5");
        static void Main(string[] args)
        {
            //testRealtimeQuotConn();

            //testStaticInfo_Security();
            //testStaticInfo_CashAuctionParams();
            testStaticInfo_DerivativeAuctionParams();
            Console.Read();
        }

        private static void testStaticInfo_Security()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/securities_20140618.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.SecurityInfoParser parser = new QuotV5.StaticInfo.SecurityInfoParser();
            var securities = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("ParseSecurity", 10, () => { var ss = parser.Parse(fileContent); });
            
        }
        private static void testStaticInfo_CashAuctionParams()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/cashauctionparams_20140618.xml");
            string fileContent = File.ReadAllText(securityFile, Encoding.UTF8);
            CodeTimer.CodeTimer.Initialize();
            QuotV5.StaticInfo.CashAuctionParamsParser parser = new QuotV5.StaticInfo.CashAuctionParamsParser();
            var paras = parser.Parse(fileContent);
            CodeTimer.CodeTimer.Time("ParseCashAuctionParams", 10, () => { var ss = parser.Parse(fileContent); });

        }

        private static void testStaticInfo_DerivativeAuctionParams()
        {
            string securityFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Samples/derivativeauctionparams_20140618.xml");
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
                IP = System.Net.IPAddress.Parse("127.0.0.1"),
                TargetCompID = "mdgw11              ",
                Port = 8016,
                HeartbeatIntervalS = 3,
                ConnectionTimeoutMS = 6000,
                ReconnectIntervalMS = 3000,
                SenderCompID = "Realtime1           ",
                Password = "                "
            };
            QuotV5.Binary.RealTimeQuotConnection conn = new QuotV5.Binary.RealTimeQuotConnection(cfg, logHelper);

            conn.OnMarketDataReceived += new Action<QuotV5.Binary.IMarketData>(conn_OnMarketDataReceived);
            conn.Start();
        }

        static void conn_OnMarketDataReceived(QuotV5.Binary.IMarketData obj)
        {
            if (obj is QuotV5.Binary.Order300192)
            {
                logHelper.LogInfoMsg("收到Order300192");
            }
            else if (obj is QuotV5.Binary.QuotSnap300111)
            {
                logHelper.LogInfoMsg("收到QuotSnap300111");
            }
        }


    }

}
