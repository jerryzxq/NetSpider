using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace Vcredit.NetSpider.Service
{
    // ProvidentFundEntity服务对象
    internal class ProvidentFundImpl : BaseService<ProvidentFundEntity>, IProvidentFund
    {
        IProvidentFundDetailDao detailDao;
        IProvidentFundReserveDao ReserveDao;
        IProvidentFundLoanDao LoanDao;
        /// <summary>
        /// 重写Save方法
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Save(ProvidentFundEntity entity)
        {
            base.Save(entity);
            if (entity.ProvidentFundDetailList != null)
            {
                foreach (var detail in entity.ProvidentFundDetailList)
                {
                    detail.ProvidentFundId = entity.Id;
                    detailDao.Save(detail);
                }
            }

            if (entity.ProvidentFundReserveRes != null)
            {
                entity.ProvidentFundReserveRes.ProvidentFundId = entity.Id;
                IProvidentFundReserve ProvidentFundReserveService = NetSpiderFactoryManager.GetProvidentFundReserveService();
                ProvidentFundReserveService.Save(entity.ProvidentFundReserveRes);
            }

            if (entity.ProvidentFundLoanRes != null)
            {
                entity.ProvidentFundLoanRes.ProvidentFundId = entity.Id;
                IProvidentFundLoan ProvidentFundLoanService = NetSpiderFactoryManager.GetProvidentFundLoanService();
                ProvidentFundLoanService.Save(entity.ProvidentFundLoanRes);
            }

            return entity.Id;
        }
        /// <summary>
        /// 根据业务信息,查询公积金数据
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        public ProvidentFundEntity GetByBusiness(string BusId, string BusType)
        {
            var ls = base.FindListByHql("from ProvidentFundEntity where BusId=? and BusType=? order by CreateTime desc", new object[] { BusId, BusType }, 1, 1);

            return ls.FirstOrDefault();
        }


        public ProvidentFundEntity GetByIdentityCard(string IdentityCard, string bustype = null, string city = null)
        {
            ProvidentFundEntity pfe = new ProvidentFundEntity();
            pfe = base.FindListByHql("from ProvidentFundEntity where BusIdentityCard=?" + (city != null ? " and ProvidentFundCity=?":"") + (bustype!=null ? " and BusType=?":"") + " order by CreateTime desc", new object[] { IdentityCard, city, bustype }, 1, 1).FirstOrDefault();
            //if (string.IsNullOrEmpty(city))
            //{
            //    pfe = base.FindListByHql("from ProvidentFundEntity where BusIdentityCard=? order by CreateTime desc", new object[] { IdentityCard }, 1, 1).FirstOrDefault();
            //}
            //else
            //{
            //    pfe = base.FindListByHql("from ProvidentFundEntity where BusIdentityCard=? and ProvidentFundCity=? order by CreateTime desc", new object[] { IdentityCard, city }, 1, 1).FirstOrDefault();
            //}
            return pfe;
        }


        public ProvidentFundEntity GetAllDataByIdentityCard(string IdentityCard, string bustype = null, string city = null)
        {
            var ls = base.FindListByHql("from ProvidentFundEntity where BusIdentityCard=?" + (city != null ? " and ProvidentFundCity=?" : "") + (bustype != null ? " and BusType=?" : "") + " order by CreateTime desc", new object[] { IdentityCard, city, bustype }, 1, 1);

            var result = ls.FirstOrDefault();
            if (result != null)
            {
                var details = base.GetSession().CreateSQLQuery("select * from provident.ProvidentFundDetail WITH(NOLOCK) where ProvidentFundId=" + result.Id).AddEntity("ProvidentFundDetailEntity", typeof(ProvidentFundDetailEntity)).List<ProvidentFundDetailEntity>();
                result.ProvidentFundDetailList = details;
            }
            return result;
        }
    }
}

