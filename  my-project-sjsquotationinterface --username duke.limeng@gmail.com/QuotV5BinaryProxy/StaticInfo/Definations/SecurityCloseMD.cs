using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 证券收盘行情
    /// </summary>
    [XmlParseInfo("Securities", "Security")]
    public class SecurityCloseMD
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        public string SecurityID { get; set; }

        /// <summary>
        /// 证券简称
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// 收盘价
        /// </summary>
        public decimal ClosePx { get; set; }

        /// <summary>
        /// 总成交量
        /// </summary>
        public double TotalVolumeTrade { get; set; }

        /// <summary>
        /// 总成交金额
        /// </summary>
        public decimal TotalValueTrade { get; set; }
    }
}
