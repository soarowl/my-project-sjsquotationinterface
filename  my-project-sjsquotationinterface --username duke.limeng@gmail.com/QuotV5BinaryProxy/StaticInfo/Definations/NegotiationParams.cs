using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 协议交易业务参考信息
    /// </summary>
    class NegotiationParams
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        /// <remarks>
        /// C8
        /// </remarks>
        public string SecurityID { get; set; }

        /// <summary>
        /// 买数量单位
        /// </summary>
        ///<remarks>
        ///N15(2)
        ///每笔买委托的委托数量必须是买数量单位的整数倍
        /// </remarks>
        public double BuyQtyUnit { get; set; }

       
        /// <summary>
        /// 卖数量单位
        /// </summary>
        ///<remarks>
        ///N15(2)
        ///每笔卖委托的委托数量必须是卖数量单位的整数倍
        /// </remarks>
        public double SellQtyUnit { get; set; }

        /// <summary>
        /// 数量门槛
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// 必须满足数量门槛或金额门槛
        /// </remarks>
        public double BuyQtyLowerLimit { get; set; }

        /// <summary>
        /// 金额门槛
        /// </summary>
        /// <remarks>
        /// N18(4)
        /// </remarks>
        public decimal BuyAmtLowerLimit { get; set; }

        /// <summary>
        /// 涨停价格
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double PriceUpLimit { get; set; }

        /// <summary>
        /// 跌停价格
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double PriceDownLimit { get; set; }

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
