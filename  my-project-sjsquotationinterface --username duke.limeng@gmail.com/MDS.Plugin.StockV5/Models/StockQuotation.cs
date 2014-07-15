using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.SZQuotV5
{
    public class StockQuotation
    {
        public StockQuotation() 
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
        /// 证券简称前缀
        /// </summary>
        public string stkIdPrefix{get;set;}

        /// <summary>
        /// 证券名称
        /// </summary>
        public string stkName{get;set;}

        /// <summary>
        /// 行情标志
        /// </summary>
        public string levelFlag{get;set;}

        /// <summary>
        /// 停牌标志
        /// </summary>
        public string closeFlag{get;set;}

        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal closePrice{get;set;}

        /// <summary>
        /// 今开价
        /// </summary>
        public decimal openPrice{get;set;}

        /// <summary>
        /// 当前价
        /// </summary>
        public decimal newPrice{get;set;}

        /// <summary>
        /// 最高价
        /// </summary>
        public decimal highPrice{get;set;}

        /// <summary>
        /// 最低价
        /// </summary>
        public decimal lowPrice{get;set;}

        /// <summary>
        /// 成交量
        /// </summary>
        public Int64 knockQty{get;set;}

        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal knockMoney{get;set;}

        public decimal buyPrice1{get;set;}
        public decimal buyPrice2{get;set;}
        public decimal buyPrice3{get;set;}
        public decimal buyPrice4{get;set;}
        public decimal buyPrice5{get;set;}

        public decimal sellPrice1{get;set;}
        public decimal sellPrice2{get;set;}
        public decimal sellPrice3{get;set;}
        public decimal sellPrice4{get;set;}
        public decimal sellPrice5{get;set;}

        public Int64 buyQty1{get;set;}
        public Int64 buyQty2{get;set;}
        public Int64 buyQty3{get;set;}
        public Int64 buyQty4{get;set;}
        public Int64 buyQty5{get;set;}

        public Int64 sellQty1{get;set;}
        public Int64 sellQty2{get;set;}
        public Int64 sellQty3{get;set;}
        public Int64 sellQty4{get;set;}
        public Int64 sellQty5{get;set;}

        /// <summary>
        /// 基金净值
        /// </summary>
        public decimal IOPV{get;set;}

        /// <summary>
        /// 涨停价格
        /// </summary>
        public decimal maxOrderPrice{get;set;}

        /// <summary>
        /// 跌停价格
        /// </summary>
        public decimal minOrderPrice{get;set;}

        /// <summary>
        /// 产品交易阶段
        /// </summary>
        public string tradeTimeFlag{get;set;}

        /// <summary>
        /// 暂停交易标志
        /// </summary>
        public bool pauseTradeStatus{get;set;}

        /// <summary>
        /// 融资交易状态
        /// </summary>
        public string passCreditCashStk{get;set;}

        /// <summary>
        /// 融券交易状态
        /// </summary>
        public string passCreditShareStk { get; set; }

        /// <summary>
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark{get;set;}

        /// <summary>
        /// 价格变动
        /// </summary>
        public decimal change{get;set;}

        /// <summary>
        /// 价格变动率
        /// </summary>
        public decimal changePercent{get;set;}

        /// <summary>
        /// 昨收指数
        /// </summary>
        public decimal closeIndex{get;set;}

        /// <summary>
        /// 今开指数
        /// </summary>
        public decimal openIndex{get;set;}

        /// <summary>
        /// 最高指数
        /// </summary>
        public decimal highIndex{get;set;}

        /// <summary>
        /// 最低指数
        /// </summary>
        public decimal lowIndex{get;set;}

        /// <summary>
        /// 最新指数
        /// </summary>
        public decimal newIndex{get;set;}

        /// <summary>
        /// 行情时间（YYYYMMDDHHMMSSsss）
        /// </summary>
        public Int64 changeTime{get;set;}
        
        /// <summary>
        /// MDS 收到行情的时间
        /// </summary>
        public Int64 receiveTime{get;set;}

        /// <summary>
        /// MDS将行情推到MQ的时间
        /// </summary>
        public Int64 sendTime{get;set;}

        public StockQuotation Clone()
        {
            return MemberwiseClone() as StockQuotation;
        }
    }

}
