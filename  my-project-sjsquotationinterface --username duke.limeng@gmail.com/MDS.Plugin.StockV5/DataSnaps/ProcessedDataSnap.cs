using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
namespace MDS.Plugin.SZQuotV5
{
     class ProcessedDataSnap
    {
         public static readonly ConcurrentDictionary<string, StockInfo> StockInfo = new ConcurrentDictionary<string, StockInfo>();

         public static readonly ConcurrentDictionary<string, StockQuotation> StockQuotation = new ConcurrentDictionary<string, StockQuotation>();

         public static readonly ConcurrentDictionary<string, FutureQuotation> FutureQuotation = new ConcurrentDictionary<string, FutureQuotation>();

     }
}
