using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // CallsEntity服务对象接口
    public interface ICalls : IBaseService<CallsEntity> 
    {
        /// <summary>
        /// 根据身份证号与手机号，通过通话记录计算最长使用月份
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        //int GetUseMonthsByIdentityNoAndMobile(string IdentityNo, string Mobile);

        /// <summary>
        /// 根据身份证号与手机号，通过通话记录计算最长使用月份
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        IList<CallsEntity> GetListOneMonth(string IdentityNo, string Mobile);

        /// <summary>
        /// 获取全部通话清单根据Oprid
        /// </summary>
        /// <param name="opr_id"></param>
        /// <returns></returns>
        IList<CallsEntity> GetCallListByOprid(int opr_id);
    }
}

