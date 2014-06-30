using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Log4cb;

namespace MDS.Plugin.StockV5
{
    public class StaticInfoProvider<TStaticInfo>
    {
        public event Action<List<TStaticInfo>> OnStaticInfoRead;

        private StaticInfoProviderConfig config;
        private Log4cb.ILog4cbHelper logHelper;

        /// <summary>
        /// 扫描线程
        /// </summary>
        private Thread scanThread = null;

        /// <summary>
        /// 停止事件
        /// </summary>
        private ManualResetEvent stopEvent = new ManualResetEvent(false);


        public StaticInfoProvider(StaticInfoProviderConfig config, Log4cb.ILog4cbHelper logHelper)
        {
            this.config = config;
            this.logHelper = logHelper;
        }

        /// <summary>
        /// 启动数据提供服务
        /// </summary>
        public void Start()
        {
            this.logHelper.LogInfoMsg("开始启动{0}文件的Provider", GetFileFullPath());
            this.stopEvent.Reset();

            // 检查是否已经启动
            if (this.scanThread != null && this.scanThread.IsAlive)
                return;

            ///创建并启动文件读取线程
            this.scanThread = new Thread(new ThreadStart(LoopScan));
            this.scanThread.Start();
            Thread.Sleep(0);
        }


        /// <summary>
        /// 停止数据提供服务
        /// </summary>
        public void Stop()
        {
            this.stopEvent.Set();
        }


        /// <summary>
        /// 扫描DBF文件
        /// </summary>
        protected abstract void OnScanData();
      


        /// <summary>
        /// 文件读取线程
        /// </summary>
        private void LoopScan()
        {
            string filePath = GetFileFullPath();
            this.logHelper.LogInfoMsg("启动{0}的Provider成功, 扫描时间间隔:{1}秒", filePath, this.config.ScanInterval.TotalSeconds);

            DateTime? lastStartTime = null;
            try
            {
                while (true)
                {
                    double waitMS = 0;
                    if (lastStartTime.HasValue)
                    {
                        TimeSpan sinceLastScan = DateTime.Now - lastStartTime.Value;
                        this.logHelper.LogPerformaceMsg(Log4cbType.LogFileOnly, "读取并处理{0}总耗时{1}ms", filePath, sinceLastScan.TotalMilliseconds);

                        waitMS = this.config.ScanInterval.TotalMilliseconds - sinceLastScan.TotalMilliseconds;
                        if (waitMS < 0)
                            waitMS = 0;
                    }
                    //队列为空，则等待新数据到达通知或中断事件
                    int index = AutoResetEvent.WaitAny(new WaitHandle[1] { stopEvent }, (int)waitMS, true);

                    //停止事件发生,退出发送线程
                    if (index == 0)
                    {
                        this.logHelper.LogInfoMsg("{0}的Provider正常退出", filePath);
                        break;
                    }
                    lastStartTime = DateTime.Now;
                    OnScanData();
                }
            }
            catch (Exception err)
            {
                this.logHelper.LogErrMsg(err, "{0}的Provider数据扫描线程异常退出", GetFileFullPath());
            }

            this.scanThread = null;
        }


        /// <summary>
        /// 取DBF文件全路径
        /// </summary>
        /// <returns>
        /// </returns>
        protected string GetFileFullPath()
        {
            DateTime now = DateTime.Now;
            return this.config.PathFormat.Replace("{YYYYMMDD}", now.ToString("yyyyMMdd"));

        }



        /// <summary>
        /// 触发OnStaticInfoRead事件
        /// </summary>
        /// <param name="records"></param>
        /// <param name="timestamp"></param>
        protected void RaiseStaticInfoReadEvent(List<TStaticInfo> records)
        {
            Action<List<TStaticInfo>> handler = this.OnStaticInfoRead;
            if (handler != null)
                handler(records);
        }

    }
    public class StaticInfoProviderConfig
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string PathFormat { get; set; }

        /// <summary>
        /// 扫描间隔
        /// </summary>
        public TimeSpan ScanInterval { get; set; }

    }

}
