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

    public class AlipayBillOrderBll : Business<AlipayBillOrderEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillOrderBll Initialize = new AlipayBillOrderBll();
        AlipayBillOrderBll() { }

        public AlipayBillOrderEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillOrderEntity> List()
        {
            return Select();
        }

        public List<AlipayBillOrderEntity> List(SqlExpression<AlipayBillOrderEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillOrderEntity item)
        {
            return base.Save(item);
        }
        public void ActionSave(AlipayBillEntity bill, AlipayBillOrderEntity entity)
        {
            if (entity == null) return;
            entity.BillID = bill.ID;
            if (base.Save(entity))
            {
                entity.SaveAction<AlipayBillOrderDetailEntity>(entity.BillOrderDetail, AlipayBillOrderDetailBll.Initialize.ActionSave);
                entity.SaveAction<AlipayBillOrderLogisticsEntity>(entity.BillOrderLogistics, AlipayBillOrderLogisticsBll.Initialize.ActionSave);
            }
        }
        public void ActionSave(AlipayBillEntity bill, List<AlipayBillOrderEntity> list)
        {
            if (list == null || list.Count == 0) return;
            list.ForEach(e => ActionSave(bill, e));

        }


    }

    public static class AlipayBillOrderExt
    {

        public static void SaveAction<T>(this AlipayBillOrderEntity baic, List<T> list, Action<AlipayBillOrderEntity, List<T>> action)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderExt--ActionSave", ex);

            }
        }
        public static void SaveAction<T>(this AlipayBillOrderEntity baic, T list, Action<AlipayBillOrderEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderExt--ActionSave", ex);

            }
        }

    }
}
