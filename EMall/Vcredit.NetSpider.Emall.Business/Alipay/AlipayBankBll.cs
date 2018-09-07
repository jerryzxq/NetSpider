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
	
	public class AlipayBankBll : Business<AlipayBankEntity, SqlConnectionFactory>
	{
	
	 
        public static readonly AlipayBankBll Initialize = new AlipayBankBll();
        AlipayBankBll() { }

	    public AlipayBankEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBankEntity> List()
        {
            return Select();
        }

        public List<AlipayBankEntity> List(SqlExpression<AlipayBankEntity> expression)
        {
            return Select(expression);
        }

        public override bool Save(AlipayBankEntity item)
        {
            
            return base.Save(item);
        }
        public void ActionSave(AlipayBaicEntity baic, List<AlipayBankEntity> list)
        {
            try
            {
                if (list==null||list.Count == 0) return;
                list.ForEach(e => e.BaicID = baic.ID);
                base.Save(list);
            }
            catch (Exception ex) {
                Log4netAdapter.WriteError("AlipayBankBll--ActionSave", ex);
            }
        }

	}

}
