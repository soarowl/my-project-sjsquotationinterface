using System.ComponentModel;
using System.ServiceProcess;
using ServiceHost.Service;

namespace MDServer
{
    /// <summary>
    /// 服务安装
    /// </summary>
    [RunInstaller(true)]
    public class ServiceInstaller : AppServiceInstaller
    {
        /// <summary>
        /// 服务的名称
        /// </summary>
        internal static readonly string ServiceName = "MDServer";

        /// <summary>
        /// 服务安装
        /// </summary>
        public ServiceInstaller()
            : base(new System.ServiceProcess.ServiceInstaller
            {
                ServiceName = ServiceInstaller.ServiceName,
                DisplayName = "CROOTWAY MARKET DATA SERVER",
                StartType = ServiceStartMode.Automatic,
                Description = "融维天成实时行情服务",
            })
        {
        }
    }
}
