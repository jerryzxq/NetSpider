using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;

namespace Vcredit.WeiXin.RestService.Job
{
    class DeleteVercodeImageJob : IJob
    {


        public void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string dir = dataMap.GetString("dir");

            if (Directory.Exists(dir)) //如果存在这个文件夹删除之 
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    //获取文件名
                    var strArr = d.Split('\\');
                    if (strArr.Length > 0)
                    {
                        strArr = strArr[strArr.Length-1].Split('.');
                    }
                    //判断此文件名对应key，是否在缓存中
                    if (strArr.Length == 2)
                    {
                        var cache = Common.CacheHelper.GetCache(strArr[0]);
                        if (cache != null)
                        {
                            continue;
                        }
                    }
                    if (File.Exists(d))
                        File.Delete(d); //直接删除其中的文件                        
                }
            }
            String strData = dataMap.GetString("key0");
        }
    }
}
