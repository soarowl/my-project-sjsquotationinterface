using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MDS.Plugin.StockV5
{
    class QuotationCenter
    {
        private Log4cb.ILog4cbHelper logHelper;

        public QuotationCenter(Log4cb.ILog4cbHelper logHelper)
        {

            this.logHelper = logHelper;
        }

        internal void Stop()
        {
            throw new NotImplementedException();
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }
    }
}
