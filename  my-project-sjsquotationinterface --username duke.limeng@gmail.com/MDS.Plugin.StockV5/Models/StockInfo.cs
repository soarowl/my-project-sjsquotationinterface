using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.StockV5
{
    public struct StockInfo
    {
        /// <summary>
        /// 市场代码
        /// </summary>
        public string exchId;
       
        /// <summary>
        /// 证券代码
        /// </summary>
        public string stkId;
        
        /// <summary>
        /// 证券名称
        /// </summary>
        public string stkName;

        /// <summary>
        /// 英文简称
        /// </summary>
        public string stkEnglishtAbbr;
        
        /// <summary>
        /// 交易单位
        /// </summary>
        public Int16 tradeUnit;

        /// <summary>
        /// 行业种类
        /// </summary>
        public string stkIndustryType;
        
        /// <summary>
        /// 每股面值
        /// </summary>
        public double stkParValue;

        /// <summary>
        /// 流通股数
        /// </summary>
        public Int64 totalCurrentStkQty;
        
        /// <summary>
        /// 上年利润
        /// </summary>
        public double lastYearProfit;
       
        /// <summary>
        /// 本年利润
        /// </summary>
        public double thisYearProfit;
       
        /// <summary>
        /// 上市日期
        /// </summary>
        public Int64 listedDate;
        
        /// <summary>
        /// 到期/交割日
        /// </summary>
        public Int64 endingDate;

        /// <summary>
        /// 买委托上限
        /// </summary>
        public Int32 buyQtyUpperLimit;

        /// <summary>
        /// 卖委托上限
        /// </summary>
        public Int32 sellQtyUpperLimit;

        /// <summary>
        /// 价格档位
        /// </summary>
        public double orderPriceUnit;

        /// <summary>
        /// 集合竞价限价
        /// </summary>
        public double aggPriceLimit;

        /// <summary>
        /// 连续竞价限价
        /// </summary>
        public double contPriceLimit;

        /// <summary>
        /// 限价参数
        /// </summary>
        public Int32 priceLimitFlag;

        /// <summary>
        /// 折合比例
        /// </summary>
        public double standardConvertRate;

        /// <summary>
        /// 交易状态
        /// </summary>
        public string tradeStatus;

        /// <summary>
        /// 证券级别
        /// </summary>
        public string stkLevel;

        /// <summary>
        /// 停牌标志
        /// </summary>
        public string closeFlag;

        /// <summary>
        /// 除权除息标志
        /// </summary>
        public string stkAllotFlag;

        /// <summary>
        /// 成标志
        /// </summary>
        public string stkIndexFlag;

        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal closePrice;

        /// <summary>
        /// 成交数量
        /// </summary>
        public Int64 exchTotalKnockQty;

        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal exchTotalKnockAmt;

        /// <summary>
        /// 百元利息额
        /// </summary>
        public double accuredInterest;

        /// <summary>
        /// 总股本
        /// </summary>
        public Int64 totalStkQty;


        /// <summary>
        /// 基础证券
        /// </summary>
        public string basicStkId;

        /// <summary>
        /// ISIN编码
        /// </summary>
        public string ISINCode;

        /// <summary>
        /// 基金份额累计净值
        /// </summary>
        public double NAV;

        /// <summary>
        /// 债券起息日
        /// </summary>
        public Int64 beginInterestDate;

        /// <summary>
        /// 大宗交易价格上限
        /// </summary>
        public double bulkTradingMaxPrice;

        /// <summary>
        /// 大宗交易价格下限
        /// </summary>
        public double bulkTradingMinPrice;

        /// <summary>
        /// 担保折扣率
        /// </summary>
        public double CMOStandardRate;

        /// <summary>
        /// 融资标的标志
        /// </summary>
        public string isCreditCashStk;

        /// <summary>
        /// 融券标的标志
        /// </summary>
        public string isCreditShareStk;

        /// <summary>
        /// 做市商标志
        /// </summary>
        public string marketMarkerFlag;

        /// <summary>
        /// 交易类型
        /// </summary>
        public string exchTradeType;


        /// <summary>
        /// 暂停交易标志
        /// </summary>
        public string pauseTradeStatus;

        /// <summary>
        /// 融券卖出价格
        /// </summary>
        public string creditShareSellPriceFlag;

        /// <summary>
        /// 网络投票标志
        /// </summary>
        public string netVoteFlag;

        /// <summary>
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark;

        /// <summary>
        /// 备用字段
        /// </summary>
        public string exchMemo;


    }
}
