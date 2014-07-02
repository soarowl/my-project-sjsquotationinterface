using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MDS.Plugin.StockV5
{


    public class SecurityInfoProvider : StaticInfoProvider<QuotV5.StaticInfo.SecurityInfoBase>
    {
        QuotV5.StaticInfo.SecurityInfoParser parser = new QuotV5.StaticInfo.SecurityInfoParser();
        protected override void OnScanData()
        {
            string filePath = GetFileFullPath();

            if (File.Exists(filePath))
            {
                try
                {
                    string fileContent = ReadAllText(filePath, encoding);

                    if (fileContent == this.lastScanFileContent)
                        return;

                    List<QuotV5.StaticInfo.SecurityInfoBase> securityInfos = parser.Parse(fileContent);

                    RaiseStaticInfoReadEvent(securityInfos);

                    this.lastScanFileContent = fileContent;
                }
                catch (Exception ex)
                {
                    this.logHelper.LogErrMsg(ex, "读取并解析静态信息文件异常，FilePath={0}",filePath);
                }
            }
            else
            {
                this.logHelper.LogInfoMsg("文件不存在：{0}", filePath);
            }
        }
    }

}
