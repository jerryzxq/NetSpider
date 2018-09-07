using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_ShoppingCartBll:Business<ShoppingCartEntity, SqlConnectionFactory>
    {
        public List<ShoppingCartEntity> GetShoppingCartInfoByUseris(int userid)
        {
           return this.Select(param => param.UserId == userid);

        }

    }
}
