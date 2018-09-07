using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.VipShop;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.VipShop
{
	
	public class VipshopReceiveAddressBll : Business<VipshopReceiveAddressEntity, SqlConnectionFactory>
	{
        public static readonly VipshopReceiveAddressBll Initialize = new VipshopReceiveAddressBll();
        VipshopReceiveAddressBll() { }

	    public VipshopReceiveAddressEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopReceiveAddressEntity> List()
        {
            return Select();
        }

        public List<VipshopReceiveAddressEntity> List(SqlExpression<VipshopReceiveAddressEntity> expression)
        {
            return Select(expression);
        }

	}
}
