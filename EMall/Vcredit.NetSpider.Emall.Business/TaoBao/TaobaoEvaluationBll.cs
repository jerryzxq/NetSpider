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
	
	public class TaobaoEvaluationBll : Business<TaobaoEvaluationEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoEvaluationBll Initialize = new TaobaoEvaluationBll();
        TaobaoEvaluationBll() { }

	    public TaobaoEvaluationEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoEvaluationEntity> List()
        {
            return Select();
        }

        public List<TaobaoEvaluationEntity> List(SqlExpression<TaobaoEvaluationEntity> expression)
        {
            return Select(expression);
        }

      
	}
}
