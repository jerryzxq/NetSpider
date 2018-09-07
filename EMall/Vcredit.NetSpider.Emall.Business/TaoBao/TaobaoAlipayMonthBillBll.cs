using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business
{
	
	public class TaobaoAlipayMonthBillBll : Business<TaobaoAlipayMonthBillEntity, SqlConnectionFactory>
	{
	  
        public static readonly TaobaoAlipayMonthBillBll Initialize = new TaobaoAlipayMonthBillBll();
        TaobaoAlipayMonthBillBll() { }

	    public TaobaoAlipayMonthBillEntity Load(UInt64 id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoAlipayMonthBillEntity> List()
        {
            return Select();
        }

        public List<TaobaoAlipayMonthBillEntity> List(SqlExpression<TaobaoAlipayMonthBillEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(TaobaoAlipayMonthBillEntity item)
        {
            return base.Save(item);
        }

        public void SaveAsync(TaobaoAlipayMonthBillEntity item)
        {
            Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());

        }

	}
}
