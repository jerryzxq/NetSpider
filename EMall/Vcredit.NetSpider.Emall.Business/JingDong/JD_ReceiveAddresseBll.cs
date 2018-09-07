using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Dto.JingDong;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_ReceiveAddresseBll : Business<ReceiveAddressEntity, SqlConnectionFactory>
    {
        /// <summary>
        /// 获取收货地址
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public List<JdReceiveAddressDto> QueryByToken(string token)
        {
            var user = new JD_UserInfoBll().Select(x => x.Token == token).FirstOrDefault();
            if (user == null)
                return new List<JdReceiveAddressDto>();

            var data = this.Select(x => x.UserId == user.ID);
            return data.Select(x => MapperHelper.Map<ReceiveAddressEntity, JdReceiveAddressDto>(x)).ToList();
        }
    }
}
