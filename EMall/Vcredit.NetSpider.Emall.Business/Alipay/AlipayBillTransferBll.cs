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

    public class AlipayBillTransferBll : Business<AlipayBillTransferEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillTransferBll Initialize = new AlipayBillTransferBll();
        AlipayBillTransferBll() { }

        public AlipayBillTransferEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillTransferEntity> List()
        {
            return Select();
        }

        public List<AlipayBillTransferEntity> List(SqlExpression<AlipayBillTransferEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillTransferEntity item)
        {
            return base.Save(item);
        }
        public void ActionSave(AlipayBillEntity bill, AlipayBillTransferEntity entity)
        {
            try
            {
                if (entity == null) return;
                entity.BillID = bill.ID;
                base.Save(entity);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillTransferBll--ActionSave", ex);
            }
        }
        public void ActionSave(AlipayBillEntity bill, List<AlipayBillTransferEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.BillID = bill.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillTransferBll--ActionSave", ex);
            }
        }


    }
}
