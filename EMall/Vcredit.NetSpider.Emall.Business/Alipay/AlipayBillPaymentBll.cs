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

    public class AlipayBillPaymentBll : Business<AlipayBillPaymentEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillPaymentBll Initialize = new AlipayBillPaymentBll();
        AlipayBillPaymentBll() { }

        public AlipayBillPaymentEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillPaymentEntity> List()
        {
            return Select();
        }

        public List<AlipayBillPaymentEntity> List(SqlExpression<AlipayBillPaymentEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillPaymentEntity item)
        {
            return base.Save(item);
        }
        public void ActionSave(AlipayBillEntity bill, AlipayBillPaymentEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillPaymentBll--ActionSave", ex);
            }
        }
        public void ActionSave(AlipayBillEntity bill, List<AlipayBillPaymentEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.BillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillPaymentBll--ActionSave", ex);
            }
        }


    }
}
