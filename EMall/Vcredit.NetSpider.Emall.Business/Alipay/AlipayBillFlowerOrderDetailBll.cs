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

    public class AlipayBillFlowerOrderDetailBll : Business<AlipayBillFlowerOrderDetailEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBillFlowerOrderDetailBll Initialize = new AlipayBillFlowerOrderDetailBll();
        AlipayBillFlowerOrderDetailBll() { }

        public AlipayBillFlowerOrderDetailEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBillFlowerOrderDetailEntity> List()
        {
            return Select();
        }

        public List<AlipayBillFlowerOrderDetailEntity> List(SqlExpression<AlipayBillFlowerOrderDetailEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBillFlowerOrderDetailEntity item)
        {
            return base.Save(item);
        }



        public void ActionSave(AlipayBillFlowerEntity billFlower, List<AlipayBillFlowerOrderDetailEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                list.ForEach(e => e.BillFlowerID = billFlower.ID);
                base.Save(list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBillFlowerOrderDetailBll--ActionSave", ex);

            }

        }

        public void ActionDeleteById(AlipayBillFlowerEntity billFlower, List<AlipayBillFlowerOrderDetailEntity> list)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                foreach (var item in SelectFlowerOrderDetailByFlowerId(billFlower.ID))
                {
                    base.DeleteById(item.ID);
                }
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("ActionDeleteById", ex);

            }
        }

        private List<AlipayBillFlowerOrderDetailEntity> SelectFlowerOrderDetailByFlowerId(long billFlowerId)
        {
            SqlExpression<AlipayBillFlowerOrderDetailEntity> ex = SqlExpression();
            //对最近3个月的数据进行分析
            ex.Where(e => e.BillFlowerID == billFlowerId);
            return this.Select(ex);
        }

    }
}
