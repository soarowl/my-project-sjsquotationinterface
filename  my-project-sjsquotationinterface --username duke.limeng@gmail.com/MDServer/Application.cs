using System;
using System.IO;
using MDServer.Config;
using Microsoft.Win32;
using ServiceHost;
using ServiceManager.Utils;

namespace MDServer
{
	internal class Application : ServiceHost.Application
	{
		public Application(string appName)
			: base(appName)
		{
            //初始化计数器
            MDServerCounter.CreatePerformanceCounters();
		}
        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
		protected override bool Initialize()
		{
			string configPath = "";
			//配置文件的默认位置在当前运行目录下
            configPath = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, string.Format("{0}{1}", this.Name, ConfigSuffix));

			try
			{
				//加载配置文件
				var cfg = new Configuration(configPath);

                GlobalContext.DBConnectionCfg = cfg.DBConnection;
                GlobalContext.QuotationCache = cfg.QuotationCache;
				GlobalContext.ThreadPoolCfg = cfg.ThreadPool;
                GlobalContext.TradeDays = cfg.TradeDays;

			}
			catch (Exception err)
			{
				Console.WriteLine("加载配置文件出错：" + err.Message);
				return false;
			}

			try
			{
				return base.Initialize();
			}
			catch (Exception)
			{
				throw;
			}
		}

	}
}
