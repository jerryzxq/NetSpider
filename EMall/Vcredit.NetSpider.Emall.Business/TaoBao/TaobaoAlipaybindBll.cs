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
	
	public class TaobaoAlipaybindBll : Business<TaobaoAlipaybindEntity, SqlConnectionFactory>
	{
        public static readonly TaobaoAlipaybindBll Initialize = new TaobaoAlipaybindBll();
        TaobaoAlipaybindBll() { }

	    public TaobaoAlipaybindEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoAlipaybindEntity> List()
        {
            return Select();
        }

        public List<TaobaoAlipaybindEntity> List(SqlExpression<TaobaoAlipaybindEntity> expression)
        {
            return Select(expression);
        }

	}
}
