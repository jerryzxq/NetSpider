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

    public class AmazonTradeRecordBll : Business<AmazonTradeRecordEntity, SqlConnectionFactory>
	{
        public static readonly AmazonTradeRecordBll Initialize = new AmazonTradeRecordBll();
        AmazonTradeRecordBll() { }

        public AmazonTradeRecordEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonTradeRecordEntity> List()
        {
            return Select();
        }

        public List<AmazonTradeRecordEntity> List(SqlExpression<AmazonTradeRecordEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<AmazonTradeRecordEntity> list)
        {
            base.Insert(list);
        }
        //public void SaveAsync(AmazonTraderecordEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
