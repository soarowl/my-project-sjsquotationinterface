using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MDS.Plugin.StockV5;
using System.Collections.Concurrent;
namespace MDS.Plugin.StockV5
{
    public class RawStaticInfoSnap
    {
        public static readonly ConcurrentDictionary<string, QuotV5.StaticInfo.SecurityInfoBase> SecurityInfo = new ConcurrentDictionary<string, QuotV5.StaticInfo.SecurityInfoBase>();

        public static readonly ConcurrentDictionary<string, QuotV5.StaticInfo.IndexInfo> IndexInfo = new ConcurrentDictionary<string, QuotV5.StaticInfo.IndexInfo>();

        public static readonly ConcurrentDictionary<string, QuotV5.StaticInfo.CashAuctionParams> CashAuctionParams = new ConcurrentDictionary<string, QuotV5.StaticInfo.CashAuctionParams>();

        public static readonly ConcurrentDictionary<string, QuotV5.StaticInfo.DerivativeAuctionParams> DerivativeAuctionParams = new ConcurrentDictionary<string, QuotV5.StaticInfo.DerivativeAuctionParams>();

        public static readonly ConcurrentDictionary<string, QuotV5.StaticInfo.NegotiationParams> NegotiationParams = new ConcurrentDictionary<string, QuotV5.StaticInfo.NegotiationParams>();
    }
}
