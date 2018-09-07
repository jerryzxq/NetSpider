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
	
	public class GomeCartBll : Business<GomeCartEntity, SqlConnectionFactory>
	{
        public static readonly GomeCartBll Initialize = new GomeCartBll();
        GomeCartBll() { }

	    public GomeCartEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeCartEntity> List()
        {
            return Select();
        }

        public List<GomeCartEntity> List(SqlExpression<GomeCartEntity> expression)
        {
            return Select(expression);
        }

        public List<GomeCartEntity> QueryList(string token)
        {
            var userInfo = GomeUserinfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<GomeCartEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.UserId == userInfo.Id);

            var data = Initialize.Select(sqlexp);
            return data;
        }
        
    }
}
