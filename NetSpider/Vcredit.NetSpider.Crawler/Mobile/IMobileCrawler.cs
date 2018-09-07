using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.Mobile;

namespace Vcredit.NetSpider.Crawler.Mobile
{
    public interface IMobileCrawler
    {
        /// <summary>
        /// 初始化页面
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes MobileInit(MobileReq mobileReq);

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        BaseRes MobileLogin(MobileReq mobileReq);

        /// <summary>
        /// 发送短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        VerCodeRes MobileSendSms(MobileReq mobileReq);

        /// <summary>
        /// 校验短信验证码
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <returns></returns>
        BaseRes MobileCheckSms(MobileReq mobileReq);

        /// <summary>
        /// 手机抓取
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="appDate"></param>
        /// <returns></returns>
        BaseRes MobileCrawler(MobileReq mobileReq, DateTime appDate);

        /// <summary>
        /// 手机解析
        /// </summary>
        /// <param name="mobileReq"></param>
        /// <param name="appDate"></param>
        /// <returns></returns>
        BaseRes MobileAnalysis(MobileReq mobileReq, DateTime appDate);

    }
}
