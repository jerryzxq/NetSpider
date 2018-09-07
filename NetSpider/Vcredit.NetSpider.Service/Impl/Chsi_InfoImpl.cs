using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // Chsi_InfoEntity服务对象
    internal class Chsi_InfoImpl : BaseService<Chsi_InfoEntity>, IChsi_Info
    {
        /// <summary>
        /// 根据身份证号,查询学信网最新信息
        /// </summary>
        /// <param name="IdentityCard">身份证号</param>
        /// <returns></returns>
        public IList<Chsi_InfoEntity> GetListByIdentityCard(string IdentityCard)
        {
            IList<Chsi_InfoEntity> ls = null;

            ls = base.Find("from Chsi_InfoEntity where IdentityCard=?  order by Id desc", new object[] { IdentityCard });
            if (ls.Count > 0)
            {
                string token = ls.FirstOrDefault().Token;
                ls = ls.Where(o=>o.Token==token).ToList();
            }
            
            return ls;
        }


        public IList<Chsi_InfoEntity> GetListByToken(string Token)
        {
            var ls = base.Find("from Chsi_InfoEntity where  Token=? ", new object[] { Token });

            return ls;
        }

        public Chsi_InfoEntity GetByIdentityCard(string IdentityCard)
        {
            IList<Chsi_InfoEntity> ls = null;

            ls = base.FindListByHql("from Chsi_InfoEntity where IdentityCard=?  order by Id desc", new object[] { IdentityCard },1,1);
            return ls.FirstOrDefault();
        }
    }
}

