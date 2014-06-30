using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceHost.Core;
using ServiceHost.Config;

namespace MDServer.Config
{
    /// <summary>
    /// 配置信息
    /// </summary>
    internal class Configuration : ConfigNode
    {
        private Document doc;

        public string Version { get; set; }

        /// <summary>
        /// 线程池配置
        /// </summary>
		public ThreadPool ThreadPool { get; set; }

        /// <summary>
        /// 数据库连接配置
        /// </summary>
        public DBConnection DBConnection { get; set; }

        /// <summary>
        /// 行情缓存配置
        /// </summary>
        public QuotationCache QuotationCache { get; set; }

        /// <summary>
        /// 交易日相关配置
        /// </summary>
        public TradeDays TradeDays { get; set; }

		/// <summary>
		/// 命名空间
		/// </summary>
        public string Namespace { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        public Configuration(string configPath)
        {
            doc = new Document();
            doc.Load(configPath);

            LoadConfigInfo(doc.RootNode, "");
        }
        /// <summary>
        /// 保存配置信息
        /// </summary>
        public void Save()
        {
            UpdateNodeInfo(doc.RootNode);
            doc.Save();
        }

    }
}
