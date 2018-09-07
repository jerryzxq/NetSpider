using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;

namespace Vcredit.NetSpider.Processor
{
    public interface ISociaSecurityExecutor
    {
        /// <summary>
        /// 社保登录页面初始化
        /// </summary>
        /// <returns></returns>
        VerCodeRes Init(string cityCode);
        /// <summary>
        /// 获取社保缴费数据
        /// </summary>
        /// <param name="city">公积金所在城市</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="prams">其他参数</param>
        /// <returns></returns>
        SocialSecurityQueryRes GetSocialSecurity(string cityCode, SocialSecurityReq socialReq);
    }
}
