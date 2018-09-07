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
	
	public class AmazonReceiveAddressBll : Business<AmazonReceiveAddressEntity, SqlConnectionFactory>
	{
        public static readonly AmazonReceiveAddressBll Initialize = new AmazonReceiveAddressBll();
        AmazonReceiveAddressBll() { }

        public AmazonReceiveAddressEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AmazonReceiveAddressEntity> List()
        {
            return Select();
        }

        public List<AmazonReceiveAddressEntity> List(SqlExpression<AmazonReceiveAddressEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<AmazonReceiveAddressEntity> list)
        {
            base.Insert(list);
        }
        //public void SaveAsync(AmazonReceiveaddressEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
