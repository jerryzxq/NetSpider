using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class spd_applyformBll : Business<spd_applyformEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly spd_applyformBll Initialize = new spd_applyformBll();

        spd_applyformBll() { }
	    public spd_applyformEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<spd_applyformEntity> List()
        {
            return Select();
        }

        public List<spd_applyformEntity> List(SqlExpression<spd_applyformEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(spd_applyformEntity item)
        {
            return base.Save(item);
        }


	}
}
