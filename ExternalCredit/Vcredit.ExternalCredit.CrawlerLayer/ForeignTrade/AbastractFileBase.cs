using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public abstract class AbastractFileBase
    {
      
        public  abstract bool SaveData();

        protected  List<FileInfo> GetLocalFileInfo(params string[] suffixs)
        {
            List<FileInfo> fileInfoList = new List<FileInfo>();
            List<string> list = GetDateDirectroy();
            foreach(var item in  list)
            {
                DirectoryInfo di = new DirectoryInfo(ConfigData.localFileSaveDir+item);
                foreach (string suffix in suffixs)
                {
                    var fileInfos = di.GetFiles("*" + suffix);
                    if (fileInfos != null && fileInfos.Length != 0)
                    {
                        fileInfoList.AddRange(fileInfos);
                    }
                }
            }
            return fileInfoList;
        }

        protected List<string> GetDateDirectroy()
        {
            List<string> list = new List<string>();
            var directoryarray = GetLocalReadDateDirectroy();
            if (directoryarray == null)//如果没有配置，给默认的日期文件夹
            {
                list.Add(new FileOperator().GetDirectoryName());
            }
            else
            {
                list.AddRange(directoryarray);
            }
            return list;
        }


    
        string[] GetLocalReadDateDirectroy()
        {
            if (string.IsNullOrEmpty(ConfigData.LocalReadDateDirectroy))
                return null;
            return ConfigData.LocalReadDateDirectroy.Split(',');

        }
    }
}
