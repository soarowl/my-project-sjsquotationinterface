using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ServiceHost.Core;
using ServiceHost;
using System.IO;

namespace MDS.Plugin.SZQuotV5
{
    public class Configuration : ConfigNode
    {
        /// <summary>
        /// 配置文件文档对象
        /// </summary>
        private Document doc;

        public Configuration(string configPath, PluginConfigInfo cfg)
        {
            doc = new Document();

            //配置文件全路径
            ConfigPath = Path.GetFullPath(configPath);

            //如果存在配置文件，则直接打开
            if (File.Exists(ConfigPath))
            {
                doc.Load(ConfigPath);
            }
            else
            {
                //如果不存在配置文件，则打开模板配置文件
                doc.Load(configPath);
            }

            //加载配置文件信息
            LoadConfigInfo(doc.RootNode, "MDS.Plugin.SZQuotV5");
        }

        /// <summary>
        /// 配置文件路径
        /// </summary>
        [NotAttribute]
        public string ConfigPath { get; set; }


        public StaticInfoFiles StaticInfoFiles { get; set; }

        public BinaryConnections BinaryConnections { get; set; }

        public ActiveMQ ActiveMQ { get; set; }
    }
    public class StaticInfoFiles : ConfigNode
    {
        public List<StaticInfoFile> Files { get; set; }
    }
    public class StaticInfoFile : ConfigNode
    {
        public string Key { get; set; }
        public string Path { get; set; }
        public string ScanInterval { get; set; }
    }

    public class BinaryConnections : ConfigNode
    {
        public List<BinaryConnection> Connections { get; set; }
    }

    public class BinaryConnection : ConfigNode
    {
        public string Key { get; set; }
        public string IP { get; set; }
        public Int32 Port { get; set; }
        public string TargetCompID { get; set; }
        public string SenderCompID { get; set; }
        public string Password { get; set; }
        public Int32 HeartbeatIntervalS { get; set; }
        public Int32 ConnectionTimeoutMS { get; set; }
        public Int32 ReconnectIntervalMS { get; set; }
    }

    public class ActiveMQ : ConfigNode
    {
        public string Address { get; set; }
        public int RecordsPerPackage { get; set; }
    }


}
