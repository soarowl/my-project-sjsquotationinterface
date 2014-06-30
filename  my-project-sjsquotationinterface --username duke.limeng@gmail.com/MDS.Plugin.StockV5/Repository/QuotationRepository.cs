using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceManager.Utils;
namespace MDS.Plugin.StockV5
{
    public class QuotationRepository
    {
        static QuotationSource quotSource = StockQuotationSource.Level1;
        public List<StockInfo> GetAllStockInfo()
        {

        }

        public List<QuotationInfo> GetAllQuotationInfo()
        {

        }

        public void UpdateBasicInfo(StockInfo stockInfo)
        {

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
