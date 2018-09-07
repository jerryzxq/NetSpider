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
	
	public class SuningReceiveAddressBll : Business<SuningReceiveAddressEntity, SqlConnectionFactory>
	{
	
	  
        public static readonly SuningReceiveAddressBll Initialize = new SuningReceiveAddressBll();
        SuningReceiveAddressBll() { }

	    public SuningReceiveAddressEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningReceiveAddressEntity> List()
        {
            return Select();
        }

        public List<SuningReceiveAddressEntity> List(SqlExpression<SuningReceiveAddressEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningReceiveAddressEntity> list)
        {
            base.Insert(list); 
        }


        public override bool Save(SuningReceiveAddressEntity item)
        {
            return base.Save(item);
        }


        public void ActionSave(SuningUserInfoEntity baic, List<SuningReceiveAddressEntity> list)
        {

            if (list == null ||list.Count == 0) return;
            list.ForEach(e => e.UserId = baic.ID);
            base.Save(list);
        }

	}
}
