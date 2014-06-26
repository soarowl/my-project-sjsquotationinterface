using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace QuotV5.StaticInfo
{
    public abstract class SecurityParamsBase
    {

    }

    /// <summary>
    /// 股票特有字段
    /// </summary>
    public class StockParams : SecurityParamsBase
    {

        /// <summary>
        /// 行业种类
        /// </summary>
        /// <remarks>
        /// C4
        /// </remarks>
        public string IndustryClassification { get; set; }

        /// <summary>
        /// 上年每股利润
        /// </summary>
        /// <remarks>
        /// N10(4)
        /// </remarks>
        public double PreviousYearProfitPerShare { get; set; }

        /// <summary>
        /// 本年每股利润
        /// </summary>
        /// <remarks>
        /// N10(4)
        /// </remarks>
        public double CurrentYearProfitPerShare { get; set; }

        /// <summary>
        /// 是否处于要约收购期
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool OfferingFlag { get; set; }


    }

    /// <summary>
    /// 基金特有字段
    /// </summary>
    public class FundParams : SecurityParamsBase
    {
        /// <summary>
        /// T-1日资金净值
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double NAV { get; set; }
    }

    /// <summary>
    /// 债券特有字段
    /// </summary>
    public class BondParams : SecurityParamsBase
    {
        /// <summary>
        /// 利率形式
        /// </summary>
        /// <remarks>
        /// C1
        /// 0=贴现债券
        /// 1=零息债券 
        /// 2=固定利率 
        /// 3=浮动利率 
        /// </remarks>
        public RateType RateType { get; set; }

        /// <summary>
        /// 付息周期
        /// </summary>
        /// <remarks>
        /// N2
        /// </remarks>
        public Int32 PayCycle { get; set; }

        /// <summary>
        /// 票面年利率
        /// </summary>
        /// <remarks>
        /// N8(4)
        /// </remarks>
        public double CouponRate { get; set; }

        /// <summary>
        /// 贴现发行价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double IssuePrice { get; set; }

        /// <summary>
        /// 每百元应计利息
        /// </summary>
        /// <remarks>
        /// N8(4)
        /// </remarks>
        public double Interest { get; set; }

        /// <summary>
        /// 发行起息日或本次付息起息日
        /// </summary>
        /// <remarks>
        /// N8
        /// </remarks>
        public Int32 InterestAccrualDate { get; set; }

        /// <summary>
        /// 到期日
        /// </summary>
        /// <remarks>
        /// N8
        /// </remarks>
        public Int32 MaturityDate { get; set; }

        /// <summary>
        /// 国债到期年收益率
        /// </summary>
        /// <remarks>
        /// N8(4)
        /// </remarks>
        public double ExpirationRate { get; set; }

        /// <summary>
        /// 是否处于转股回售期
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool OfferingFlag { get; set; }
    }
    
    /// <summary>
    /// 权证特有字段
    /// </summary>
    public class WarrantParams : SecurityParamsBase
    {
        /// <summary>
        /// 行权价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double ExercisePrice { get; set; }
        /// <summary>
        /// 行权比例
        /// </summary>
        /// <remarks>
        /// N10(4)
        /// </remarks>
        public double ExerciseRatio { get; set; }

        /// <summary>
        /// 行权起始日
        /// </summary>
        ///<remarks>
        ///N8
        /// </remarks>
        public Int32 ExericseBeginDate { get; set; }

        /// <summary>
        /// 行权截止日
        /// </summary>
        ///<remarks>
        ///N8
        /// </remarks>
        public Int32 ExerciseEndDate { get; set; }

        /// <summary>
        /// 认购或认沽
        /// </summary>
        /// <remarks>
        /// C1
        /// C=Call
        /// P=Put
        /// </remarks>
        public CallOrPut CallOrPut { get; set; }

        /// <summary>
        /// 结算方式
        /// </summary>
        /// <remarks>
        /// C1
        /// S=证券结算
        /// C=现金结算
        /// </remarks>
        public WarrantClearingType WarrantClearingType { get; set; }

        /// <summary>
        /// 结算价格
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double SettlePrice { get; set; }


        /// <summary>
        /// 行权方式
        /// </summary>
        ///<remarks>
        ///C1
        ///A=美式
        ///E=欧式
        ///B=百慕大式
        /// </remarks>
        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// 是否处于行权期
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool ExerciseFlag { get; set; }

    }

    /// <summary>
    /// 质押式回购交易代码特有字段
    /// </summary>
    public class RepoParams : SecurityParamsBase
    {
        /// <summary>
        /// 购回期限
        /// </summary>
        /// <remarks>
        /// N4
        /// </remarks>
        public Int32 ExpirationDays { get; set; }
    }

    /// <summary>
    /// 期权特有字段
    /// </summary>
    public class OptionParams : SecurityParamsBase
    {
        /// <summary>
        /// 认购或认沽
        /// </summary>
        /// <remarks>
        /// C1
        /// C=Call
        /// P=Put
        /// </remarks>
        public CallOrPut CallOrPut { get; set; }

        /// <summary>
        /// 交割月份
        /// </summary>
        /// <remarks>
        /// N4
        /// 格式为YYMM
        /// </remarks>
        public Int32 DeliveryMonth { get; set; }

        /// <summary>
        /// 行权起始日期
        /// </summary>
        ///<remarks>
        ///N8
        /// </remarks>
        public Int32 ExerciseBeginDate { get; set; }

        /// <summary>
        /// 行权结束日期
        /// </summary>
        ///<remarks>
        ///N8
        /// </remarks>
        public Int32 ExerciseEndDate { get; set; }

        /// <summary>
        /// 行权价
        /// </summary>
        ///<remarks>
        ///N13(4)
        /// </remarks>
        public double ExercisePrice { get; set; }

            /// <summary>
        /// 最后交易日
        /// </summary>
        /// <remarks>
        /// N8
        /// </remarks>
        public Int32 LastTradeDay { get; set; }

        /// <summary>
        /// 是否调整
        /// </summary>
        /// <remarks>
        /// C1
        /// Y=是
        /// N=否
        /// </remarks>
        public bool Adjusted { get; set; }

        /// <summary>
        /// 调整次数
        /// </summary>
        public Int32 AdjuestTimes { get; set; }


        /// <summary>
        /// 合约单位
        /// </summary>
        /// <remarks>
        /// N15(2)
        /// </remarks>
        public double ContractUnit { get; set; }

        /// <summary>
        /// 昨日结算价
        /// </summary>
        /// <remarks>
        /// N13(4)
        /// </remarks>
        public double PrevSettlePrice { get; set; }

        /// <summary>
        /// 合约持仓量
        /// </summary>
        /// <remarks>
        /// N18(2)
        /// </remarks>
        public decimal ContractPosition { get; set; }
    }
}
