using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
namespace Vcredit.NetSpider.Emall.Business
{

    public class AlipayBillOrderLogisticsBll : Business<AlipayBillOrderLogisticsEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillOrderLogisticsBll Initialize = new AlipayBillOrderLogisticsBll();
        AlipayBillOrderLogisticsBll() { }

        public AlipayBillOrderLogisticsEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillOrderLogisticsEntity> List()
        {
            return Select();
        }

        public List<AlipayBillOrderLogisticsEntity> List(SqlExpression<AlipayBillOrderLogisticsEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillOrderLogisticsEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(AlipayBillOrderEntity bill, AlipayBillOrderLogisticsEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillOrderID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderLogisticsBll--ActionSave", ex);
            }

        }
        public void ActionSave(AlipayBillOrderEntity bill, List<AlipayBillOrderLogisticsEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.BillOrderID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderLogisticsBll--ActionSave", ex);
            }
        }


    }
}
