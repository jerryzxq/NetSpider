using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace  Vcredit.NetSpider.Service
{
    // BasicEntity服务对象接口
    public interface IBasic : IBaseService<BasicEntity> 
    {
        /// <summary>
        /// 根据手机和身份证，查询基本信息
        /// </summary>
        /// <param name="IdentityNo"></param>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        BasicEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile);

        /// <summary>
        /// 根据oper_id查询基本信息
        /// </summary>
        /// <param name="IdentityNo"></param>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        BasicEntity GetByOperid(int oper_id);
    }
}

