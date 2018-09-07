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
    public class NeedCommitUserInfoHandler : BaseCreditQueryHandler
    {
        private CRD_CD_CreditUserInfoBusiness _creditUserInfoBiz = new CRD_CD_CreditUserInfoBusiness();

        /// <summary>
        /// 每次数据库取的数量
        /// </summary>
        private int _PerReportCount = 0;

        public NeedCommitUserInfoHandler()
        {
            _PerReportCount = this.GetPerReportCount();
        }

        private int GetPerReportCount()
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
            Log4netAdapter.WriteInfo(string.Format("每次取出 {0} 条上报查询", preportCount));
            return preportCount;
        }

        protected override HandleResponse Handle(HandleRequest request)
        {
            var res = new HandleResponse();
            res.Data = GetNeedQueryUserInfo();

            if (res.Data != null && res.Data.Count > 0)
                res.DoNextHandler = true;
            else
                res.Description = "没有查询到待提交的用户！";

            return res;
        }

        protected override void SetNextRequest(HandleRequest request, HandleResponse res)
        {
            request.UserInfoList = res.Data as List<CRD_CD_CreditUserInfoEntity>;
        }

        /// <summary>
        /// 获取待查询数据
        /// </summary>
        /// <returns></returns>
        private List<CRD_CD_CreditUserInfoEntity> GetNeedQueryUserInfo()
        {
            List<CRD_CD_CreditUserInfoEntity> userInfoList = null;
            try
            {
                userInfoList = this.QueryData();
                // 修改为数据上传中状态更改
                foreach (var uinfo in userInfoList)
                {
                    uinfo.State = (int)RequestState.UpLoading;
                    uinfo.Error_Reason = "提交进程正在提交数据...";
                    uinfo.UpdateTime = DateTime.Now;
                    _creditUserInfoBiz.UpdateEntity(uinfo);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("获取待提交数据出现异常"));
                Log4netAdapter.WriteError(string.Format("获取待提交数据出现异常"), ex);

                throw;
            }

            Console.WriteLine(string.Format("查询到：{0} 条等待提交的数据", userInfoList.Count));
            Log4netAdapter.WriteInfo(string.Format("查询到：{0} 条等待提交的数据", userInfoList.Count));

            return userInfoList;
        }

        /// <summary>
        /// 数据过滤，保证多个job取到的数据不重复
        /// </summary>
        /// <param name="userInfoList"></param>
        /// <returns></returns>
        private List<CRD_CD_CreditUserInfoEntity> QueryData()
        {
            var jobIndex = ConfigurationManager.AppSettings["jobIndex"].ToInt();
            var jobCount = ConfigurationManager.AppSettings["jobCount"].ToInt();

            if (jobIndex == null || jobCount == null)
                throw new ArgumentException("appsettings必须配置参数jobIndex、jobCount");

            if (jobIndex >= jobCount)
                throw new ArgumentException("当前jobIndex必须小于jobCount");

            // 当天连接失败的信息
            var connectionfailedList = _creditUserInfoBiz.GetConnectionFailedUsersByJobConfig(_PerReportCount, jobCount, jobIndex);
            // 等待提交的信息
            var userInfoList = _creditUserInfoBiz.GetUsersByJobConfig(_PerReportCount, jobCount, jobIndex);

            // 连接失败的用户与待提交的合并，取得配置的条数，优先连接失败的用户
            var needCommitUserInfoList = new List<CRD_CD_CreditUserInfoEntity>();
            needCommitUserInfoList.AddRange(connectionfailedList);
            needCommitUserInfoList.AddRange(userInfoList);
            needCommitUserInfoList = needCommitUserInfoList.Take(_PerReportCount).ToList();

            return needCommitUserInfoList;
        }
    }
}
