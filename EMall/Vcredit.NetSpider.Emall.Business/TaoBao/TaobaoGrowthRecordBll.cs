using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.TaoBao
{
	
	public class TaobaoGrowthRecordBll : Business<TaobaoGrowthRecordEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoGrowthRecordBll Initialize = new TaobaoGrowthRecordBll();
        TaobaoGrowthRecordBll() { }

	    public TaobaoGrowthRecordEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoGrowthRecordEntity> List()
        {
            return Select();
        }

        public List<TaobaoGrowthRecordEntity> List(SqlExpression<TaobaoGrowthRecordEntity> expression)
        {
            return Select(expression);
        }

	}
}
