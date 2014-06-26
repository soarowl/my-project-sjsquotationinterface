using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    /// <summary>
    /// 表示所有证券都包含的字段
    /// </summary>
    public class CommonSecurityInfo
    {
        /// <summary>
        /// 证券代码
        /// </summary>
        /// <remarks>
        /// C8
        /// </remarks>
        public string SecurityID { get; set; }

        /// <summary>
        /// 证券简称
        /// </summary>
        /// <remarks>
        /// C40
        /// </remarks>
        public string Symbol { get; set; }

        /// <summary>
        /// 英文简称
        /// </summary>
        /// <remarks>
        /// C40
        /// </remarks>       
        public string EnglishName { get; set; }

        /// <summary>
        /// ISIN代码
        /// </summary>
        /// <remarks>
        /// C12
        /// </remarks>        
        public string ISIN { get; set; }

        /// <summary>
        /// 证券代码源
        /// </summary>
        /// <remarks>
        /// C4
        /// </remarks>
        public string SecurityIDSource { get; set; }

        /// <summary>
        ///基础证券代码 
        /// </summary>
        /// <remarks>
        /// C8
        /// </remarks>
        public string UnderlyingSecurityID { get; set; }

        /// <summary>
        /// 上市日期
        /// </summary>
        /// <remarks>
        /// N8
        /// </remarks>
        public Int32 ListDate { get; set; }

        /// <summary>
        /// 证券类别
        /// </summary>
        /// <remarks>
        /// N4
        /// </remarks>
        public SecurityType SecurityType { get; set; }

        /// <summary>
        /// 货币种类
        /// </summary>
        /// <remarks>
        /// C4
        /// </remarks>
        public string Currency { get; set; }

        /// <summary>
        /// 数量单位
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double QtyUnit { get; set; }

        /// <summary>
        /// 是否支持当日回转交易
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=支持
        /// N=不支持
        /// </remarks>
        public bool DayTrade { get; set; }

        /// <summary>
        /// 昨日收盘价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double PrevClosePx { get; set; }

        /// <summary>
        /// 证券状态
        /// </summary>
        /// <remarks>
        /// 一只证券可能具有0个或多个状态
        /// </remarks>
        public List<SecurityStatus> Status { get; set; }

        /// <summary>
        /// 总发行量
        /// </summary>
        /// <remarks>
        /// N18(2)
        /// </remarks>
        public decimal OutstandingShare { get; set; }

        /// <summary>
        /// 流通股数
        /// </summary>
        /// <remarks>
        /// N18(2)
        /// </remarks>
        public decimal PublicFloatShareQuantity { get; set; }

        /// <summary>
        /// 面值
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double ParValue { get; set; }

        /// <summary>
        /// 是否可作为融资融券可充抵保证金证券
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>
        public bool GageFlag { get; set; }

        /// <summary>
        /// 可充抵保证金折算率
        /// </summary>
        /// <remarks>
        /// N5(2)
        /// </remarks>
        public double GageRatio { get; set; }

        /// <summary>
        /// 是否为融资标的
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>        
        public bool CrdBuyUnderlying { get; set; }

        /// <summary>
        /// 是否为融券标的
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>
        public bool CrdSellUnderlying { get; set; }

        /// <summary>
        /// 是否可质押入库
        /// </summary>
        ///<remarks>
        ///C1
        ///Y=是
        ///N=否
        /// </remarks>
        public bool PledgeFlag { get; set; }

        /// <summary>
        /// 对回购标准券折算率
        /// </summary>
        ///<remarks>
        ///N6(5)
        /// </remarks>
        public double ContractMultiplier { get; set; }

        /// <summary>
        /// 对应回购标准券
        /// </summary>
        ///<remarks>
        ///C8
        /// </remarks>
        public string RegularShare { get; set; }

    }
}
