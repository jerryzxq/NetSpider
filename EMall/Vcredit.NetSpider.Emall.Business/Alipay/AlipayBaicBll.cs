using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity;
using System.Threading.Tasks;
using Vcredit.Common.Utility;
namespace Vcredit.NetSpider.Emall.Business
{

    public class AlipayBaicBll : Business<AlipayBaicEntity, SqlConnectionFactory>
    {


        public static readonly AlipayBaicBll Initialize = new AlipayBaicBll();
        AlipayBaicBll() { }

        public AlipayBaicEntity Load(long id)
        {
            return Single(e => e.ID == id);
        }

        public List<AlipayBaicEntity> List()
        {
            return Select();
        }
        public AlipayBaicEntity LoadByToken(string token)
        {
            return Single(e => e.Token == token);
        }

        public List<AlipayBaicEntity> List(SqlExpression<AlipayBaicEntity> expression)
        {
            return Select(expression);
        }


        public override bool Save(AlipayBaicEntity item)
        {

            //SqlExpression<AlipayBaicEntity> sql = SqlExpression();

            //var items = db.Select<Items>(q => Sql.In(q.Id, ids));

            return base.Save(item);

        }

        public void CrawlerSave(AlipayBaicEntity item)
        {
            item.SaveAction<AlipayAddressEntity>(item.Address, AlipayAddressBll.Initialize.ActionSave);
            item.SaveAction<AlipayBankEntity>(item.Bank, AlipayBankBll.Initialize.ActionSave);
            //item.SaveAction<AlipayElectronicBillEntity>(item.ElectronicBill, AlipayElectronicBillBll.Initialize.ActionSave);

        }



        /// <summary>
        /// ≤‚ ‘ ¬ŒÔ
        /// </summary>
        public void Trant()
        {
            using (var db = DBConnection)
            {
                using (var tran = db.OpenTransaction())
                {
                    try
                    {
                        AlipayBaicEntity item = new AlipayBaicEntity()
                        {
                            Email = "1"

                        };
                        db.Save(item);
                        AlipayBaicEntity item2 = new AlipayBaicEntity()
                        {
                            Email = "1"

                        };
                        db.Save(item2);

                        tran.Commit();
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                    }
                }


            }


        }

    }

    public static class AlipayBaicBillExt
    {

        public static void SaveAction<T>(this AlipayBaicEntity baic, List<T> list, Action<AlipayBaicEntity, List<T>> action)
        {
            try
            {
                if (list == null || list.Count == 0) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBaicBillExt", ex);

            }
        }
        public static void SaveAction<T>(this AlipayBaicEntity baic, T list, Action<AlipayBaicEntity, T> action)
        {
            try
            {
                if (list == null) return;
                action.Invoke(baic, list);
            }
            catch (Exception ex)
            {
                Log4netAdapter.WriteError("AlipayBaicBillExt", ex);

            }
        }

    }
}
