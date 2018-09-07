using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Chsi_InfoEntity服务对象接口
    public interface IChsi_Info : IBaseService<Chsi_InfoEntity>
    {
        /// <summary>
        /// 根据身份证号,查询学历最新信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <returns></returns>
        Chsi_InfoEntity GetByIdentityCard(string IdentityCard);
        /// <summary>
        /// 根据身份证号,查询学信网所有学历信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <returns></returns>
        IList<Chsi_InfoEntity> GetListByIdentityCard(string IdentityCard);

        IList<Chsi_InfoEntity> GetListByToken(string Token);
    }
}

