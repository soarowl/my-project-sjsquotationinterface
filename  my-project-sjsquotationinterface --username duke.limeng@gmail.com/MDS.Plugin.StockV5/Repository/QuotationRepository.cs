using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceManager.Utils;
using ServiceStack.Redis;
namespace MDS.Plugin.StockV5
{
    public class QuotationRepository
    {
        static QuotationSource quotSource = StockQuotationSource.Level1;

        public List<StockInfo> GetAllStockInfo()
        {
            using (var conn = QuotationPublisher.Publisher.GetNeedDisposedConnection())
            {
                if (conn == null)
                    return null;

                string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());


                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;

                List<StockInfo> rtn = new List<StockInfo>();
                foreach (var rtnByte in rtnBytes)
                {
                    StockInfo stockInfo = rtnByte.BytesToStruct<StockInfo>();
                    rtn.Add(stockInfo);
                }
                return rtn;

            }
        }

        public List<QuotationInfo> GetAllQuotationInfo()
        {
            using (var conn = QuotationPublisher.Publisher.GetNeedDisposedConnection())
            {
                if (conn == null)
                    return null;

                string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString());


                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;

                List<QuotationInfo> rtn = new List<QuotationInfo>();
                foreach (var rtnByte in rtnBytes)
                {
                    QuotationInfo quotInfo = rtnByte.BytesToStruct<QuotationInfo>();
                    rtn.Add(quotInfo);
                }
                return rtn;

            }
        }

        public void UpdateBasicInfo(StockInfo stockInfo)
        {
            byte[] stockInfoBytes = stockInfo.StructToBytes<StockInfo>();

            string stkId = stockInfo.stkId;
            string key = string.Format("{0}:{1}:{2}", quotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString(), stkId.PadLeft(10, '#'));
            QuotationPublisher.Publisher.Publish(key, stockInfoBytes);
        }

        public void UpdateQuotInfo(QuotationInfo quotInfo)
        {
            byte[] quotInfoBytes = quotInfo.StructToBytes<QuotationInfo>();

            string stkId = quotInfo.stkId;
            string key = string.Format("{0}:{1}:{2}", quotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString(), stkId.PadLeft(10, '#'));
            QuotationPublisher.Publisher.Publish(key, quotInfoBytes);

        }
    }
}
