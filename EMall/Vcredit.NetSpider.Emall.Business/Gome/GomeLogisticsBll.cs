using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Gome;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.Gome
{
	
	public class GomeLogisticsBll : Business<GomeLogisticsEntity, SqlConnectionFactory>
	{
        public static readonly GomeLogisticsBll Initialize = new GomeLogisticsBll();
        GomeLogisticsBll() { }

	    public GomeLogisticsEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeLogisticsEntity> List()
        {
            return Select();
        }

        public List<GomeLogisticsEntity> List(SqlExpression<GomeLogisticsEntity> expression)
        {
            return Select(expression);
        }

        public List<GomeLogisticsEntity> QueryList(string token)
        {
            var userInfo = GomeUserinfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<GomeLogisticsEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userInfo.Account);

            var data = Initialize.Select(sqlexp);
            return data;
        }
        
    }
}
