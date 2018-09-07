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
        /// �����û�����ĵ�½����ȡ�û�����½�����������䣬�ֻ��ţ��˺ţ�-- ������������Ϊ׼
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

        #region ��ʼ���û����� -- ��½�������ݱ���
        /// <summary>
        /// ��ʼ���û����� -- ��½�������ݱ���
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
                Log4netAdapter.WriteError(string.Format("�˻���{0} ��Ϣ��{1}", req.Username, "�û���½--��Ϣ����ʧ��"), e);
                throw;
            }
        }
        private void MatchLoginName(TaobaoUserInfoEntity userinfo, string loginName)
        {
            if (string.IsNullOrEmpty(loginName))
                return;

            // �ж� _loginReq.Username �����䣬�ֻ����û���������ֵ���浽���ݿ�
            // ����
            var regEmailResult = Regex.Match(loginName,
                @"[\w!#$%&'*+/=?^_`{|}~-]+(?:\.[\w!#$%&'*+/=?^_`{|}~-]+)*@(?:[\w](?:[\w-]*[\w])?\.)+[\w](?:[\w-]*[\w])?")
                .Value;
            if (!string.IsNullOrEmpty(regEmailResult))
                userinfo.Email = loginName;
            else
            {
                // �ֻ�
                var regMobileResult = Regex.Match(loginName, @"^1[\d]{10}$").Value;
                if (!string.IsNullOrEmpty(regMobileResult))
                    userinfo.MobilePhone = loginName;
                else
                    userinfo.Account = loginName;
            }
        }
        #endregion

        /// <summary>
        /// ����token��ȡ�û�
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public TaobaoUserInfoEntity GetByToken(string token)
        {
            return this.Select(x => x.Token == token).FirstOrDefault();
        }

        #region �ж��Ƿ�ʵ����֤
        /// <summary>
        /// �ж��Ƿ�ʵ����֤
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public bool IsAuth(EmallCheckIsRealNameReq req)
        {
            var result = false;
            var user = this.Select(x => x.Token == req.Token).FirstOrDefault();
            if (user == null)
                return result;

            // ֧���������ã���ȡʵ����֤��Ϣ
            var alipayInfo = TaobaoAlipaybindBll.Initialize.Select(x => x.UserId == user.Id).FirstOrDefault();
            if (alipayInfo != null && !alipayInfo.Name.IsEmpty() && !alipayInfo.IdentityCard.IsEmpty())
            {
                result = FrameWorkCommon.IsAuthV2(req.Name, alipayInfo.Name,
                                                req.IdentityCard, alipayInfo.IdentityCard);
            }
            // ���֧������������Ϣ�����ھ͸����û���ַ�����ж����û��������ֻ������ж�
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
            // �������ֻ�����һ��ƥ�伴��
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
                    // TelePhone ���ָ�ʽ����
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
