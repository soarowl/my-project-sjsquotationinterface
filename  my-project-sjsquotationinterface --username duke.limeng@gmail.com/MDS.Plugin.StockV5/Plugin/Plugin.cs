using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceHost.Core;
using Log4cb;
using ServiceManager.Services;


namespace MDS.Plugin.StockV5
{
	[ServiceHost.PluginName("StockV5")]
    public class Plugin : IPlugin
    {
        
        private static MDS.Plugin.StockV5.QuotationCenter  quotCenter;


        /// <summary>
        /// 当前插件提供的应用服务名称
        /// </summary>
        public static readonly string ServiceName = "CORE-SERVICES:MDSSTOCKV5";
        /// 主/备冗余服务对象
        /// 用于控制插件同一时间只能有一个实例在运行
        /// </summary>
        MasterSlaveService MSService = null;



        #region 属性及变量

        /// <summary>
        /// Plugin模块的上下文实例
        /// </summary>
        PluginContext Context = null;

        /// <summary>
        /// 插件名称
        /// 由RS2加载时，根据配置文件中的名称填写
        /// </summary>
        public string Name { get; set; }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public Plugin()
        {
            Context = PluginContext.GetContext();
            Context.Plugin = this;
        }

        #region IPlugin 成员

        /// <summary>
        /// 状态变化的回调处理函数设置
        /// </summary>
        /// <param name="callback">回调处理</param>
        public void SetStatusChanged(PluginStatusChanged callback)
        {
        }

        /// <summary>
        /// 初始化插件
        /// </summary>
        /// <param name="cfg">插件配置文件实例</param>
        /// <param name="logHelper"></param>
        /// <returns>成功则返回true，否则返回false</returns>
        public bool Initialize(PluginConfigInfo cfg, ILog4cbHelper logHelper)
        {
            try
            {
                Context.PluginInfo = cfg;
                Context.LogHelper = logHelper;

                //创建一个主备冗余服务对象
                MSService = DirectoryService.GetMasterSlaveService(ServiceName,false);
                MSService.ModeChanged += new EventHandler(MSService_ModeChanged);
                MSService.CheckStatus += new Func<MasterSlaveService, bool>(MSService_CheckStatus);


                quotCenter = new MDS.Plugin.StockV5.QuotationCenter (Context.LogHelper);
                return true;
            }
            catch (Exception ex)
            {
                Context.LogHelper.LogErrMsg(ex, "初始化失败");
                return false;
            }
        }



        /// <summary>
        /// 卸载插件
        /// </summary>
        /// <returns>成功则返回true，否则返回false</returns>
        public bool UnInitialize()
        {
            try
            {
                MSService.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Context.LogHelper.LogErrMsg(ex, "卸载失败");
                return false;
            }
        }

        /// <summary>
        /// 启动插件
        /// </summary>
        /// <returns>成功则返回true，否则返回false</returns>
        public bool Start()
        {
            MSService.StartAutoCheck();
            return true;
        }

        /// <summary>
        /// 停止插件
        /// </summary>
        /// <returns>成功则返回true，否则返回false</returns>
        public bool Stop()
        {
            MSService.StopAutoCheck();
            return true;
        }



        /// <summary>
        /// 状态检查回调函数
        /// 需要检查当前的MDS实例是否工作正常
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        bool MSService_CheckStatus(MasterSlaveService arg)
        {
            return true;
        }

        /// <summary>
        /// Master/Slave角色身份发生变化时的处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void MSService_ModeChanged(object sender, EventArgs e)
        {
            if (MSService.Mode == MasterSlaveMode.Master)
            {
                this.StartBusiness();
            }
            else
            {
                this.StopBusiness();
            }
        }
        /// <summary>
        /// 业务逻辑启动
        /// </summary>
        /// <returns></returns>
        private bool StartBusiness()
        {
            //启动自动队列Queue
            try
            {
                quotCenter.Start();
                return true;
            }
            catch (Exception err)
            {
                Context.LogHelper.LogErrMsg(err, "启动失败");
                return false;
            }
        }

        /// <summary>
        /// 业务逻辑停止
        /// </summary>
        /// <returns></returns>
        private bool StopBusiness()
        {
            try
            {
                quotCenter.Stop();
                return true;
            }
            catch (Exception err)
            {
                Context.LogHelper.LogErrMsg(err, "停止失败");
                return false;
            }
        }


        #endregion
    }
}
