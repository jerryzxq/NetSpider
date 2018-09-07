using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business.TaoBao
{

    public class TaobaoReceiveAddressBll : Business<TaobaoReceiveAddressEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoReceiveAddressBll Initialize = new TaobaoReceiveAddressBll();
        TaobaoReceiveAddressBll() { }

        public TaobaoReceiveAddressEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoReceiveAddressEntity> List()
        {
            return Select();
        }

        public List<TaobaoReceiveAddressEntity> List(SqlExpression<TaobaoReceiveAddressEntity> expression)
        {
            return Select(expression);
        }

        /// <summary>
        /// 根据token获取收货地址
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public List<TaobaoReceiveAddressDto> QueryByToken(string token)
        {
            var userinfo = TaobaoUserInfoBll.Initialize.GetByToken(token);
            if (userinfo == null)
                return new List<TaobaoReceiveAddressDto>();

            var data = this.Select(x => x.UserId == userinfo.Id);
            return data.Select(x => MapperHelper.Map<TaobaoReceiveAddressEntity, TaobaoReceiveAddressDto>(x)).ToList();
        }
    }
}
