using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace Vcredit.NetSpider.Service
{
    // SocialSecurityEntity服务对象
    internal class SocialSecurityImpl : BaseService<SocialSecurityEntity>, ISocialSecurity
    {
        ISocialSecurityDetailDao detailDao;
        /// <summary>
        /// 重写Save方法
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Save(SocialSecurityEntity entity)
        {
            base.Save(entity);
            if (entity.Details != null)
            {
                foreach (var detail in entity.Details)
                {
                    detail.SocialSecurityId = entity.Id;
                    detailDao.Save(detail);
                }
            }
            return entity.Id;
        }

        public SocialSecurityEntity GetByBusiness(string BusId, string BusType)
        {
            var ls = base.FindListByHql("from SocialSecurityEntity where  BusId=? and BusType=? order by CreateTime desc", new object[] { BusId, BusType }, 1, 1);

            return ls.FirstOrDefault();
        }


        public SocialSecurityEntity GetByIdentityCard(string IdentityCard, string bustype = null, string city = null)
        {
            SocialSecurityEntity sse = new SocialSecurityEntity();
            sse = base.FindListByHql("from SocialSecurityEntity where  BusIdentityCard=?" + (city!=null?" and SocialSecurityCity=?":"") + (bustype != null?" and BusType=?":"") + " order by CreateTime desc", new object[] { IdentityCard, city, bustype }, 1, 1).FirstOrDefault();
            //if (string.IsNullOrEmpty(city))
            //{
            //    sse = base.FindListByHql("from SocialSecurityEntity where  BusIdentityCard=? order by CreateTime desc", new object[] { IdentityCard }, 1, 1).FirstOrDefault();
            //}
            //else
            //{
            //    sse = base.FindListByHql("from SocialSecurityEntity where  BusIdentityCard=? and SocialSecurityCity=? order by CreateTime desc", new object[] { IdentityCard, city }, 1, 1).FirstOrDefault();
            //}
            return sse;
            
        }

        public SocialSecurityEntity GetAllDataByIdentityCard(string IdentityCard, string bustype = null, string city = null)
        {
            var ls = base.FindListByHql("from SocialSecurityEntity where  BusIdentityCard=?" + (city != null ? " and SocialSecurityCity=?" : "") + (bustype != null ? " and BusType=?" : "") + " order by CreateTime desc", new object[] { IdentityCard, city, bustype }, 1, 1);

            var result = ls.FirstOrDefault();
            if (result != null)
            {
                var details = base.GetSession().CreateSQLQuery("select * from social.SocialSecurityDetail WITH(NOLOCK) where SocialSecurityId=" + result.Id).AddEntity("SocialSecurityDetailEntity", typeof(SocialSecurityDetailEntity)).List<SocialSecurityDetailEntity>();
                result.Details = details;
            }
            return result;
        }
    }
}

