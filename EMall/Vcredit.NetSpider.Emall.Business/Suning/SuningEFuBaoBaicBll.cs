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

    public class SuningEFuBaoBaicBll : Business<SuningEFuBaoBaicEntity, SqlConnectionFactory>
	{
         public static readonly SuningEFuBaoBaicBll Initialize = new SuningEFuBaoBaicBll();
         SuningEFuBaoBaicBll() { }

	    public SuningEFuBaoBaicEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoBaicEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoBaicEntity> List(SqlExpression<SuningEFuBaoBaicEntity> expression)
        {
            return Select(expression);
        }

        public SuningEFuBaoBaicEntity GetSuningEFuBaoByToken(string token)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(token);
            if (user != null)
            {
                return Single(e=>e.UserId == user.ID);
            }
            return null;
        }

        public override bool Insert(Vcredit.NetSpider.Emall.Entity.SuningEFuBaoBaicEntity item)
        {
            return base.Insert(item);
        }

        public override bool Save(SuningEFuBaoBaicEntity item)
        {
            return base.Save(item);
        }
        //public void SaveAsync(SuningEfubaobaicEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}

        public void ActionSave(SuningUserInfoEntity baic, SuningEFuBaoBaicEntity item)
        {
            if (item == null) return;
            item.UserId = baic.ID;
            base.Save(item);
        }
	}
}
