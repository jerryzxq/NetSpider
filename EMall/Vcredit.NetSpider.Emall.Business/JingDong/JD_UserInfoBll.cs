using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class JD_UserInfoBll : Business<UserInfoEntity, SqlConnectionFactory>
    {

        public UserInfoEntity GetUserEntityBytoken(string token)
        {
            var spdentity = spd_applyBll.Initialize.LoadByToken(token);
            string realToken = string.IsNullOrEmpty(spdentity.Ptoken) ? spdentity.Token : spdentity.Ptoken;
            var userInfo = this.Single(x => x.Token == realToken);
            if (userInfo == null)
                return null;
            return userInfo;
        }

        /// <summary>
        /// 判断是否实名认证
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public bool IsAuth(EmallCheckIsRealNameReq req)
        {
            var result = false;
            var user = Select(x => x.Token == req.Token).FirstOrDefault();
            if (user == null)
                return result;

            // 个人信息中：真是姓名，身份证，如果个人信息中认证身份证号，认证姓名为空通过下面 else 中根据地址数据判断
            if (!user.IdentityCard.IsEmpty() && !user.AuthName.IsEmpty())
            {
                result = FrameWorkCommon.IsAuthV2(req.Name, user.AuthName,
                                               req.IdentityCard, user.IdentityCard);
            }
            // 根据地址中的姓名，手机号判断
            else
            {
                result = this.CheckUserAddress(req, user);
            }

            return result;
        }

        private bool CheckUserAddress(EmallCheckIsRealNameReq req, UserInfoEntity user)
        {
            var addrs = new JD_ReceiveAddresseBll().Select(x => x.UserId == user.ID);
            if (addrs == null || !addrs.Any())
                return false;

            var result = false;
            // 姓名或手机号码一个匹配即可
            if (addrs.Where(x => x.Receiver.Equals(req.Name)).FirstOrDefault() != null)
            {
                result = true;
            }
            else
            {
                foreach (var addr in addrs)
                {
                    result = FrameWorkCommon.CheckMobilePartlyMatch(req.Mobile, addr.Mobile);
                    if (result)
                        return result;
                }
            }

            return result;
        }
    }
}
