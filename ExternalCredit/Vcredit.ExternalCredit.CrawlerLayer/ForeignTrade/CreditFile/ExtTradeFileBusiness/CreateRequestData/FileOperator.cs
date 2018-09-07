using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public  class FileOperator
    {
        internal void CreateNotExistDirectory(string directroyPath)
        {
           if(!Directory.Exists(directroyPath))
           {
               Directory.CreateDirectory(directroyPath);
           }
           
        }
        /// <summary>
        /// 判断是否是放在第二天的文件夹
        /// </summary>
        /// <returns></returns>
        public static bool CheckIsNextToday(DateTime dtime)
        {
            bool isNext = false;
            double hourPoint = 20;
            if (double.TryParse(ConfigData.todayDealingPoint, out  hourPoint))
            {
                if (dtime.Hour+dtime.Minute/60.00 >= hourPoint)
                {
                    isNext=true;
                }
            }
            return isNext;
        }
        internal string GetDirectoryName(string path="",bool isLimitedByTime =false)
        {
            string DirectoryName = string.Empty;
            DateTime dtime = DateTime.Now;
            if (isLimitedByTime&&CheckIsNextToday(dtime))
            {
                DirectoryName = dtime.AddDays(1).ToString("yyyyMMdd");
            }
            else
            {
                DirectoryName = dtime.ToString("yyyyMMdd");
            }
            return path+DirectoryName;
        }

        internal string GetTxtFileName(int bathNo,string path,string directoryName)
        {
            string batchNoStr = string.Empty;
            if(bathNo>0&&bathNo<10)
            {
                batchNoStr = "0" + bathNo.ToString();
            }
            else
            {
                batchNoStr = bathNo.ToString();
            }
            return path + directoryName + "/" + directoryName + "_" + batchNoStr + ".txt";

        }
        internal Tuple<string, bool> CheckIsExistFile(string DatedirectoryName, string fileName,bool isFindAll=false)
        {
            bool result = true;
            string newFilePath = "";
            string fullFileName= DatedirectoryName + "\\" + fileName;
            result = File.Exists(fullFileName);
            if (!isFindAll)
                return new Tuple<string, bool>(fullFileName, result);
            if(!result)//如果指定的目录不存在就，在上一级目录递归查找
            {
                string[] regexFileArray = Directory.GetFiles(ConfigData.RequestedFileSavePath, fileName, SearchOption.AllDirectories);
                if (regexFileArray.Length == 0)
                {
                    result = false;
                }
                else
                {
                    newFilePath = regexFileArray[0];
                    result = true;
                }
            }
            else
            {
                newFilePath = fullFileName;
            }
            return new Tuple<string,bool>(newFilePath,result);
        }

    }
}
