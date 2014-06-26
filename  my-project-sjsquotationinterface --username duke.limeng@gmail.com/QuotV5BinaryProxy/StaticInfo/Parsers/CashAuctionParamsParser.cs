using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QuotV5.StaticInfo
{
    public class CashAuctionParamsParser : ParserBase
    {

        public List<CashAuctionParams> Parse(string xmlContent)
        {
            XDocument xDoc = XDocument.Parse(xmlContent, LoadOptions.None);
            var paramsNodes = xDoc.Descendants("AuctionParams").First().Descendants("Security");
            var count = paramsNodes.Count();
            if (count > 0)
            {
                List<CashAuctionParams> rtn = new List<CashAuctionParams>(count);
                foreach (var securityInfoNode in paramsNodes)
                {
                    CashAuctionParams cashAcutionParas = ParseNode<CashAuctionParams>(securityInfoNode);
                    rtn.Add(cashAcutionParas);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }

        protected override bool ParseSpecialPropery<T>(T obj, System.Reflection.PropertyInfo property, XElement node)
        {
            if (property.PropertyType == typeof(List<PriceLimitSetting>))
            {
                var priceLimitSettingRootNode = node.Descendants("PriceLimitSetting").FirstOrDefault();
                if (priceLimitSettingRootNode != null)
                {
                    List<PriceLimitSetting> settings = new List<PriceLimitSetting>();
                    foreach (var settingNode in priceLimitSettingRootNode.Descendants("Setting"))
                    {
                        PriceLimitSetting setting = ParseNode<PriceLimitSetting>(settingNode);
                        settings.Add(setting);
                    }
                    FastReflection<T>.SetPropertyValue<List<PriceLimitSetting>>(obj, property, settings);
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
