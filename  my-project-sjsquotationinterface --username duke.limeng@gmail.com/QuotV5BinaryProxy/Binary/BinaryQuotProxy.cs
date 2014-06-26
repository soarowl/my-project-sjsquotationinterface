using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace QuotV5.Binary
{
    public class BinaryQuotProxy
    {

        private BinaryQuotProxyConfig config;
        private Log4cb.ILog4cbHelper logHelper;
        private object startStopLockObj = new object();
       
        /// <summary>
        /// 停止
        /// </summary>
        private ManualResetEvent stopEvent = new ManualResetEvent(false);


        public BinaryQuotProxy(BinaryQuotProxyConfig config,Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;
        }


        public void Start()
        {
            lock (this.startStopLockObj)
            {
                if (this.CurrentStatus == Status.Started)
                    return;
            

                this.logHelper.LogInfoMsg("BinaryProxy Starting");
                this.CurrentStatus = Status.Starting;
                this.stopEvent.Reset();




                this.CurrentStatus = Status.Started;
                this.logHelper.LogInfoMsg("BinaryProxy Started");
            }
        }
        public void Stop()
        { 
        
        }





        /// <summary>
        /// 当前状态
        /// </summary>
        public Status CurrentStatus { get; private set; }

        /// <summary>
        /// 表示当前状态
        /// </summary>
        public enum Status
        {
            Starting,
            Started,
            Stopping,
            Stopped
        }
    }
}
