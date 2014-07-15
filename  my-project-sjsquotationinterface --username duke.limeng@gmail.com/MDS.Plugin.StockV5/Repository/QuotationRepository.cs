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
        static QuotationSource stockQuotSource = StockQuotationSource.Level1;
        static QuotationSource futureQuotSource = FutureQuotationSource.FFuture;

        public List<StockInfo> GetAllStockInfo()
        {
            using (var conn = QuotationPublisher.Publisher.GetNeedDisposedConnection())
            {
                if (conn == null)
                    return null;

                string key = string.Format("{0}:{1}", stockQuotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString());


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

        public List<StockQuotation> GetAllStockQuotation()
        {
            using (var conn = QuotationPublisher.Publisher.GetNeedDisposedConnection())
            {
                if (conn == null)
                    return null;

                string key = string.Format("{0}:{1}", stockQuotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());

                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;

                List<StockQuotation> rtn = new List<StockQuotation>();
                foreach (var rtnByte in rtnBytes)
                {
                    StockQuotation quotInfo = JsonSerializer.BytesToObject<StockQuotation>(rtnByte);
                    rtn.Add(quotInfo);
                }
                return rtn;

            }
        }

        public List<FutureQuotation> GetAllFutureQuotation()
        {
            using (var conn = QuotationPublisher.Publisher.GetNeedDisposedConnection())
            {
                if (conn == null)
                    return null;

                string key = string.Format("{0}:{1}", futureQuotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());

                var rtnBytes = (conn as RedisClient).HVals(key);
                if (rtnBytes == null)
                    return null;

                List<FutureQuotation> rtn = new List<FutureQuotation>();
                foreach (var rtnByte in rtnBytes)
                {
                    FutureQuotation quotInfo = JsonSerializer.BytesToObject<FutureQuotation>(rtnByte);
                    rtn.Add(quotInfo);
                }
                return rtn;

            }
        }


        public void UpdateBasicInfo(StockInfo stockInfo)
        {
            byte[] stockInfoBytes = JsonSerializer.ObjectToBytes<StockInfo>(stockInfo);
            string stkId = stockInfo.stkId;
            string key = string.Format("{0}:{1}", stockQuotSource.ToFormatString(), QuotationDataType.StockInfo.ToFormatString());
            QuotationPublisher.Publisher.Update(key, stkId, stockInfoBytes);
        }

        public void UpdateStockQuotation(StockQuotation quotInfo)
        {
            byte[] quotInfoBytes = JsonSerializer.ObjectToBytes<StockQuotation>(quotInfo);
            string stkId = quotInfo.stkId;
            string key = string.Format("{0}:{1}", stockQuotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());
            QuotationPublisher.Publisher.Update(key, stkId, quotInfoBytes);

        }

        public void UpdateFutureQuotation(FutureQuotation quotInfo)
        {
            byte[] quotInfoBytes = JsonSerializer.ObjectToBytes<FutureQuotation>(quotInfo);
            string stkId = quotInfo.stkId;
            string key = string.Format("{0}:{1}", futureQuotSource.ToFormatString(), QuotationDataType.Quotation.ToFormatString());
            QuotationPublisher.Publisher.Update(key, stkId, quotInfoBytes);
        }
      
     
    }
}
