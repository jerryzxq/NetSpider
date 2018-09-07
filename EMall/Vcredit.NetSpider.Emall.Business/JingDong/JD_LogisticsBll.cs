using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_LogisticsBll : Business<LogisticsEntity, SqlConnectionFactory>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        public List<LogisticsEntity> GetByUserID(string account)
        {
            SqlExpression<LogisticsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == account);
            var data = this.Select(sqlexp);
            return data;
        }
    }
}
