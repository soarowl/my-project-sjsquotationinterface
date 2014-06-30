using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceHost.Core;

namespace MDS.Plugin.StockV5
{
    /// <summary>
    /// 插件上下文类
    /// </summary>
    internal class PluginContext : ContextBase
    {
		/// <summary>
		/// 
		/// </summary>
		static PluginContext Instance;

		/// <summary>
		/// 静态方法，获得模块上下文类实例
		/// </summary>
		/// <returns>模块上下文类实例</returns>
		public static new PluginContext GetContext()
		{
			if (Instance == null)
				Instance = new PluginContext();

			return Instance;
		}
    }
}
