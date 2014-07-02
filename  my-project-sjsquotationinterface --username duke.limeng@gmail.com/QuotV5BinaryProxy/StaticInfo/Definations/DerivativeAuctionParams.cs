using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 衍生品集中竞价交易类业务参考信息
    /// </summary>
    [XmlParseInfo("AuctionParams", "Security")]
    public class DerivativeAuctionParams
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        /// <remarks>
        /// C8
        /// </remarks>
        public string SecurityID { get; set; }

        /// <summary>
        /// 买数量上限
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double BuyQtyUpperLimit { get; set; }

        /// <summary>
        /// 卖数量上限
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double SellQtyUpperLimit { get; set; }

        /// <summary>
        /// 买数量单位
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double BuyQtyUnit { get; set; }

        /// <summary>
        /// 卖数量单位
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double SellQtyUnit { get; set; }

        /// <summary>
        /// 价格档位
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double PriceTick { get; set; }

        /// <summary>
        /// 涨停价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double RisePrice { get; set; }


        /// <summary>
        /// 跌停价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double FallPrice { get; set; }

        /// <summary>
        /// 昨卖开每张保证金
        /// </summary>
        /// <remarks>
        /// N18(4)
        /// </remarks>
        public decimal LastSellMargin { get; set; }

        /// <summary>
        /// 今卖开每张保证金
        /// </summary>
        /// <remarks>
        /// N18(4)
        /// </remarks>
        public decimal SellMargin { get; set; }


        /// <summary>
        /// 做市商标志
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool MarketMakerFlag { get; set; }

    }
}
