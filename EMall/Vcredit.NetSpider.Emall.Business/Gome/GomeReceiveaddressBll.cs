using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Gome;
using System.Threading.Tasks;
namespace Vcredit.NetSpider.Emall.Business.Gome
{
	
	public class GomeReceiveaddressBll : Business<GomeReceiveaddressEntity, SqlConnectionFactory>
	{
        public static readonly GomeReceiveaddressBll Initialize = new GomeReceiveaddressBll();
        GomeReceiveaddressBll() { }

	    public GomeReceiveaddressEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeReceiveaddressEntity> List()
        {
            return Select();
        }

        public List<GomeReceiveaddressEntity> List(SqlExpression<GomeReceiveaddressEntity> expression)
        {
            return Select(expression);
        }

        //public void SaveAsync(GomeReceiveaddressEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
