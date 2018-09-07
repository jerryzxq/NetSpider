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

    public class AlipayIncomeSummaryBll : Business<AlipayIncomeSummaryEntity, SqlConnectionFactory>
    {


        public static readonly AlipayIncomeSummaryBll Initialize = new AlipayIncomeSummaryBll();
        AlipayIncomeSummaryBll() { }

        public AlipayIncomeSummaryEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayIncomeSummaryEntity> List()
        {
            return Select();
        }

        public List<AlipayIncomeSummaryEntity> List(SqlExpression<AlipayIncomeSummaryEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayIncomeSummaryEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(AlipayElectronicBillEntity bill, AlipayIncomeSummaryEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.ElectronicBillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayIncomeSummaryBll--ActionSave", ex);
            }

        }
        public void ActionSave(AlipayElectronicBillEntity bill, List<AlipayIncomeSummaryEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.ElectronicBillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayIncomeSummaryBll--ActionSave", ex);

            }
        }
    }
}
