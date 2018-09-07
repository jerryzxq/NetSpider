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

    public class AlipayBillCreditPaymentBll : Business<AlipayBillCreditPaymentEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillCreditPaymentBll Initialize = new AlipayBillCreditPaymentBll();
        AlipayBillCreditPaymentBll() { }

        public AlipayBillCreditPaymentEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillCreditPaymentEntity> List()
        {
            return Select();
        }

        public List<AlipayBillCreditPaymentEntity> List(SqlExpression<AlipayBillCreditPaymentEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillCreditPaymentEntity item)
        {
            return base.Save(item);
        }
        public void ActionSave(AlipayBillEntity bill, List<AlipayBillCreditPaymentEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.BillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillCreditPaymentBll--ActionSave", ex);

            }
        }
        public void ActionSave(AlipayBillEntity bill, AlipayBillCreditPaymentEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillCreditPaymentBll--ActionSave", ex);

            }
        }


    }
}
