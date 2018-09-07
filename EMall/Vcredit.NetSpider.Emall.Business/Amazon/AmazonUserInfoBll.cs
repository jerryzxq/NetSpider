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
	
	public class AmazonUserInfoBll : Business<AmazonUserInfoEntity, SqlConnectionFactory>
	{
        public static readonly AmazonUserInfoBll Initialize = new AmazonUserInfoBll();
        AmazonUserInfoBll() { }

	    public AmazonUserInfoEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public AmazonUserInfoEntity LoadByToken(string token)
        {
            return Single(e => e.Token == token);
        }

        public List<AmazonUserInfoEntity> List()
        {
            return Select();
        }

        public List<AmazonUserInfoEntity> List(SqlExpression<AmazonUserInfoEntity> expression)
        {
            return Select(expression);
        }

        public long Insert(AmazonUserInfoEntity userInfo)
        {
            return base.InsertIdentity(userInfo);
        }

        public AmazonUserInfoEntity GetUserInfoEntityByToken(string token)
        {
            return Single(e => e.Token == token);
        }

        //public void SaveAsync(AmazonUserinfoEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
