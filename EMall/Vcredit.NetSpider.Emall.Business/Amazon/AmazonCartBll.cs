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
	
	public class AmazonCartBll : Business<AmazonCartEntity, SqlConnectionFactory>
	{
        public static readonly AmazonCartBll Initialize = new AmazonCartBll();
        AmazonCartBll() { }

        public AmazonCartEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonCartEntity> LoadCartByUserId(long userId)
        {
            return Select(e=>e.UserId == userId);
        }

        public List<AmazonCartEntity> List()
        {
            return Select();
        }
          
        public List<AmazonCartEntity> List(SqlExpression<AmazonCartEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<AmazonCartEntity> list)
        {
            base.Insert(list);
        }

        //public void SaveAsync(AmazonCartEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
