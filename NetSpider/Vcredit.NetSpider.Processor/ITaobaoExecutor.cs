using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Processor
{
    public interface ITaobaoExecutor
    {
        /// <summary>
        ///  获取天猫商家店铺的近三月销售额
        /// </summary>
        /// <param name="SellerAddress"></param>
        /// <returns></returns>
        TaobaoSellerRes GetTmallSellerTotalAmount(string SellerAddress);
        /// <summary>
        /// 获取淘宝商家店铺的近三月销售额
        /// </summary>
        /// <param name="SellerAddress"></param>
        /// <returns></returns>
        TaobaoSellerRes GetTaobaoSellerTotalAmount(string SellerAddress);
    }
}
