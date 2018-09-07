using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Util;
using Vcredit.Framework.Server.Service;
using Vcredit.NetSpider.Entity.DB;

namespace Vcredit.NetSpider.Service
{
    internal class Mobile_NumberImpl : BaseService<Mobile_NumberEntity>, IMobile_Number
    {
        public Mobile_NumberEntity GetModel(string mobile)
        {
            var entity = new Mobile_NumberEntity();
            try
            {
                entity = base.Find("FROM Mobile_NumberEntity WHERE Mobile=?", new object[] { mobile }).FirstOrNull() as Mobile_NumberEntity;
            }
            catch (Exception)
            { }
            return entity;
        }
    }
}
