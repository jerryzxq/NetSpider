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

    public class AlipayConsumptionBll : Business<AlipayConsumptionEntity, SqlConnectionFactory>
    {


        public static readonly AlipayConsumptionBll Initialize = new AlipayConsumptionBll();
        AlipayConsumptionBll() { }

        public AlipayConsumptionEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayConsumptionEntity> List()
        {
            return Select();
        }

        public List<AlipayConsumptionEntity> List(SqlExpression<AlipayConsumptionEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayConsumptionEntity item)
        {
            return base.Save(item);
        }
        public void ActionSave(AlipayElectronicBillEntity bill, AlipayConsumptionEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.ElectronicBillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayConsumptionBll--ActionSave", ex);
            }
        }
        public void ActionSave(AlipayElectronicBillEntity bill, List<AlipayConsumptionEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.ElectronicBillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayConsumptionBll--ActionSave", ex);
            }
        }



    }
}
