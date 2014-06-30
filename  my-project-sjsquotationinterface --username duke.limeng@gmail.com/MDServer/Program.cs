using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using ServiceHost.Service;
using System.Diagnostics;

namespace MDServer
{
    public class Program : AppServiceBase
    {
        Application Application;

		/// <summary>
		/// 
		/// </summary>
		public Program()
			: base(ServiceInstaller.ServiceName)
		{
			Application = new Application(ServiceInstaller.ServiceName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="appName"></param>
		public Program(string appName)
			: base(string.IsNullOrWhiteSpace(appName) ? ServiceInstaller.ServiceName : appName)
		{
			Application = new Application(string.IsNullOrWhiteSpace(appName) ? ServiceInstaller.ServiceName : appName);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="args"></param>
		protected override void OnStart(string[] args)
		{
			Application.Start(args);
		}

        protected override void OnStop()
        {
            Application.Stop();
        }

        protected override void OnShutdown()
        {
            Application.Stop();
        }

        [MTAThread]
        static void Main(string[] args)
        {
			//当前进程名称
			string appName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            //如果是Unix/Linux系统(暂且认为是MONO平台)
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
				new Program(appName).OnStart(args);
            }

            //如果是Windows系统
            else if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (Environment.UserInteractive)
                {
					////////如果服务不存在，则按照服务
					//////var service = ServiceInstaller.GetService(ServiceInstaller.ServiceName);
					//////if (service == null)
					//////{
					//////    try
					//////    {
					//////        Console.WriteLine("正在安装服务 ......");

					//////        AssemblyInstaller installer = new AssemblyInstaller(Assembly.GetEntryAssembly(), new string[] { "Install", "/LogToConsole=false" });
					//////        installer.Install(null);

					//////        Console.WriteLine("安装服务成功");
					//////    }
					//////    catch (Exception err)
					//////    {
					//////        Console.WriteLine("安装服务{0}失败，原因：{1}", ServiceInstaller.ServiceName, err.Message);
					//////        Thread.Sleep(2000);
					//////    }
					//////}
					////////如果服务正在运行，则进程退出
					//////else if (service.Status == ServiceControllerStatus.Running)
					//////{
					//////    Console.WriteLine("服务正在运行中，此进程即将退出！");
					//////    Thread.Sleep(2000);
					//////    Environment.Exit(0);
					//////}

					new Program(appName).OnStart(args);
                }
                else
                {
					System.ServiceProcess.ServiceBase.Run(new Program(appName));
                }
            }
        }
    }
}
