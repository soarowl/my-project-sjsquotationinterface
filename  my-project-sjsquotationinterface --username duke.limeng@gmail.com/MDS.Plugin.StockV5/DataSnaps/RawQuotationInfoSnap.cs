using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
namespace MDS.Plugin.StockV5
{
    class RawQuotationInfoSnap
    {
        public static readonly ConcurrentDictionary<string, QuotV5.Binary.QuotSnap300111> QuotSnap300111 = new ConcurrentDictionary<string, QuotV5.Binary.QuotSnap300111>();

        public static readonly ConcurrentDictionary<string, QuotV5.Binary.Order300192> Order300192 = new ConcurrentDictionary<string, QuotV5.Binary.Order300192>(); 

    }
}
