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
	
	public class AlipayBillOrderDetailBll : Business<AlipayBillOrderDetailEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly AlipayBillOrderDetailBll Initialize = new AlipayBillOrderDetailBll();
        AlipayBillOrderDetailBll() { }

	    public AlipayBillOrderDetailEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillOrderDetailEntity> List()
        {
            return Select();
        }

        public List<AlipayBillOrderDetailEntity> List(SqlExpression<AlipayBillOrderDetailEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillOrderDetailEntity item)
        {
            return base.Save(item);
        }

        public void ActionSave(AlipayBillOrderEntity bill, AlipayBillOrderDetailEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.OrderID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderDetailBll--ActionSave", ex);

            }

        }
        public void ActionSave(AlipayBillOrderEntity bill, List<AlipayBillOrderDetailEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.OrderID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillOrderDetailBll--ActionSave", ex);

            }
        }
      

	}
}
