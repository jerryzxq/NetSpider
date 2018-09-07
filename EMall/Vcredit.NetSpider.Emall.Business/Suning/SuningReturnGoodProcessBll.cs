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
	
	public class SuningReturnGoodProcessBll : Business<SuningReturnGoodProcessEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningReturnGoodProcessBll Initialize = new SuningReturnGoodProcessBll();
        SuningReturnGoodProcessBll() { }

	    public SuningReturnGoodProcessEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningReturnGoodProcessEntity> List()
        {
            return Select();
        }

        public List<SuningReturnGoodProcessEntity> List(SqlExpression<SuningReturnGoodProcessEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(SuningReturnGoodProcessEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(SuningReturnGoodsOrderEntity snReturnOrder, List<SuningReturnGoodProcessEntity> snOrderGoods)
        {
            if (snOrderGoods == null || snOrderGoods.Count == 0) return;
            snOrderGoods.ForEach(e => e.ReturnID = snReturnOrder.ID);

            base.Save(snOrderGoods);
        }
	}
}
