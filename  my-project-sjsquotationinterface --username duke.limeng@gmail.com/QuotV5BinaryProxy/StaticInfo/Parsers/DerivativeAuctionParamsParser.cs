using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace QuotV5.StaticInfo
{
    public class DerivativeAuctionParamsParser : ParserBase
    {

        public List<DerivativeAuctionParams> Parse(string xmlContent)
        {
            XDocument xDoc = XDocument.Parse(xmlContent, LoadOptions.None);
            var paramsNodes = xDoc.Descendants("AuctionParams").First().Descendants("Security");
            var count = paramsNodes.Count();
            if (count > 0)
            {
                List<DerivativeAuctionParams> rtn = new List<DerivativeAuctionParams>(count);
                foreach (var securityInfoNode in paramsNodes)
                {
                    DerivativeAuctionParams cashAcutionParas = ParseNode<DerivativeAuctionParams>(securityInfoNode);
                    rtn.Add(cashAcutionParas);
                }
                return rtn;
            }
            else
            {
                return null;
            }
        }


        protected override bool ParseSpecialPropery<T>(T obj, System.Reflection.PropertyInfo property, System.Xml.Linq.XElement node)
        {
            return false;
        }
    }
}
