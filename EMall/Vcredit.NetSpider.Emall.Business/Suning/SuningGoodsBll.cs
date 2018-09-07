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
	
	public class SuningGoodsBll : Business<SuningGoodsEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningGoodsBll Initialize = new SuningGoodsBll();
        SuningGoodsBll() { }

	    public SuningGoodsEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningGoodsEntity> List()
        {
            return Select();
        }

        public List<SuningGoodsEntity> List(SqlExpression<SuningGoodsEntity> expression)
        {
            return Select(expression);
        }
        public List<SuningGoodsEntity> ListByOrderNo(long orderNo)
        {
            return Select(e=>e.OrderID == orderNo);
        }
        public override bool Save(SuningGoodsEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(SuningOrderEntity snOrder, List<SuningGoodsEntity> snOrderGoods)
        {
            if (snOrderGoods == null || snOrderGoods.Count == 0) return;
            snOrderGoods.ForEach(e => {
                e.OrderID = snOrder.ID;
                e.UserId = snOrder.UserId;
                e.AccountName = snOrder.AccountName;
            });
            base.Save(snOrderGoods);
        }
	}
}
