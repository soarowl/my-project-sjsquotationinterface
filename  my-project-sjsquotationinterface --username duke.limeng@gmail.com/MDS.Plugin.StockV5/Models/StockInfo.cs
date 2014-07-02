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
        public char exchId;
       
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
        public double tradeUnit;

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
        public decimal totalCurrentStkQty;
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
        public Int32 listedDate;
        /// <summary>
        /// 到期/交割日
        /// </summary>
        public Int32 endingDate;

        /// <summary>
        /// 买数量单位
        /// </summary>
        public double buyQtyUnit;

        /// <summary>
        /// 卖数量单位
        /// </summary>
        public double sellQtyUnit;

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
        public char priceLimitFlag;

        /// <summary>
        /// 涨停价格
        /// </summary>
        public double maxOrderPrice;

        /// <summary>
        /// 跌停价格
        /// </summary>
        public double minOrderPrice;

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
        public char closeFlag;

        /// <summary>
        /// 除权除息标志
        /// </summary>
        public char stkAllotFlag;

        /// <summary>
        /// 成标志
        /// </summary>
        public char stkIndexFlag;

        /// <summary>
        /// 昨收价
        /// </summary>
        public double closePrice;

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
        public decimal totalStkQty;

        /// <summary>
        /// 证券简称前缀
        /// </summary>
        public string stkIdPrefix;

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
        public Int32 beginInterestDate;

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
        public bool isCreditCashStk;

        /// <summary>
        /// 融券标的标志
        /// </summary>
        public bool isCreditShareStk;

        /// <summary>
        /// 做市商标志
        /// </summary>
        public bool marketMarkerFlag;

        /// <summary>
        /// 交易类型
        /// </summary>
        public string exchTradeType;

        /// <summary>
        /// 产品交易阶段
        /// </summary>
        public string tradeTimeFlag;

        /// <summary>
        /// 暂停交易标志
        /// </summary>
        public bool pauseTradeStatus;

        /// <summary>
        /// 融资交易状态
        /// </summary>
        public char passCreditCashStk;

        /// <summary>
        /// 融券交易状态
        /// </summary>
        public char passCreditShareStk;

        /// <summary>
        /// 融券卖出价格
        /// </summary>
        public char creditShareSellPriceFlag;

        /// <summary>
        /// 网络投票标志
        /// </summary>
        public bool netVoteFlag;

        /// <summary>
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark;

        /// <summary>
        /// 备用字段
        /// </summary>
        public string exchMemo;

        /// <summary>
        /// 当前价
        /// </summary>
        public double newPrice;

        /// <summary>
        /// 最高价
        /// </summary>
        public double highPrice;
        
        /// <summary>
        /// 最低价
        /// </summary>
        public double lowPrice;
    }
}
