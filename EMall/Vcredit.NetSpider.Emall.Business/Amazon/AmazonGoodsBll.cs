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
	
	public class AmazonGoodsBll : Business<AmazonGoodsEntity, SqlConnectionFactory>
	{
        public static readonly AmazonGoodsBll Initialize = new AmazonGoodsBll();
        AmazonGoodsBll() { }

        public AmazonGoodsEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonGoodsEntity> List()
        {
            return Select();
        }

        public List<AmazonGoodsEntity> SelectAmazonGoodsListByOrderId(long orderId)
        {
            return Select(e=>e.OrderID==orderId);
        }

        public List<AmazonGoodsEntity> List(SqlExpression<AmazonGoodsEntity> expression)
        {
            return Select(expression);
        }

        public void ActionSave(AmazonOrderEntity amazonOrder, List<AmazonGoodsEntity> amazonOrderGoods)
        {
            if (amazonOrderGoods == null || amazonOrderGoods.Count == 0) return;
            amazonOrderGoods.ForEach(e => e.OrderID = amazonOrder.ID);
            base.Save(amazonOrderGoods);
        }
        //public void SaveAsync(AmazonGoodsEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
