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

    public class AlipayBillRefundsBll : Business<AlipayBillRefundsEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillRefundsBll Initialize = new AlipayBillRefundsBll();
        AlipayBillRefundsBll() { }

        public AlipayBillRefundsEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillRefundsEntity> List()
        {
            return Select();
        }

        public List<AlipayBillRefundsEntity> List(SqlExpression<AlipayBillRefundsEntity> expression)
        {
            return Select(expression);
        }
        public void ActionSave(AlipayBillEntity bill, AlipayBillRefundsEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillRefundsBll--ActionSave", ex);
            }
        }
        public void ActionSave(AlipayBillEntity bill, List<AlipayBillRefundsEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.BillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillRefundsBll--ActionSave", ex);
            }
        }
        public override bool Save(AlipayBillRefundsEntity item)
        {
            return base.Save(item);
        }



    }
}
