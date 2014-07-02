using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace MDS.Plugin.StockV5
{
    public class QuotationManager
    {
        private Log4cb.ILog4cbHelper logHelper;
        private QuotV5.Binary.RealTimeQuotConnection quotConn;
        private QuotationRepository quotRepository;
        private QuotationPublisher quotPublisher;

        private SecurityInfoProvider securityInfoProvider;
        private IndexInfoProvider indexInfoProvider;
        private CashAuctionParamsProvider cashAuctionParamsProvider;
        private DerivativeAuctionParamsProvider derivativeAuctionParamsProvider;
        private NegotiationParamsProvider negotiationParamsProvider;

        public QuotationManager(
            QuotV5.Binary.RealTimeQuotConnection quotConn,
            QuotationRepository quotRepository,
            QuotationPublisher quotPublisher,
            Log4cb.ILog4cbHelper logHelper
            )
        {
            this.quotConn = quotConn;
            this.quotRepository = quotRepository;
            this.quotPublisher = quotPublisher;
            this.logHelper = logHelper;

            this.quotConn.OnMarketDataReceived += new Action<QuotV5.Binary.IMarketData>(QuotConn_OnMarketDataReceived);

            this.securityInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.SecurityInfoBase>>(SecurityInfoProvider_OnStaticInfoRead);
            this.indexInfoProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.IndexInfo>>(IndexInfoProvider_OnStaticInfoRead);
            this.cashAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.CashAuctionParams>>(CashAuctionParamsProvider_OnStaticInfoRead);
            this.derivativeAuctionParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.DerivativeAuctionParams>>(DerivativeAuctionParamsProvider_OnStaticInfoRead);
            this.negotiationParamsProvider.OnStaticInfoRead += new Action<List<QuotV5.StaticInfo.NegotiationParams>>(NegotiationParamsProvider_OnStaticInfoRead);

        }


        public void Start()
        {

        }
        public void Stop()
        {

        }

        private void LoadSoredDataSnap()
        {

        }

        private void QuotConn_OnMarketDataReceived(QuotV5.Binary.IMarketData marketData)
        {

        }

        #region 静态信息更新
        private void SecurityInfoProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.SecurityInfoBase> securityInfos)
        {
            if (securityInfos == null)
                return;
            foreach (var securityInfo in securityInfos)
            {
                RawStaticInfoSnap.SecurityInfo[securityInfo.CommonInfo.SecurityID] = securityInfo;
            }

        }
        private void IndexInfoProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.IndexInfo> securityInfos)
        {
            if (securityInfos == null)
                return;
            foreach (var securityInfo in securityInfos)
            {
                RawStaticInfoSnap.IndexInfo[securityInfo.SecurityID] = securityInfo;
            }
        }
        private void CashAuctionParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.CashAuctionParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            foreach (var securityInfo in securityInfos)
            {
                RawStaticInfoSnap.CashAuctionParams[securityInfo.SecurityID] = securityInfo;
            }
        }
        private void DerivativeAuctionParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.DerivativeAuctionParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            foreach (var securityInfo in securityInfos)
            {
                RawStaticInfoSnap.DerivativeAuctionParams[securityInfo.SecurityID] = securityInfo;
            }
        }
        private void NegotiationParamsProvider_OnStaticInfoRead(List<QuotV5.StaticInfo.NegotiationParams> securityInfos)
        {
            if (securityInfos == null)
                return;
            foreach (var securityInfo in securityInfos)
            {
                RawStaticInfoSnap.NegotiationParams[securityInfo.SecurityID] = securityInfo;
            }
        }



        #endregion


        private StockInfo ConstructStockInfo(
            QuotV5.Binary.RealtimeStatus status,
            QuotV5.Binary.QuotSnap300111 quotSnap,
            QuotV5.StaticInfo.SecurityInfoBase securityInfo,
            QuotV5.StaticInfo.CashAuctionParams cashAuctionParams,
            QuotV5.StaticInfo.EVoteParams eVoteParams)
        {

            Contract.Requires(securityInfo != null);
            Contract.Requires(cashAuctionParams != null);

            StockInfo rtn = new StockInfo()
            {
                exchId = '0',
                stkId = securityInfo.CommonInfo.SecurityID,
                stkName = securityInfo.CommonInfo.Symbol,
                stkEnglishtAbbr = securityInfo.CommonInfo.EnglishName,
                tradeUnit = securityInfo.CommonInfo.QtyUnit,
                stkParValue = securityInfo.CommonInfo.ParValue,
                totalCurrentStkQty = securityInfo.CommonInfo.PublicFloatShareQuantity,
                listedDate = securityInfo.CommonInfo.ListDate,
                standardConvertRate = securityInfo.CommonInfo.ContractMultiplier,
                closePrice = securityInfo.CommonInfo.PrevClosePx,
                basicStkId = securityInfo.CommonInfo.UnderlyingSecurityID,
                ISINCode = securityInfo.CommonInfo.ISIN,
                CMOStandardRate = securityInfo.CommonInfo.GageRatio,
                totalStkQty = securityInfo.CommonInfo.OutstandingShare,
                isCreditCashStk = securityInfo.CommonInfo.CrdBuyUnderlying,
                isCreditShareStk = securityInfo.CommonInfo.CrdSellUnderlying,

                buyQtyUnit = cashAuctionParams.BuyQtyUnit,
                sellQtyUnit = cashAuctionParams.SellQtyUnit,
                orderPriceUnit = cashAuctionParams.PriceTick,
                marketMarkerFlag = cashAuctionParams.MarketMakerFlag,

                exchTotalKnockQty = quotSnap.CommonInfo.TotalValueTrade,
                exchTotalKnockAmt = quotSnap.CommonInfo.TotalValueTrade,
                tradeTimeFlag = quotSnap.CommonInfo.TradingPhaseCode,

                stkIdPrefix = status.Status.SecurityPreName,


                newPrice = 0,
                highPrice = 0,
                lowPrice = 0,
                maxOrderPrice = 0,
                minOrderPrice = 0,
                aggPriceLimit = 0,
                contPriceLimit = 0,
                priceLimitFlag = default(char),
                tradeStatus = null,
                stkLevel = null,
                closeFlag = default(char),
                stkAllotFlag = default(char),
                stkIndexFlag = default(char),
                creditShareSellPriceFlag = default(char),
                exchTradeType = null,
                otherBusinessMark = null,
                passCreditCashStk = default(char),
                passCreditShareStk = default(char),
                pauseTradeStatus = false,

                netVoteFlag = false,
                #region 根据证券类型，从不同的扩展信息中获取的字段

                stkIndustryType = null,
                lastYearProfit = 0,
                thisYearProfit = 0,
                endingDate = 0,
                accuredInterest = 0,
                beginInterestDate = 0,
                NAV = 0,
                #endregion

                #region 无法获取的字段

                exchMemo = null,
                bulkTradingMaxPrice = 0,
                bulkTradingMinPrice = 0,
                #endregion
            };

            if (securityInfo is QuotV5.StaticInfo.StockSecurityInfo)
            {
                QuotV5.StaticInfo.StockParams para = (securityInfo as QuotV5.StaticInfo.StockSecurityInfo).Params;
                rtn.stkIndustryType = para.IndustryClassification;
                rtn.lastYearProfit = para.PreviousYearProfitPerShare;
                rtn.thisYearProfit = para.CurrentYearProfitPerShare;
            }
            else if (securityInfo is QuotV5.StaticInfo.BondSecurityInfo)
            {
                QuotV5.StaticInfo.BondParams para = (securityInfo as QuotV5.StaticInfo.BondSecurityInfo).Params;
                rtn.endingDate = para.MaturityDate;
                rtn.accuredInterest = para.Interest;
                rtn.beginInterestDate = para.InterestAccrualDate;
            }
            else if (securityInfo is QuotV5.StaticInfo.FundSecurityInfo)
            {
                QuotV5.StaticInfo.FundParams para = (securityInfo as QuotV5.StaticInfo.FundSecurityInfo).Params;
                rtn.NAV = para.NAV;
            }

            if (eVoteParams != null)
                rtn.netVoteFlag = true;

            return rtn;
        }




        private QuotationInfo ConstructQuotationInfo(
            QuotV5.Binary.RealtimeStatus status,
            QuotV5.Binary.QuotSnap300111 quotSnap,
            QuotV5.StaticInfo.StockSecurityInfo securityInfo)
        {
            QuotationInfo rtn = new QuotationInfo()
            {
                exchId = '0',
                stkId = securityInfo.CommonInfo.SecurityID,
                stkName = securityInfo.CommonInfo.Symbol,
                knockQty = quotSnap.CommonInfo.TotalVolumeTrade,
                knockMoney = quotSnap.CommonInfo.TotalValueTrade,

                #region 不用赋值的字段
                closeIndex = 0,
                openIndex = 0,
                highIndex = 0,
                lowIndex = 0,
                newIndex = 0,
                #endregion

                #region 稍后赋值的字段

                #region 盘口
                buyPrice1 = 0,
                buyPrice2 = 0,
                buyPrice3 = 0,
                buyPrice4 = 0,
                buyPrice5 = 0,
                sellPrice1 = 0,
                sellPrice2 = 0,
                sellPrice3 = 0,
                sellPrice4 = 0,
                sellPrice5 = 0,
                buyQty1 = 0,
                buyQty2 = 0,
                buyQty3 = 0,
                buyQty4 = 0,
                buyQty5 = 0,
                sellQty1 = 0,
                sellQty2 = 0,
                sellQty3 = 0,
                sellQty4 = 0,
                sellQty5 = 0,
                #endregion
                closeFlag = false,
                change = 0,
                changePercent = 0,
                #endregion
            };

            rtn.change = rtn.newPrice - rtn.closePrice;
            if (rtn.closePrice != 0.0m)
                rtn.changePercent = rtn.change / rtn.closePrice;



            SetBidAsks(rtn, quotSnap.ExtInfo.MDEntries);
            return rtn;
        }

        /// <summary>
        /// 构建指数的行情信息
        /// </summary>
        /// <param name="status"></param>
        /// <param name="quotSnap"></param>
        /// <param name="securityInfo"></param>
        /// <returns></returns>
        private QuotationInfo ConstructQuotationInfo(
           QuotV5.Binary.RealtimeStatus status,
           QuotV5.Binary.QuotSnap309011 quotSnap,
           QuotV5.StaticInfo.IndexInfo securityInfo)
        {

            QuotationInfo rtn = new QuotationInfo()
            {
                exchId = '0',
                stkId = securityInfo.SecurityID,
                stkName = securityInfo.Symbol,
                knockQty = quotSnap.CommonInfo.TotalVolumeTrade,
                knockMoney = quotSnap.CommonInfo.TotalValueTrade,





                #region 不用赋值的字段
                closePrice = 0,
                openPrice = 0,
                highPrice = 0,
                lowPrice = 0,
                newPrice = 0,

                #region 盘口
                buyPrice1 = 0,
                buyPrice2 = 0,
                buyPrice3 = 0,
                buyPrice4 = 0,
                buyPrice5 = 0,
                sellPrice1 = 0,
                sellPrice2 = 0,
                sellPrice3 = 0,
                sellPrice4 = 0,
                sellPrice5 = 0,
                buyQty1 = 0,
                buyQty2 = 0,
                buyQty3 = 0,
                buyQty4 = 0,
                buyQty5 = 0,
                sellQty1 = 0,
                sellQty2 = 0,
                sellQty3 = 0,
                sellQty4 = 0,
                sellQty5 = 0,
                #endregion

                #endregion

                #region 稍后赋值的字段
                closeFlag = false,
                change = 0,
                changePercent = 0,
                closeIndex = 0,
                openIndex = 0,
                highIndex = 0,
                lowIndex = 0,
                newIndex = 0,
                #endregion
            };

            rtn.change = rtn.newIndex - rtn.closeIndex;
            if (rtn.closeIndex != 0.0m)
                rtn.changePercent = rtn.change / rtn.closeIndex;


            return rtn;
        }

        private void SetBidAsks(QuotationInfo quotInfo, IEnumerable<QuotV5.Binary.QuotSnapExtInfo300111.MDEntry> mdEntries)
        {

        }
    }


}
