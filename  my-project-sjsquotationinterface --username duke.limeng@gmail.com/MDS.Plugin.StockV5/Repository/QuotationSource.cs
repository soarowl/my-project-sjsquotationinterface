using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceManager.Services;
using System.Diagnostics;

namespace MDS.Plugin.SZQuotV5
{
    /// <summary>
    /// 行情源
    /// </summary>
    [DebuggerNonUserCode]
    public class QuotationSource
    {
      
        /// <summary>
        /// 
        /// </summary>
        string Key;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        internal QuotationSource(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static implicit operator string(QuotationSource source)
        {
            return source.Key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static implicit operator QuotationSource(string key)
        {
            if (string.IsNullOrEmpty(key) || key.Length != 3)
                return null;

            return new QuotationSource(key.Substring(0, 3));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return string.IsNullOrEmpty(Key) ? 0 : Key.GetHashCode();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is string)
                return this == (QuotationSource)(string)obj;

            return this == obj as QuotationSource;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs1"></param>
        /// <param name="qs2"></param>
        /// <returns></returns>
        public static bool operator ==(QuotationSource qs1, QuotationSource qs2)
        {
            if (ReferenceEquals(qs1, qs2))
                return true;

            if ((object)qs1 == null || (object)qs2 == null)
                return false;

            return string.Compare(qs1.Key, qs2.Key, true) == 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="qs1"></param>
        /// <param name="qs2"></param>
        /// <returns></returns>
        public static bool operator !=(QuotationSource qs1, QuotationSource qs2)
        {
            return !(qs1 == qs2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsStockQuotation()
        {
            return this == StockQuotationSource.Level1 || this == StockQuotationSource.Level2;
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Key;
        }
    }

    /// <summary>
    /// 现货行情源
    /// </summary>
    public sealed class StockQuotationSource : QuotationSource
    {
        /// <summary>
        /// Level1行情
        /// </summary>
        public static readonly StockQuotationSource Level1 = new StockQuotationSource(QuotServiceNames.Level1);

        /// <summary>
        /// Level2行情
        /// </summary>
        public static readonly StockQuotationSource Level2 = new StockQuotationSource(QuotServiceNames.Level2);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        StockQuotationSource(string source)
            : base(source)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override bool IsStockQuotation()
        {
            return true;
        }
    }

    /// <summary>
    /// 期货行情源
    /// </summary>
    [DebuggerNonUserCode]
    public sealed class FutureQuotationSource : QuotationSource
    {
        /// <summary>
        /// FFuture行情
        /// </summary>
        public static readonly FutureQuotationSource FFuture = new FutureQuotationSource(QuotServiceNames.FFuture);

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        FutureQuotationSource(string source)
            : base(source)
        {
        }

    }


}
