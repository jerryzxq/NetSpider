using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // ProvidentFundEntity服务对象接口
    public interface IProvidentFund : IBaseService<ProvidentFundEntity>
    {
        /// <summary>
        /// 根据业务信息,查询公积金数据
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        ProvidentFundEntity GetByBusiness(string BusId, string BusType);
        /// <summary>
        /// 根据身份证号,查询公积金数据
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <returns></returns>
        ProvidentFundEntity GetByIdentityCard(string IdentityCard, string bustype = null, string city = null);
        /// <summary>
        /// 根据身份证号,查询公积金数据包含明细
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <returns></returns>
        ProvidentFundEntity GetAllDataByIdentityCard(string IdentityCard, string bustype = null, string city = null);
    }
}

