using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
namespace MDS.Plugin.StockV5
{
     class StoredDataSnap
    {
         public static readonly ConcurrentDictionary<string, StockInfo> StockInfo = new ConcurrentDictionary<string, StockInfo>();

         public static readonly ConcurrentDictionary<string, QuotationInfo> QuotationInfo = new ConcurrentDictionary<string, QuotationInfo>();
    }
}
