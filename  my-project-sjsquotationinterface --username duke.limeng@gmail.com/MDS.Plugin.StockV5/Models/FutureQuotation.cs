using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    public class FutureQuotation
    {
        public FutureQuotation()
        {
            this.exchId = "X";
            this.upPercent = 0;
            this.downPercent = 0;
            this.qtyPerHand = 1;
            this.tradeUnit = 1;
            this.deliveryUnit = 1;
            this.strikeStyle = "E";
            this.marginRate1 = 0;
            this.marginRate2 = 0;
        }


        /// <summary>
        /// 市场代码
        /// </summary>
        public string exchId { get; set; }
        /// <summary>
        /// 产品代码
        /// </summary>
        public string F_productId { get; set; }
        /// <summary>
        /// 结算组
        /// </summary>
        public string settleGrp { get; set; }
        /// <summary>
        /// 结算编号
        /// </summary>
        public string settleID { get; set; }
        /// <summary>
        /// 产品类型
        /// </summary>
        public string F_ProductClass { get; set; }
        /// <summary>
        /// 产品组
        /// </summary>
        public string productGrp { get; set; }
        /// <summary>
        /// 产品代码
        /// </summary>
        public string productCode { get; set; }
        /// <summary>
        /// 合约代码
        /// </summary>
        public string stkId { get; set; }
        /// <summary>
        /// 合约名称
        /// </summary>
        public string stkName { get; set; }
        /// <summary>
        /// 合约状态（未上市、上市、停牌、到期）
        /// </summary>
        public string stkStatus { get; set; }
        /// <summary>
        /// 合约标的市场
        /// </summary>
        public string basicExchId { get; set; }
        /// <summary>
        /// 合约标的代码
        /// </summary>
        public string basicStkId { get; set; }
        /// <summary>
        /// 合约单位
        /// </summary>
        public Int32 contractTimes { get; set; }
        /// <summary>
        /// 交割方式
        /// </summary>
        public string deliveryType { get; set; }
        /// <summary>
        /// 交割年份
        /// </summary>
        public Int32 deliveryYear { get; set; }
        /// <summary>
        /// 交割月份
        /// </summary>
        public Int32 deliveryMonth { get; set; }
        /// <summary>
        /// 上市日
        /// </summary>
        public Int64 listDate { get; set; }
        /// <summary>
        /// 首交易日
        /// </summary>
        public Int64 firstTrdDate { get; set; }
        /// <summary>
        /// 最后交易日
        /// </summary>
        public Int64 lastTrdDate { get; set; }
        /// <summary>
        /// 到期日
        /// </summary>
        public Int64 matureDate { get; set; }
        /// <summary>
        /// 最后结算日
        /// </summary>
        public Int64 lastSettleDate { get; set; }
        /// <summary>
        /// 交割日
        /// </summary>
        public Int64 deliveryDate { get; set; }
        /// <summary>
        /// 合约交易状态
        /// </summary>
        public string stkOrderStatus { get; set; }
        /// <summary>
        /// 价格档位
        /// </summary>
        public double orderPriceUnit { get; set; }
        /// <summary>
        /// 限价委托上限数量(每笔最大限量)
        /// </summary>
        public Int32 buyMaxLimitOrderQty { get; set; }
        /// <summary>
        /// 限价委托下限数量(每单最小数量单位)
        /// </summary>
        public Int32 buyMinLimitOrderQty { get; set; }
        /// <summary>
        /// 限价委托上限数量(每笔最大限量)
        /// </summary>
        public Int32 sellMaxLimitOrderQty { get; set; }
        /// <summary>
        /// 限价委托下限数量(每单最小数量单位)
        /// </summary>
        public Int32 sellMinLimitOrderQty { get; set; }
        /// <summary>
        /// 市价委托上限数量(每笔最大限量)
        /// </summary>
        public Int32 buyMaxMarketOrderQty { get; set; }
        /// <summary>
        /// 市价委托下限数量(每单最小数量单位)
        /// </summary>
        public Int32 buyMinMarketOrderQty { get; set; }
        /// <summary>
        /// 市价委托上限数量(每笔最大限量)
        /// </summary>
        public Int32 sellMaxMarketOrderQty { get; set; }
        /// <summary>
        /// 市价委托下限数量(每单最小数量单位)
        /// </summary>
        public Int32 sellMinMarketOrderQty { get; set; }

        /// <summary>
        /// 委托价格上限(涨停价格)
        /// </summary>
        public decimal maxOrderPrice { get; set; }
        /// <summary>
        /// 委托价格下限(跌停价格)
        /// </summary>
        public decimal minOrderPrice { get; set; }
        /// <summary>
        /// 涨幅比例限制
        /// </summary>
        public decimal upPercent { get; set; }
        /// <summary>
        /// 跌幅比例限制
        /// </summary>
        public decimal downPercent { get; set; }
        /// <summary>
        /// 最新价
        /// </summary>
        public decimal newPrice { get; set; }
        /// <summary>
        /// 昨结算
        /// </summary>
        public decimal preSettlementPrice { get; set; }
        /// <summary>
        /// 昨收盘
        /// </summary>
        public decimal preClosePrice { get; set; }
        /// <summary>
        /// 市场昨持仓量
        /// </summary>
        public decimal preOpenPosition { get; set; }
        /// <summary>
        /// 今开盘
        /// </summary>
        public decimal openPrice { get; set; }
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal highestPrice { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public decimal lowestPrice { get; set; }
        /// <summary>
        /// 总成交数量
        /// </summary>
        public Int64 exchTotalKnockQty { get; set; }
        /// <summary>
        /// 成交金额 
        /// </summary>
        public decimal exchTotalKnockAmt { get; set; }
        /// <summary>
        /// 市场持仓量
        /// </summary>
        public decimal openPosition { get; set; }
        /// <summary>
        /// 今收盘
        /// </summary>
        public decimal closePrice { get; set; }
        /// <summary>
        /// 今结算
        /// </summary>
        public decimal settlementPrice { get; set; }
        /// <summary>
        /// 昨Delta
        /// </summary>
        public decimal preDelta { get; set; }
        /// <summary>
        /// delta
        /// </summary>
        public decimal delta { get; set; }
        /// <summary>
        /// 最后修改时间
        /// </summary>
        public Int64 lastModifyTime { get; set; }
        /// <summary>
        /// isinCode
        /// </summary>
        public string isinCode { get; set; }
        /// <summary>
        /// isinName
        /// </summary>
        public string isinName { get; set; }
        /// <summary>
        /// 买一价
        /// </summary>
        public decimal buy1 { get; set; }
        /// <summary>
        /// 买一量
        /// </summary>
        public Int64 buyAmt1 { get; set; }
        /// <summary>
        /// 卖一价
        /// </summary>
        public decimal sell1 { get; set; }
        /// <summary>
        /// 卖一量
        /// </summary>
        public Int64 sellAmt1 { get; set; }
        /// <summary>
        /// 买二价
        /// </summary>
        public decimal buy2 { get; set; }
        /// <summary>
        /// 买二量
        /// </summary>
        public Int64 buyAmt2 { get; set; }

        public decimal sell2 { get; set; }
        public Int64 sellAmt2 { get; set; }
        public decimal buy3 { get; set; }
        public Int64 buyAmt3 { get; set; }
        public decimal sell3 { get; set; }
        public Int64 sellAmt3 { get; set; }
        public decimal buy4 { get; set; }
        public Int64 buyAmt4 { get; set; }
        public decimal sell4 { get; set; }
        public Int64 sellAmt4 { get; set; }
        public decimal buy5 { get; set; }
        public Int64 buyAmt5 { get; set; }
        public decimal sell5 { get; set; }
        public Int64 sellAmt5 { get; set; }

        /// <summary>
        /// 预估结算价
        /// </summary>
        public decimal estSettlementPrice { get; set; }
        /// <summary>
        /// 每手数量 
        /// </summary>
        public Int32 qtyPerHand { get; set; }
        /// <summary>
        /// 交易单位
        /// </summary>
        public Int16 tradeUnit { get; set; }
        /// <summary>
        /// 合约类型
        /// </summary>
        public string stkType { get; set; }
        /// <summary>
        /// 交割单位
        /// </summary>
        public Int32 deliveryUnit { get; set; }
        /// <summary>
        /// 最低价
        /// </summary>
        public decimal lowPrice { get; set; }
        /// <summary>
        /// 最高价
        /// </summary>
        public decimal highPrice { get; set; }
        /// <summary>
        /// 开始价
        /// </summary>
        public decimal beginPrice { get; set; }
        /// <summary>
        /// 结束价
        /// </summary>
        public decimal endPrice { get; set; }
        /// <summary>
        /// 执行价格
        /// </summary>
        public decimal strikePrice { get; set; }
        /// <summary>
        /// 期权类型
        /// </summary>
        public string optionType { get; set; }
        /// <summary>
        /// 行权类型 欧式/美式
        /// </summary>
        public string optExecType { get; set; }
        /// <summary>
        /// 标的证券类型(EBS-ETF，ASH-A股)
        /// </summary>
        public string basicStkType { get; set; }
        /// <summary>
        /// 标的证券昨收盘
        /// </summary>
        public decimal basicPreClosePrice { get; set; }
        /// <summary>
        /// 行权方式欧式美式(欧式-E,美式-A)
        /// </summary>
        public string strikeStyle { get; set; }
        /// <summary>
        /// 行权日(T+1)
        /// </summary>
        public Int64 exerciseDate { get; set; }
        /// <summary>
        /// 持空仓单位保证金
        /// </summary>
        public decimal currMargin { get; set; }
        /// <summary>
        /// 保证金比例1
        /// </summary>
        public decimal marginRate1 { get; set; }
        /// <summary>
        /// 保证金比例2
        /// </summary>
        public decimal marginRate2 { get; set; }
        /// <summary>
        /// 期权合约代码
        /// </summary>
        public string optionStkId { get; set; }
        /// <summary>
        /// 合约交易限制
        /// </summary>
        public string optionPermitList { get; set; }
        /// <summary>
        /// 到期月份类型
        /// </summary>
        public string optionMonth { get; set; }
        /// <summary>
        /// 是否调整
        /// </summary>
        public bool adjustedFlag { get; set; }
        /// <summary>
        /// 调整次数
        /// </summary>
        public Int32 adjustNum { get; set; }
        /// <summary>
        /// 买数量单位
        /// </summary>
        public Int32 minBuyQtyTimes { get; set; }
       /// <summary>
        /// 卖数量单位
       /// </summary>
        public Int32 minSellQtyTimes { get; set; }
        /// <summary>
        /// 昨卖开每张保证金
        /// </summary>
        public decimal preCurrMargin { get; set; }
        /// <summary>
        /// 做市商标志
        /// </summary>
        public bool marketMakerFlag { get; set; }



        public FutureQuotation Clone()
        {
            return MemberwiseClone() as FutureQuotation;
        }
    }
}
