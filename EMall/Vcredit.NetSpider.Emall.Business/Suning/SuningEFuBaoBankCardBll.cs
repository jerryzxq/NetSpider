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

    public class SuningEFuBaoBankCardBll : Business<SuningEFuBaoBankCardEntity, SqlConnectionFactory>
	{
        public static readonly SuningEFuBaoBankCardBll Initialize = new SuningEFuBaoBankCardBll();
        SuningEFuBaoBankCardBll() { }

        public SuningEFuBaoBankCardEntity Load(int id)
        {
            return Single(e => e.ID == id);
        }

        public List<SuningEFuBaoBankCardEntity> List()
        {
            return Select();
        }

        public List<SuningEFuBaoBankCardEntity> List(SqlExpression<SuningEFuBaoBankCardEntity> expression)
        {
            return Select(expression);
        }

        public override void Insert(List<SuningEFuBaoBankCardEntity> list)
        {
            base.Insert(list);
        }

        public void ActionSave(SuningUserInfoEntity baic, List<SuningEFuBaoBankCardEntity> list)
        {
            if (list.Count == 0) return;
            list.ForEach(e=>e.UserId = baic.ID); 
            base.Save(list);
        }

        public List<SuningEFuBaoBankCardEntity> GetSuningEFuBaoBankCardByToken(string token)
        {
            var user = SuningUserInfoBll.Initialize.GetUserInfoByToken(token);
            if (user != null)
            {
                return Select(e=>e.UserId == user.ID);
            } 
            return null;
        }

        //public void SaveAsync(SuningEfubaobankcardEntity item)
        //{
        //    Task.Factory.StartNew(() => { base.Save(item); }).ContinueWith(e => e.Dispose());
        //}
	}
}
