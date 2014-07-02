using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.StockV5
{
    public struct QuotationInfo
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
        /// 证券简称前缀
        /// </summary>
        public string stkIdPrefix;

        /// <summary>
        /// 证券名称
        /// </summary>
        public string stkName;

        /// <summary>
        /// 行情标志
        /// </summary>
        public string levelFlag;

        /// <summary>
        /// 停牌标志
        /// </summary>
        public bool closeFlag;

        /// <summary>
        /// 昨收价
        /// </summary>
        public decimal closePrice;

        /// <summary>
        /// 今开价
        /// </summary>
        public double openPrice;

        /// <summary>
        /// 当前价
        /// </summary>
        public decimal newPrice;

        /// <summary>
        /// 最高价
        /// </summary>
        public double highPrice;

        /// <summary>
        /// 最低价
        /// </summary>
        public double lowPrice;

        /// <summary>
        /// 成交量
        /// </summary>
        public Int64 knockQty;

        /// <summary>
        /// 成交金额
        /// </summary>
        public decimal knockMoney;

        public double buyPrice1;
        public double buyPrice2;
        public double buyPrice3;
        public double buyPrice4;
        public double buyPrice5;

        public double sellPrice1;
        public double sellPrice2;
        public double sellPrice3;
        public double sellPrice4;
        public double sellPrice5;

        public Int64 buyQty1;
        public Int64 buyQty2;
        public Int64 buyQty3;
        public Int64 buyQty4;
        public Int64 buyQty5;

        public Int64 sellQty1;
        public Int64 sellQty2;
        public Int64 sellQty3;
        public Int64 sellQty4;
        public Int64 sellQty5;

        /// <summary>
        /// 基金净值
        /// </summary>
        public double IOPV;

        /// <summary>
        /// 涨停价格
        /// </summary>
        public double maxOrderPrice;

        /// <summary>
        /// 跌停价格
        /// </summary>
        public double minOrderPrice;

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
        /// 其它业务状态
        /// </summary>
        public string otherBusinessMark;

        /// <summary>
        /// 价格变动
        /// </summary>
        public decimal change;

        /// <summary>
        /// 价格变动率
        /// </summary>
        public decimal changePercent;

        /// <summary>
        /// 昨收指数
        /// </summary>
        public decimal closeIndex;

        /// <summary>
        /// 今开指数
        /// </summary>
        public double openIndex;

        /// <summary>
        /// 最高指数
        /// </summary>
        public double highIndex;

        /// <summary>
        /// 最低指数
        /// </summary>
        public double lowIndex;

        /// <summary>
        /// 行情时间
        /// </summary>
        public decimal newIndex;

        /// <summary>
        /// MDS 收到行情的时间
        /// </summary>
        public Int64 ReceiveTime;

        /// <summary>
        /// MDS将行情推到MQ的时间
        /// </summary>
        public Int64 SendTime;
    
    }

}
