using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceManager.Utils;
using ServiceStack.Redis;
namespace MDS.Plugin.SZQuotV5
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

                string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString());


                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;
                List<StockInfo> rtn = new List<StockInfo>();
                foreach (var rtnByte in rtnBytes)
                {
                    StockInfo stockInfo = JsonSerializer.BytesToObject<StockInfo>(rtnByte);
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

                string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());

                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;

                List<QuotationInfo> rtn = new List<QuotationInfo>();
                foreach (var rtnByte in rtnBytes)
                {
                    QuotationInfo quotInfo = JsonSerializer.BytesToObject<QuotationInfo>(rtnByte);
                    rtn.Add(quotInfo);
                }
                return rtn;

            }
        }

        public void UpdateBasicInfo(StockInfo stockInfo)
        {
            byte[] stockInfoBytes = JsonSerializer.ObjectToBytes<StockInfo>(stockInfo);
            string stkId = stockInfo.stkId;
            string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString());
            QuotationPublisher.Publisher.Update(key, stkId, stockInfoBytes);
        }

        public void UpdateQuotInfo(QuotationInfo quotInfo)
        {
            byte[] quotInfoBytes = JsonSerializer.ObjectToBytes<QuotationInfo>(quotInfo);
            string stkId = quotInfo.stkId;
            string key = string.Format("{0}:{1}", quotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());
            QuotationPublisher.Publisher.Update(key, stkId, quotInfoBytes);

        }

        public void ClearAllBasicInfo()
        {

        }

        public void ClearAllQuotInfo()
        {

        }

     
    }
}
