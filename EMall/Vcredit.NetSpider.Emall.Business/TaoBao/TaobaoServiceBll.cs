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

    public class TaobaoServiceBll : Business<TaobaoServiceEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoServiceBll Initialize = new TaobaoServiceBll();
        TaobaoServiceBll() { }

        public TaobaoServiceEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoServiceEntity> List()
        {
            return Select();
        }

        public List<TaobaoServiceEntity> List(SqlExpression<TaobaoServiceEntity> expression)
        {
            return Select(expression);
        }

      
    }
}
