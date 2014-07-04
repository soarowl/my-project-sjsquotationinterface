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
        public static readonly ConcurrentDictionary<string, QuotV5.Binary.QuotSnap300611> QuotSnap300611 = new ConcurrentDictionary<string, QuotV5.Binary.QuotSnap300611>();
        public static readonly ConcurrentDictionary<string, QuotV5.Binary.QuotSnap309011> QuotSnap309011 = new ConcurrentDictionary<string, QuotV5.Binary.QuotSnap309011>();
        public static readonly ConcurrentDictionary<string, QuotV5.Binary.RealtimeStatus> RealtimeStatus = new ConcurrentDictionary<string, QuotV5.Binary.RealtimeStatus>();
        public static readonly ConcurrentDictionary<string, QuotV5.Binary.Order300192> Order300192 = new ConcurrentDictionary<string, QuotV5.Binary.Order300192>();

        public static void ClearAll()
        {
            QuotSnap300111.Clear();
            QuotSnap300611.Clear();
            QuotSnap309011.Clear();
            RealtimeStatus.Clear();
            Order300192.Clear();
        }
    }
}
