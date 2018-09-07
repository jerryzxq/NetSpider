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
	
	public class SuningLogisticsDetailBll : Business<SuningLogisticsDetailEntity, SqlConnectionFactory>
	{


        public static readonly SuningLogisticsDetailBll Initialize = new SuningLogisticsDetailBll();
        SuningLogisticsDetailBll() { }

        public SuningLogisticsDetailEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningLogisticsDetailEntity> List()
        {
            return Select();
        }

        public List<SuningLogisticsDetailEntity> List(SqlExpression<SuningLogisticsDetailEntity> expression)
        {
            return Select(expression);
        }

        public List<SuningLogisticsDetailEntity> ListByOrderId(long orderId)
        {
            return Select(e=>e.OrderID == orderId);
        }

        public override bool Save(SuningLogisticsDetailEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(SuningOrderEntity snOrder, List<SuningLogisticsDetailEntity> snLogistics)
        {
            if (snLogistics == null || snLogistics.Count == 0) return;
            snLogistics.ForEach(e => e.OrderID = snOrder.ID);

            base.Save(snLogistics);
        }
	}
}
