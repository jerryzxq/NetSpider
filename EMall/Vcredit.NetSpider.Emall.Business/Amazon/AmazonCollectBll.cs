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
	
	public class AmazonCollectBll : Business<AmazonCollectEntity, SqlConnectionFactory>
	{
        public static readonly AmazonCollectBll Initialize = new AmazonCollectBll();
        AmazonCollectBll() { }

        public AmazonCollectEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonCollectEntity> List()
        {
            return Select();
        }

        public List<AmazonCollectEntity> List(SqlExpression<AmazonCollectEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<AmazonCollectEntity> list)
        {
            base.Insert(list);
        }
        //public void SaveAsync(AmazonCollectEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
