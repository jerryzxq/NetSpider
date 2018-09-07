using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;

namespace Vcredit.NetSpider.Processor
{
    public interface IProvidentFundExecutor
    {
        /// <summary>
        /// 公积金登录页面初始化
        /// </summary>
        /// <returns></returns>
        VerCodeRes Init(string cityCode);
        /// <summary>
        /// 获取公积金缴费数据
        /// </summary>
        /// <param name="token">会话token</param>
        /// <param name="city">公积金所在城市</param>
        /// <param name="username">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="vercode">验证码</param>
        /// <param name="prams"></param>
        /// <returns></returns>
        ProvidentFundQueryRes GetProvidentFund(string cityCode, ProvidentFundReq fundReq);
    }
}
