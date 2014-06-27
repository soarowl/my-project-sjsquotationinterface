using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QuotV5.StaticInfo
{
    public class SecurityInfoParser : ParserBase
    {

        public  List<SecurityInfoBase> Parse(string xmlContent)
        {
            XDocument xDoc = XDocument.Parse(xmlContent, LoadOptions.None);
            var securityInfoNodes = xDoc.Descendants("Securities").First().Descendants("Security");
            var count = securityInfoNodes.Count();
            if (count > 0)
            {
                List<SecurityInfoBase> rtn = new List<SecurityInfoBase>(count);
                foreach (var securityInfoNode in securityInfoNodes)
                {
                    SecurityInfoBase securityInfo = Parse(securityInfoNode);

                    rtn.Add(securityInfo);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }

        private  SecurityInfoBase Parse(XContainer securityInfoNode)
        {
            SecurityInfoBase rtn = null;
            CommonSecurityInfo commonInfo = ParseNode<CommonSecurityInfo>(securityInfoNode);


            switch (commonInfo.SecurityType)
            {
                case SecurityType.主板A股:
                case SecurityType.中小板股票:
                case SecurityType.创业板股票:
                case SecurityType.主板B股:
                    var stockParaNode = securityInfoNode.Descendants("StockParams").First();
                    StockParams stockPara = ParseNode<StockParams>(stockParaNode);
                    rtn = new StockSecurityInfo() { CommonInfo = commonInfo, Params = stockPara };
                    break;


                case SecurityType.国债:
                case SecurityType.企业债:
                case SecurityType.公司债:
                case SecurityType.可转债:
                case SecurityType.中小企业可交换私募债:
                case SecurityType.中小企业私募债:
                case SecurityType.证券公司次级债:
                    var bondParaNode = securityInfoNode.Descendants("BondParams").First();
                    BondParams bondPara = ParseNode<BondParams>(bondParaNode);
                    rtn = new BondSecurityInfo() { CommonInfo = commonInfo, Params = bondPara };
                    break;

                case SecurityType.质押式回购:
                    var repoParaNode = securityInfoNode.Descendants("RepoParams").First();
                    RepoParams repoPara = ParseNode<RepoParams>(repoParaNode);
                    rtn = new RepoSecurityInfo() { CommonInfo = commonInfo, Params = repoPara };
                    break;

                case SecurityType.资产支持证券:
                    rtn = new OtherSecurityInfo() { CommonInfo = commonInfo };
                    break;

                case SecurityType.本市场股票ETF:
                case SecurityType.跨市场股票ETF:
                case SecurityType.跨境ETF:
                case SecurityType.标准LOF:
                case SecurityType.本市场事务债券ETF:
                case SecurityType.现金债券ETF:
                case SecurityType.黄金ETF:
                case SecurityType.货币ETF:
                case SecurityType.分级子基金:
                case SecurityType.封闭式基金:
                case SecurityType.仅申赎基金:
                case SecurityType.仅实时申赎货币基金:
                    var fundParaNode = securityInfoNode.Descendants("FundParams").First();
                    FundParams fundPara = ParseNode<FundParams>(fundParaNode);
                    rtn = new FundSecurityInfo() { CommonInfo = commonInfo, Params = fundPara };
                    break;

                case SecurityType.权证:
                    var warrantParaNode = securityInfoNode.Descendants("WarrantParams").First();
                    WarrantParams warrantPara = ParseNode<WarrantParams>(warrantParaNode);
                    rtn = new WarrantSecurityInfo() { CommonInfo = commonInfo, Params = warrantPara };
                    break;

                case SecurityType.股票期权:
                case SecurityType.ETF期权:
                    var optionParaNode = securityInfoNode.Descendants("OptionParams").First();
                    OptionParams optionPara = ParseNode<OptionParams>(optionParaNode);
                    rtn = new OptionSecuirtyInfo() { CommonInfo = commonInfo, Params = optionPara };
                    break;
                default:
                    break;
            }

            return rtn;
        }

    }
}
