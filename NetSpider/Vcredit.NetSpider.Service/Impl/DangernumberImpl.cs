using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    internal class DangernumberImpl : BaseService<DangernumberEntity>, IDangernumber
    {
        public IList<DangernumberEntity> Get()
        {
            IList<DangernumberEntity> entityList = new List<DangernumberEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from Dangernumber WITH(NOLOCK) ").AddEntity("DangernumberEntity", typeof(DangernumberEntity)).List<DangernumberEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
        public IList<DangernumberEntity> GetByIsOwn(int isOwn)
        {
            IList<DangernumberEntity> entityList = new List<DangernumberEntity>();
            try
            {
                entityList = base.GetSession().CreateSQLQuery("select * from Dangernumber WITH(NOLOCK) where IsOwn=" + isOwn).AddEntity("DangernumberEntity", typeof(DangernumberEntity)).List<DangernumberEntity>();
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}
