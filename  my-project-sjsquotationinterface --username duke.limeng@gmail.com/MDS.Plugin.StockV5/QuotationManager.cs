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


                //newPrice = 0,
                //highPrice = 0,
                //lowPrice = 0,
                //maxOrderPrice = 0,
                //minOrderPrice = 0,
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



        /// <summary>
        /// 构建股票的行情信息
        /// </summary>
        /// <param name="status"></param>
        /// <param name="quotSnap"></param>
        /// <param name="securityInfo"></param>
        /// <returns></returns>
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
                closeFlag = default(char),
                change = 0,
                changePercent = 0,
                #endregion
            };

            rtn.change = rtn.newPrice - rtn.closePrice;
            if (rtn.closePrice != 0.0m)
                rtn.changePercent = rtn.change / rtn.closePrice;



            ;
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
                stkIdPrefix = status.Status.SecurityPreName,
                tradeTimeFlag = quotSnap.CommonInfo.TradingPhaseCode,





                #region 不用赋值的字段
                closePrice = 0,
                openPrice = 0,
                highPrice = 0,
                lowPrice = 0,
                newPrice = 0,
                IOPV = 0,
                maxOrderPrice = 0,
                minOrderPrice = 0,

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
                passCreditCashStk = default(char),
                passCreditShareStk = default(char),
                pauseTradeStatus = false,
                closeFlag = default(char),
                change = 0,
                changePercent = 0,
                closeIndex = 0,
                openIndex = 0,
                highIndex = 0,
                lowIndex = 0,
                newIndex = 0,


                SendTime = 0,
                ReceiveTime = 0,
                #endregion

                #region 无法获取的字段
                levelFlag = null,
                otherBusinessMark = null,
                #endregion
            };

            rtn.change = rtn.newIndex - rtn.closeIndex;
            if (rtn.closeIndex != 0.0m)
                rtn.changePercent = rtn.change / rtn.closeIndex;


            return rtn;
        }



        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.StaticInfo.IndexInfo securityInfo)
        {
            quotInfo.stkId = securityInfo.SecurityID;
            quotInfo.stkName = securityInfo.Symbol;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.StaticInfo.StockSecurityInfo securityInfo)
        {
            quotInfo.stkId = securityInfo.CommonInfo.SecurityID;
            quotInfo.stkName = securityInfo.CommonInfo.Symbol;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.RealtimeStatus status)
        {
            quotInfo.stkIdPrefix = status.Status.SecurityPreName;
            if (!string.IsNullOrEmpty(status.Status.SecurityPreName))
            {
                quotInfo.closeFlag = status.Status.SecurityPreName.FirstOrDefault();

                if (status.Status.SecurityPreName.Length > 1)
                {
                    quotInfo.passCreditCashStk = status.Status.SecurityPreName.Skip(1).Take(1).First();
                    quotInfo.passCreditShareStk = quotInfo.passCreditCashStk;
                }
            }
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnap309011 quotSnap)
        {
            SetQuotInfoProperty(quotInfo, quotSnap.CommonInfo);
            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.NewIndex)
                    {
                        quotInfo.newIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.OpenIndex)
                    {
                        quotInfo.openIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.CloseIndex)
                    {
                        quotInfo.closeIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.HighIndex)
                    {
                        quotInfo.highIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo309011.MDEntryType.LowIndex)
                    {
                        quotInfo.lowIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                }
            }
            quotInfo.change = quotInfo.newIndex - quotInfo.closeIndex;
            if (quotInfo.closeIndex != 0.0m)
                quotInfo.changePercent = quotInfo.change / quotInfo.closeIndex;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnap300111 quotSnap)
        {
            SetQuotInfoProperty(quotInfo, quotSnap.CommonInfo);
            if (quotSnap.ExtInfo != null && quotSnap.ExtInfo.MDEntries != null && quotSnap.ExtInfo.MDEntries.Length > 0)
            {
                decimal d = 1000000;
                foreach (var mdEntry in quotSnap.ExtInfo.MDEntries)
                {
                    if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuyPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                            quotInfo.buyPrice1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                            quotInfo.buyPrice2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                            quotInfo.buyPrice3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                            quotInfo.buyPrice4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                            quotInfo.buyPrice5 = (decimal)mdEntry.Entry.MDEntryPx / d;

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.BuySummary)
                    {
                        //  quotInfo.openIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff1)
                    {
                        //  quotInfo.closeIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.Diff2)
                    {
                        //  quotInfo.highIndex = (decimal)mdEntry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.HighPrice)
                    {
                        quotInfo.highPrice = (decimal)mdEntry.Entry.MDEntryPx / d;

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.KnockPrice)
                    {
                        quotInfo.knockMoney = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }

                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.LowPrice)
                    {
                        quotInfo.lowPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.OpenPrice)
                    {
                        quotInfo.openPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                 
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellPrice)
                    {
                        if (mdEntry.Entry.MDPriceLevel == 1)
                            quotInfo.sellPrice1 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 2)
                            quotInfo.sellPrice2 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 3)
                            quotInfo.sellPrice3 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 4)
                            quotInfo.sellPrice4 = (decimal)mdEntry.Entry.MDEntryPx / d;
                        else if (mdEntry.Entry.MDPriceLevel == 5)
                            quotInfo.sellPrice5 = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.MaxOrderPrice)
                    {
                        quotInfo.maxOrderPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.MinOrderPrice)
                    {
                        quotInfo.minOrderPrice = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.IOPV)
                    {
                        quotInfo.IOPV = (decimal)mdEntry.Entry.MDEntryPx / d;
                    }

                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SellSummary)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.SettlePrice)
                    {

                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio1)
                    {
                        
                    }
                    else if (mdEntry.Entry.MDEntryType == QuotV5.Binary.QuotSnapExtInfo300111.MDEntryType.PriceEarningRatio2)
                    {
                        
                    }
                }
            }

            quotInfo.change = quotInfo.newPrice - quotInfo.closePrice;
            if (quotInfo.closePrice != 0.0m)
                quotInfo.changePercent = quotInfo.change / quotInfo.closePrice;
        }

        private void SetQuotInfoProperty(QuotationInfo quotInfo, QuotV5.Binary.QuotSnapCommonInfo quotSnapCommonInfo)
        {
            quotInfo.knockQty = quotSnapCommonInfo.TotalVolumeTrade;
            quotInfo.knockMoney = quotSnapCommonInfo.TotalValueTrade;
            quotInfo.tradeTimeFlag = quotSnapCommonInfo.TradingPhaseCode;
            quotInfo.closePrice = (decimal)quotSnapCommonInfo.PrevClosePx / 10000;
            quotInfo.changeTime = quotSnapCommonInfo.OrigTime;
        }

    
    }


}
