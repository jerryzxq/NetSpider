using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Dto.TaoBao;

namespace Vcredit.NetSpider.Emall.Business.TaoBao
{

    public class TaobaoLogisticsBll : Business<TaobaoLogisticsEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoLogisticsBll Initialize = new TaobaoLogisticsBll();
        TaobaoLogisticsBll() { }

        public TaobaoLogisticsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoLogisticsEntity> List()
        {
            return Select();
        }

        public List<TaobaoLogisticsEntity> List(SqlExpression<TaobaoLogisticsEntity> expression)
        {
            return Select(expression);
        }

        /// <summary>
        /// 物流数据查询
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<TaobaoLogisticsEntity> QueryList(string token)
        {
            var userInfo = TaobaoUserInfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<TaobaoLogisticsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userInfo.Account);

            var data = Initialize.Select(sqlexp);
            return data;
        }
    }
}
