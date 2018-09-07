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
	
	public class AlipayExpSummaryBll : Business<AlipayExpSummaryEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly AlipayExpSummaryBll Initialize = new AlipayExpSummaryBll();
        AlipayExpSummaryBll() { }

	    public AlipayExpSummaryEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayExpSummaryEntity> List()
        {
            return Select();
        }

        public List<AlipayExpSummaryEntity> List(SqlExpression<AlipayExpSummaryEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayExpSummaryEntity item)
        {
            return base.Save(item);
        }

      
        public void ActionSave(AlipayElectronicBillEntity bill, AlipayExpSummaryEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.ElectronicBillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayExpSummaryBll--ActionSave", ex);

            }

        }
        public void ActionSave(AlipayElectronicBillEntity bill, List<AlipayExpSummaryEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.ElectronicBillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayExpSummaryBll--ActionSave", ex);

            }
        }

	}
}
