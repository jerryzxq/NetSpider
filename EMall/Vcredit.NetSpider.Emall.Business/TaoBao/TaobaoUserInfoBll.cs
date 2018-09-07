using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using Vcredit.Common.Utility;
using System.Text.RegularExpressions;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Business.TaoBao;

namespace Vcredit.NetSpider.Emall.Business.TaoBao
{

    public class TaobaoUserInfoBll : Business<TaobaoUserInfoEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoUserInfoBll Initialize = new TaobaoUserInfoBll();
        TaobaoUserInfoBll() { }

        public TaobaoUserInfoEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoUserInfoEntity> List()
        {
            return Select();
        }

        public List<TaobaoUserInfoEntity> List(SqlExpression<TaobaoUserInfoEntity> expression)
        {
            return Select(expression);
        }

        /// <summary>
        /// 根据用户传入的登陆名获取用户（登陆名可能是邮箱，手机号，账号）-- 以最后插入数据为准
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        public TaobaoUserInfoEntity GetByUserLoginName(string loginName)
        {
            SqlExpression<TaobaoUserInfoEntity> ex = this.SqlExpression();
            ex.OrderByDescending(x => x.CreateTime).Where(x => x.Account == loginName ||
                     x.MobilePhone == (loginName) ||
                     x.Email == (loginName));
            var exsitUser = this.Select(ex).FirstOrDefault();

            return exsitUser;
        }

        #region 初始化用户数据 -- 登陆过程数据保存
        /// <summary>
        /// 初始化用户数据 -- 登陆过程数据保存
        /// </summary>
        /// <param name="req"></param>
        public TaobaoUserInfoEntity SaveUserInfoForLogin(EmallReq req)
        {
            try
            {
                var userinfo = new TaobaoUserInfoEntity();
                this.MatchLoginName(userinfo, req.Username);
                userinfo.Token = req.Token;
                //userinfo.Password = req.Password;
                //userinfo.UmToken = req.UmToken;
                userinfo.IsCrawled = false;

                Initialize.Save(userinfo);
                return userinfo;
            }
            catch (Exception e)
            {
                Log4netAdapter.WriteError(string.Format("账户：{0} 信息：{1}", req.Username, "用户登陆--信息保存失败"), e);
                throw;
            }
        }
        private void MatchLoginName(TaobaoUserInfoEntity userinfo, string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
                return;

            // 判断 _loginReq.Username 是邮箱，手机，用户名，并赋值保存到数据库
            // 邮箱
            var regEmailResult = Regex.Match(loginName,
                @"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?")
                .Value;
            if (!string.IsNullOrEmpty(regEmailResult))
                userinfo.Email = loginName;
            else
            {
                // 手机
                var regMobileResult = Regex.Match(loginName, @"^1[\d]{10}$").Value;
                if (!string.IsNullOrEmpty(regMobileResult))
                    userinfo.MobilePhone = loginName;
                else
                    userinfo.Account = loginName;
            }
        }
        #endregion

        /// <summary>
        /// 根据token获取用户
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public TaobaoUserInfoEntity GetByToken(string token)
        {
            return this.Select(x => x.Token == token).FirstOrDefault();
        }

        #region 判断是否实名认证
        /// <summary>
        /// 判断是否实名认证
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public bool IsAuth(EmallCheckIsRealNameReq req)
        {
            var result = false;
            var user = this.Select(x => x.Token == req.Token).FirstOrDefault();
            if (user == null)
                return result;

            // 支付宝绑定设置，获取实名认证信息
            var alipayInfo = TaobaoAlipaybindBll.Initialize.Select(x => x.UserId == user.Id).FirstOrDefault();
            if (alipayInfo != null && !alipayInfo.Name.IsEmpty() && !alipayInfo.IdentityCard.IsEmpty())
            {
                result = FrameWorkCommon.IsAuthV2(req.Name, alipayInfo.Name,
                                                req.IdentityCard, alipayInfo.IdentityCard);
            }
            // 如果支付宝绑定设置信息不存在就根据用户地址数据判定：用户姓名，手机号码判断
            else
            {
                result = this.CheckUserAddress(req, user);
            }

            return result;
        }

        private bool CheckUserAddress(EmallCheckIsRealNameReq req, TaobaoUserInfoEntity user)
        {
            var addrs = TaobaoReceiveAddressBll.Initialize.Select(x => x.UserId == user.Id);
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
                    // 13********88
                    // 18601785695
                    // 021-56513801 18019317286
                    // 86-18262280463 
                    // TelePhone 多种格式处理
                    var opMobiles = addr.TelePhone.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var opMobile in opMobiles)
                    {
                        var opMobileCopy = opMobile;
                        if (opMobile.Contains("-"))
                        {
                            var arr = opMobile.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length >= 2)
                                opMobileCopy = arr[1];
                        }

                        result = FrameWorkCommon.CheckMobilePartlyMatch(req.Mobile, opMobileCopy);

                        if (result)
                            return result;
                    }
                }
            }
            return result;
        }
        #endregion

    }
}
