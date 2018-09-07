using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Entity.VipShop;
using Vcredit.NetSpider.Emall.Dto;

namespace Vcredit.NetSpider.Emall.Business.VipShop
{
	
	public class VipshopUserInfoBll : Business<VipshopUserInfoEntity, SqlConnectionFactory>
	{
        public static readonly VipshopUserInfoBll Initialize = new VipshopUserInfoBll();
        VipshopUserInfoBll() { }

	    public VipshopUserInfoEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<VipshopUserInfoEntity> List()
        {
            return Select();
        }

        public List<VipshopUserInfoEntity> List(SqlExpression<VipshopUserInfoEntity> expression)
        {
            return Select(expression);
        }

        public VipshopUserInfoEntity GetByToken(string token)
        {
            return this.Select(x => x.Token == token).FirstOrDefault();
        }
       
    }
}
