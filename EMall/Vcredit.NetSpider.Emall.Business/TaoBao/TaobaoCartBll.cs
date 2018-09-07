using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Emall.Business.TaoBao
{

    public class TaobaoCartBll : Business<TaobaoCartEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoCartBll Initialize = new TaobaoCartBll();
        TaobaoCartBll() { }

        public TaobaoCartEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoCartEntity> List()
        {
            return Select();
        }

        public List<TaobaoCartEntity> List(SqlExpression<TaobaoCartEntity> expression)
        {
            return Select(expression);
        }

        /// <summary>
        /// 根据token查询用户购物车数据
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public List<TaobaoCartEntity> QueryList(string token)
        {
            //var sqlCommand = string.Format(@" SELECT Id
            //                                  ,UserId
            //                                  ,Seller
            //                                  ,ShopUrl
            //                                  ,ProductTitle
            //                                  ,Description
            //                                  ,Price
            //                                  ,Count
            //                                  ,TotalAmount
            //                                  ,Service
            //                                  ,IsValid
            //                              FROM taobao_cart
            //                            WHERE UserId = (SELECT Id FROM taobao_userInfo WHERE Token = '{0}') ", token);

            //return DBConnection.Select<TaobaoCartEntity>(sqlCommand);

            var userInfo = TaobaoUserInfoBll.Initialize.GetByToken(token);
            if (userInfo == null)
                return null;

            SqlExpression<TaobaoCartEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.UserId == userInfo.Id);

            var data = Initialize.Select(sqlexp);
            return data;
        }
    }
}
