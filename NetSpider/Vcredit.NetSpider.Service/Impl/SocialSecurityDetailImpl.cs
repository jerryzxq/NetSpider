using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;


namespace Vcredit.NetSpider.Service
{
    // SocialSecurityDetailEntity服务对象
    internal class SocialSecurityDetailImpl : BaseService<SocialSecurityDetailEntity>, ISocialSecurityDetail
    {

        public IList<SocialSecurityDetailEntity> GetDetailListBySocialSecurityId(int id)
        {
            IList<SocialSecurityDetailEntity> entityList = new List<SocialSecurityDetailEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from social.SocialSecurityDetail WITH(NOLOCK) where SocialSecurityId=" + id).AddEntity("SocialSecurityDetailEntity", typeof(SocialSecurityDetailEntity)).List<SocialSecurityDetailEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}

