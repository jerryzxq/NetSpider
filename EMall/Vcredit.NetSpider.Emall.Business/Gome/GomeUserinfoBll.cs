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
	
	public class GomeUserinfoBll : Business<GomeUserinfoEntity, SqlConnectionFactory>
	{
        public static readonly GomeUserinfoBll Initialize = new GomeUserinfoBll();
        GomeUserinfoBll() { }

	    public GomeUserinfoEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeUserinfoEntity> List()
        {
            return Select();
        }

        public List<GomeUserinfoEntity> List(SqlExpression<GomeUserinfoEntity> expression)
        {
            return Select(expression);
        }

        public GomeUserinfoEntity GetByToken(string token)
        {
            return this.Select(x => x.Token == token).FirstOrDefault();
        }

        //public void SaveAsync(GomeUserinfoEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
    }
}
