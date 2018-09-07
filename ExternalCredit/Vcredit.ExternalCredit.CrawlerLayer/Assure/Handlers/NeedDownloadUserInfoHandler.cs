using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Common;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.Handlers
{
    public class NeedDownloadUserInfoHandler : BaseCreditQueryHandler
    {
        private CRD_CD_CreditUserInfoBusiness _creditUserInfoBiz = new CRD_CD_CreditUserInfoBusiness();

        /// <summary>
        /// 每次数据库取的数量
        /// </summary>
        private static int _PerReportCount
        {
            get
            {
                var preportCount = 0;
                var defaultCount = 35;
                try
                {
                    preportCount = ConfigurationManager.AppSettings["PreReportCount"].ToInt(defaultCount);
                }
                catch (Exception)
                {
                    preportCount = defaultCount;
                }
                Log4netAdapter.WriteInfo(string.Format("每次取出 {0} 条解析", preportCount));
                return preportCount;
            }
        }

        protected override HandleResponse Handle(HandleRequest request)
        {
            var res = new HandleResponse();
            res.Data = GetNeedDownloadUserInfo();

            if (res.Data != null && res.Data.Count > 0)
                res.DoNextHandler = true;
            else
                res.Description = "没有查询到待解析的用户！";

            return res;
        }

        protected override void SetNextRequest(HandleRequest request, HandleResponse res)
        {
            request.UserInfoList = res.Data as List<CRD_CD_CreditUserInfoEntity>;
        }

        /// <summary>
        /// 获取等待下载的数据
        /// </summary>
        /// <returns></returns>
        private List<CRD_CD_CreditUserInfoEntity> GetNeedDownloadUserInfo()
        {
            var entities = _creditUserInfoBiz.GetNeedDownload(_PerReportCount);

            Console.WriteLine(string.Format("查询到：{0} 条等待下载的数据", entities.Count));
            Log4netAdapter.WriteInfo(string.Format("查询到：{0} 条等待下载的数据", entities.Count));
            return entities;
        }
    }
}
