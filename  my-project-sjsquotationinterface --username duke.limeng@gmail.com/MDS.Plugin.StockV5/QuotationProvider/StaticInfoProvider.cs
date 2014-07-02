using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Log4cb;

namespace MDS.Plugin.StockV5
{
    public abstract class StaticInfoProvider<TStaticInfo>
    {
        public event Action<List<TStaticInfo>> OnStaticInfoRead;
        protected  static Encoding encoding = Encoding.UTF8;
        protected StaticInfoProviderConfig config;
        protected Log4cb.ILog4cbHelper logHelper;
        protected string lastScanFileContent;

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
           
            this.logHelper.LogInfoMsg("启动{0}的Provider成功, 扫描时间间隔:{1}秒", this.config.PathFormat, this.config.ScanInterval.TotalSeconds);

            DateTime? lastStartTime = null;
            try
            {
                while (true)
                {
                    double waitMS = 0;
                    if (lastStartTime.HasValue)
                    {
                        TimeSpan sinceLastScan = DateTime.Now - lastStartTime.Value;
                        this.logHelper.LogPerformaceMsg(Log4cbType.LogFileOnly, "读取并处理{0}总耗时{1}ms", this.config.PathFormat,, sinceLastScan.TotalMilliseconds);

                        waitMS = this.config.ScanInterval.TotalMilliseconds - sinceLastScan.TotalMilliseconds;
                        if (waitMS < 0)
                            waitMS = 0;
                    }
                    //队列为空，则等待新数据到达通知或中断事件
                    int index = AutoResetEvent.WaitAny(new WaitHandle[1] { stopEvent }, (int)waitMS, true);

                    //停止事件发生,退出发送线程
                    if (index == 0)
                    {
                        this.logHelper.LogInfoMsg("{0}的Provider正常退出", this.config.PathFormat,);
                        break;
                    }
                    lastStartTime = DateTime.Now;
                    OnScanData();
                }
            }
            catch (Exception err)
            {
                this.logHelper.LogErrMsg(err, "{0}的Provider数据扫描线程异常退出", this.config.PathFormat);
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

        protected static string ReadAllText(string fileFullPath, Encoding encoding)
        {
            byte[] fileBytes = ReadAllBytes(fileFullPath);
            if (fileBytes == null)
                return null;
            else
                return encoding.GetString(fileBytes);
        }

        protected static byte[] ReadAllBytes(string fileFullPath)
        {
            using (FileStream fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (!fileStream.CanRead)
                {
                    throw new Exception("文件流不可读");
                }
                else if (fileStream.Length <= 0)
                {
                    return null;
                }
                else
                {
                    byte[] fileBuffer = null;
                    int nRead = 0;
                    fileBuffer = new byte[fileStream.Length];
                    nRead = fileStream.Read(fileBuffer, 0, (int)fileStream.Length);
                    if (nRead == fileStream.Length)
                    {
                        return fileBuffer;
                    }
                    else
                    {
                        throw new Exception("未能完整读取文件流");
                    }
                }
            }

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
