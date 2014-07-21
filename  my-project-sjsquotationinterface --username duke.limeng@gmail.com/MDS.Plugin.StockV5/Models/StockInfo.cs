using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    public class StockInfo
    {
        public StockInfo()
        {
            this.exchId = "1";
        }
        /// <summary>
        /// 市场代码
        /// </summary>
        public string exchId{get;set;}
       
        /// <summary>
        /// 证券代码
        /// </summary>
        public string stkId{get;set;}
        
        /// <summary>
        /// 证券名称
        /// </summary>
        public string stkName{get;set;}

        /// <summary>
        /// 英文简称
        /// </summary>
        public string stkEnglishtAbbr{get;set;}
        
        /// <summary>
        /// 交易单位
        /// </summary>
        public Int16 tradeUnit{get;set;}

        /// <summary>
        /// 行业种类
        /// </summary>
        public string stkIndustryType{get;set;}
        
        /// <summary>
        /// 每股面值
        /// </summary>
        public double stkParValue{get;set;}

        /// <summary>
        /// 流通股数
        /// </summary>
        public Int64 totalCurrentStkQty{get;set;}
        
        /// <summary>
        /// 上年利润
        /// </summary>
        public double lastYearProfit{get;set;}
       
        /// <summary>
        /// 本年利润
        /// </summary>
        public double thisYearProfit{get;set;}
       
        /// <summary>
        /// 上市日期
        /// </summary>
        public Int64 listedDate{get;set;}
        
        /// <summary>
        /// 到期/交割日
        /// </summary>
        public Int64 endingDate{get;set;}

        /// <summary>
        /// 买委托上限
        /// </summary>
        public Int32 buyQtyUpperLimit{get;set;}

        /// <summary>
        /// 卖委托上限
        /// </summary>
        public Int32 sellQtyUpperLimit{get;set;}

        /// <summary>
        /// 价格档位
        /// </summary>
        public double orderPriceUnit{get;set;}

        /// <summary>
        /// 集合竞价限价
        /// </summary>
        public double aggPriceLimit{get;set;}

        /// <summary>
        /// 连续竞价限价
        /// </summary>
        public double contPriceLimit{get;set;}

        /// <summary>
        /// 限价参数
        /// </summary>
        public Int32 priceLimitFlag{get;set;}

        /// <summary>
        /// 折合比例
        /// </summary>
        public double standardConvertRate{get;set;}

        /// <summary>
        /// 交易状态
        /// </summary>
        public string tradeStatus{get;set;}

        /// <summary>
        /// 证券级别
        /// </summary>
        public string stkLevel{get;set;}

        /// <summary>
        /// 停牌标志
        /// </summary>
        public string closeFlag{get;set;}

        /// <summary>
        /// 除权除息标志
        /// </summary>
        public string stkAllotFlag{get;set;}

        /// <summary>
        /// 成标志
        /// </summary>
        public string stkIndexFlag{get;set;}

        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal closePrice{get;set;}

        /// <summary>
        /// 成交数量
        /// </summary>
        public Int64 exchTotalKnockQty{get;set;}

        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal exchTotalKnockAmt{get;set;}

        /// <summary>
        /// 百元利息额
        /// </summary>
        public double accuredInterest{get;set;}

        /// <summary>
        /// 总股本
        /// </summary>
        public Int64 totalStkQty{get;set;}


        /// <summary>
        /// 基础证券
        /// </summary>
        public string basicStkId{get;set;}

        /// <summary>
        /// ISIN编码
        /// </summary>
        public string ISINCode{get;set;}

        /// <summary>
        /// 基金份额累计净值
        /// </summary>
        public double NAV{get;set;}

        /// <summary>
        /// 债券起息日
        /// </summary>
        public Int64 beginInterestDate{get;set;}

        /// <summary>
        /// 大宗交易价格上限
        /// </summary>
        public double bulkTradingMaxPrice{get;set;}

        /// <summary>
        /// 大宗交易价格下限
        /// </summary>
        public double bulkTradingMinPrice{get;set;}

        /// <summary>
        /// 担保折扣率
        /// </summary>
        public double CMOStandardRate{get;set;}

        /// <summary>
        /// 融资标的标志
        /// </summary>
        public bool isCreditCashStk{get;set;}

        /// <summary>
        /// 融券标的标志
        /// </summary>
        public bool isCreditShareStk{get;set;}

        /// <summary>
        /// 做市商标志
        /// </summary>
        public bool marketMakerFlag{get;set;}

        /// <summary>
        /// 交易类型
        /// </summary>
        public string exchTradeType{get;set;}


        /// <summary>
        /// 暂停交易标志
        /// </summary>
        public string pauseTradeStatus{get;set;}

        /// <summary>
        /// 融券卖出价格
        /// </summary>
        public string creditShareSellPriceFlag{get;set;}

        /// <summary>
        /// 网络投票标志
        /// </summary>
        public bool netVoteFlag{get;set;}

        /// <summary>
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark{get;set;}

        public StockInfo Clone()
        {
            return MemberwiseClone() as StockInfo;
        }
    }
}
