//using System;
//using System.Text;
//using System.Linq;
//using System.Runtime.InteropServices;
//namespace QuotV5
//{
//    public class ObjectToString_System_String
//    {
//        public static string ObjectToString(System.String obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[System_String]");
//            sb.AppendLine(string.Format("Chars={0}", obj.Chars));
//            sb.AppendLine(string.Format("Length={0}", obj.Length));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapCommonInfo
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapCommonInfo obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapCommonInfo]");
//            sb.AppendLine(string.Format("OrigTime={0}", obj.OrigTime));
//            sb.AppendLine(string.Format("ChannelNo={0}", obj.ChannelNo));
//            sb.AppendLine(string.Format("MDStreamID={0}", obj.MDStreamID));
//            sb.AppendLine(string.Format("SecurityID={0}", obj.SecurityID));
//            sb.AppendLine(string.Format("SecurityIDSource={0}", obj.SecurityIDSource));
//            sb.AppendLine(string.Format("TradingPhaseCode={0}", obj.TradingPhaseCode));
//            sb.AppendLine(string.Format("PrevClosePx={0}", obj.PrevClosePx));
//            sb.AppendLine(string.Format("NumTrades={0}", obj.NumTrades));
//            sb.AppendLine(string.Format("TotalVolumeTrade={0}", obj.TotalVolumeTrade));
//            sb.AppendLine(string.Format("TotalValueTrade={0}", obj.TotalValueTrade));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntryWithoutOrders
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111.MDEntryWithoutOrders obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111__MDEntryWithoutOrders]");
//            sb.AppendLine(string.Format("MDEntryType={0}", obj.MDEntryType));
//            sb.AppendLine(string.Format("MDEntryPx={0}", obj.MDEntryPx));
//            sb.AppendLine(string.Format("MDEntrySize={0}", obj.MDEntrySize));
//            sb.AppendLine(string.Format("MDPriceLevel={0}", obj.MDPriceLevel));
//            sb.AppendLine(string.Format("Unknow={0}", obj.Unknow));
//            sb.AppendLine(string.Format("NumberOfOrders={0}", obj.NumberOfOrders));
//            sb.AppendLine(string.Format("NoOrders={0}", obj.NoOrders));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__Order
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111.Order obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111__Order]");
//            sb.AppendLine(string.Format("OrderQty={0}", obj.OrderQty));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__Order_Array
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111.Order[] obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111__Order_Array]");
//            if (obj != null)
//            {
//                foreach (var item in obj)
//                {
//                    sb.AppendLine(ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__Order.ObjectToString(item));
//                }
//            }
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntry
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111.MDEntry obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111__MDEntry]");
//            sb.AppendLine(string.Format("Entry={0}", ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntryWithoutOrders.ObjectToString(obj.Entry)));
//            sb.AppendLine(string.Format("Orders={0}", ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__Order_Array.ObjectToString(obj.Orders)));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntry_Array
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111.MDEntry[] obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111__MDEntry_Array]");
//            if (obj != null)
//            {
//                foreach (var item in obj)
//                {
//                    sb.AppendLine(ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntry.ObjectToString(item));
//                }
//            }
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnapExtInfo300111 obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnapExtInfo300111]");
//            sb.AppendLine(string.Format("NoMDEntries={0}", obj.NoMDEntries));
//            sb.AppendLine(string.Format("MDEntries={0}", ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111__MDEntry_Array.ObjectToString(obj.MDEntries)));
//            return sb.ToString();
//        }
//    }
//    public class ObjectToString_QuotV5_Binary_QuotSnap300111
//    {
//        public static string ObjectToString(QuotV5.Binary.QuotSnap300111 obj)
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.AppendLine("[QuotV5_Binary_QuotSnap300111]");
//            sb.AppendLine(string.Format("CommonInfo={0}", ObjectToString_QuotV5_Binary_QuotSnapCommonInfo.ObjectToString(obj.CommonInfo)));
//            sb.AppendLine(string.Format("ExtInfo={0}", ObjectToString_QuotV5_Binary_QuotSnapExtInfo300111.ObjectToString(obj.ExtInfo)));
//            return sb.ToString();
//        }
//    }
//}
