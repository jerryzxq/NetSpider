using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // SummaryEntity服务对象接口
    public interface ISummary : IBaseService<SummaryEntity> 
    {
        /// <summary>
        /// 根据Bid,查询Summary
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        SummaryEntity GetByBid(int bid);
        /// <summary>
        /// 根据业务信息,查询账单统计
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        SummaryEntity GetByBusiness(string BusId, string BusType);
        /// <summary>
        /// 根据身份证号,查询账单统计
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        SummaryEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile, string BusType=null);
        /// <summary>
        /// 根据OperId，查询账单统计
        /// </summary>
        /// <param name="OperId"></param>
        /// <returns></returns>
        SummaryEntity GetByOperId(int OperId);
        /// <summary>
        /// 根据手机号，查询账单统计
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        SummaryEntity GetByMobile(string Mobile);

        /// <summary>
        /// 修改手机实名认证
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <param name="IsAuth">认证状态</param>
        /// <returns></returns>
        bool UpdateRealNameAuth(string IdentityNo, string Mobile, int IsAuth = 1);
        /// <summary>
        /// 获取手机已使用月份
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        int GetUseMonths(string IdentityNo, string Mobile);
    }
}

