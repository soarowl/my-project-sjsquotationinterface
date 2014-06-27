using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 集中竞价交易类业务参考信息
    /// </summary>
   [XmlParseInfo("AuctionParams", "Security")]
    public class CashAuctionParams
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
        /// 竞价限价参数
        /// </summary>
        [XmlParseInfo("PriceLimitSetting", "Setting")]
        public List<PriceLimitSetting> PriceLimitSetting { get; set; }

        /// <summary>
        /// 做市商标志
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>
        public bool MarketMakerFlag { get; set; }
    }
    /// <summary>
    /// 竞价限价参数
    /// </summary>
    public class PriceLimitSetting
    {
        /// <summary>
        /// 参数类型
        /// </summary>
        /// <remarks>
        /// C1
        /// O=开盘集合竞价
        /// T=连续竞价
        /// C=收盘集合竞价
        /// </remarks>
        public string Type { get; set; }

        /// <summary>
        /// 是否有涨跌限制
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool HasPriceLimit { get; set; }

        /// <summary>
        /// 基准价类型
        /// </summary>
        /// <remarks>
        /// C1
        /// 1=昨收价
        /// </remarks>
        public string ReferPriceType { get; set; }

        /// <summary>
        /// 涨跌限制类型
        /// </summary>
        /// <remarks>
        /// C1
        /// 1=幅度
        /// 2=价格
        /// </remarks>
        public string LimitType { get; set; }

        /// <summary>
        /// 上涨幅度
        /// </summary>
        ///<remarks>
        ///N10(3)
        /// </remarks>
        public double LimitUpRate { get; set; }

        /// <summary>
        /// 下跌幅度
        /// </summary>
        ///<remarks>
        ///N10(3)
        /// </remarks>
        public double LimitDownRate { get; set; }

        /// <summary>
        /// 上涨限价
        /// </summary>
        ///<remarks>
        ///N10(4)
        /// </remarks>
        public double LimitUpAbsolute { get; set; }

        /// <summary>
        /// 下跌限价
        /// </summary>
        ///N10(4)
        public double LimitDownAbsolute { get; set; }

        /// <summary>
        /// 是否有有效竞价范围限制
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>
        public bool HasAuctionLimit { get; set; }

        /// <summary>
        /// 有效范围限制类型
        /// </summary>
        ///<remarks>
        ///C1
        /// </remarks>
        public string AuctionLimitType { get; set; }

        /// <summary>
        /// 有效范围涨跌幅度
        /// </summary>
        ///<remarks>
        ///N10(3)
        /// </remarks>
        public double AuctionUpDownRate { get; set; }

        /// <summary>
        /// 有效范围涨跌价格
        /// </summary>
        /// <remarks>
        /// N10(4)
        /// </remarks>
        public double AuctionUpDownAbsolute { get; set; }
    }
}
