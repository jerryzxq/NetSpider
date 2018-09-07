using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Processor
{
    public interface IMobileExecutor
    {
        #region 初始化

        /// <summary>
        /// 移动话费账单登录
        /// </summary>
        /// <param name="mobile">电话号码</param>
        /// <returns></returns>
        VerCodeRes MobileInit(string mobile);
        VerCodeRes MobileInit(MobileReq mobileReq);

        #endregion

        #region 登录

        BaseRes MobileLogin(MobileReq mobileReq);

        #endregion

        #region 发送和校验短信验证码

        VerCodeRes MobileSendSms(MobileReq mobileReq);
        BaseRes MobileCheckSms(MobileReq mobileReq);

        #endregion

        #region 解析

        /// <summary>
        /// 解析抓取的数据
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        BaseRes MobileAnalysis(MobileReq mobileReq, DateTime crawlerDate);

        #endregion

        #region 重置密码

        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes ResetInit(MobileReq mobileReq);

        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes ResetSendSms(MobileReq mobileReq);

        /// <summary>
        /// 重置密码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        BaseRes ResetPassWord(MobileReq mobileReq);

        #endregion

        #region 查询

        Basic MobileQuery(MobileReq mobileReq);

        Variable_mobile_summaryEntity MobileVariableSummary(MobileReq mobileReq);

        Summary MobileSummaryQuery(MobileReq mobileReq);

        BaseRes MobileCatName(string mobile);

        List<Call> MobileCall(MobileReq mobileReq);

        /// <summary>
        /// 获取采集状态
        /// </summary>
        /// <param name="token">Token</param>
        /// <returns></returns>
        BaseRes GetCrawlerState(MobileReq mobileReq);

        /// <summary>
        /// 获取采集记录
        /// </summary>
        /// <param name="dic"></param>
        /// <returns></returns>
        List<Spd_applyEntity> GetCollectRecords(Dictionary<string, string> dic);


        #region 根据sourceid获取
        BaseRes GetCrawlerState(string source);

        Variable_mobile_summaryEntity MobileVariableSummary(string source);

        Basic MobileQuery(string source);

        List<Call> MobileCall(string source);

        #endregion

        #endregion
    }
}
