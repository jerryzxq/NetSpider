using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Entity.Service;

namespace Vcredit.NetSpider.Processor
{
    public interface IPbccrcExecutor
    {
        /// <summary>
        /// 央行征信登录页面初始化
        /// </summary>
        /// <returns></returns>
        VerCodeRes Init();
        /// <summary>
        /// 央行征信注册,第一步
        /// </summary>
        /// <param name="name">姓名</param>
        /// <param name="certNo">证件号</param>
        /// <param name="certType">证件类型,0-身份证、1-户口簿、2-护照、3-军官证、4-士兵证、5-港澳居民来往内地通行证、6-台湾同胞来往内地通行证、7-临时身份证、8-外国人居留证、9-警官证、A-香港身份证、B-澳门身份证、C-台湾身份证、X-其他证件、</param>
        /// <returns></returns>
        BaseRes Register_Step1(string token, string name, string certNo, string certType, string verCode);
        /// <summary>
        /// 央行互联网认证注册,第二步，发送手机验证码
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="mobileTel">手机号</param>
        /// <returns></returns>
        BaseRes Register_Step2(string token, string mobileTel);
        /// <summary>
        /// 央行互联网认证注册,第三步，补充用户信息
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="loginname">登录账号</param>
        /// <param name="password">密码</param>
        /// <param name="confirmpassword">确认密码</param>
        /// <param name="email">电子邮件</param>
        /// <param name="mobileTel">手机号</param>
        /// <param name="verifyCode">手机验证码</param>
        /// <returns></returns>
        BaseRes Register_Step3(string token, string username, string password, string confirmpassword, string email, string mobileTel, string verifyCode);
        /// <summary>
        /// 央行征信登录
        /// </summary>
        /// <param name="username">登录名</param>
        /// <param name="Passoword">密码</param>
        /// <param name="VerCode">验证码</param>
        /// <returns></returns>
        BaseRes Login(string token, string username, string passoword, string VerCode);
        /// <summary>
        /// 征信查询码申请，第一步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="isContinue">如果已有报告，是否继续重新申请</param>
        /// <returns></returns>
        BaseRes QueryApplication_Step1(string token, bool isContinue,string identitycard);
        /// <summary>
        /// 征信查询码申请，第二步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="kbaList">验证问题集合</param>
        /// <returns></returns>
        BaseRes QueryApplication_Step2(string token, List<CRD_KbaQuestion> kbaList);
        /// <summary>
        /// 征信银行卡查询码申请，第一步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="isContinue">如果已有报告，是否继续重新申请</param>
        /// <returns></returns>
        VerCodeRes QueryApplication_CreditCard_Step1(string token, bool isContinue);
        /// <summary>
        /// 征信查询码申请，第二步
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <param name="kbaList">验证问题集合</param>
        /// <returns></returns>
        BaseRes QueryApplication_CreditCard_Step2(string token, string unionpaycode, string vercode);
        /// <summary>
        /// 获取银联认证码，初始化
        /// </summary>
        /// <param name="html">获取银联认证码的页面信息</param>
        /// <returns></returns>
        BaseRes QueryApplication_GetUnionPayCode_Init(string html);
        /// <summary>
        /// 获取银联认证码，第一步
        /// </summary>
        /// <param name="html">本次会话令牌</param>
        /// <param name="creditcardNo">银联卡号码</param>
        /// <returns></returns>
        BaseRes QueryApplication_GetUnionPayCode_Step1(string token, string creditcardNo);
        /// <summary>
        /// 获取银联认证码，第二步
        /// </summary>
        /// <param name="html">本次会话令牌</param>
        /// <param name="mobileTel">银联卡预留手机号码</param>
        /// <returns></returns>
        BaseRes QueryApplication_GetUnionPayCode_Step2(string token, string mobile);
        /// <summary>
        /// 获取银联认证码，第三步
        /// </summary>
        /// <param name="html">本次会话令牌</param>
        /// <param name="mobileTel">银联卡预留手机号码</param>
        /// <returns></returns>
        BaseRes QueryApplication_GetUnionPayCode_Step3(UnionPayReq unionpay);
        /// <summary>
        /// 征信查询码申请状态查询
        /// </summary>
        /// <param name="token">本次会话令牌</param>
        /// <returns></returns>
        BaseRes QueryApplication_Result(string token);
        /// <summary>
        /// 获取征信报告
        /// </summary>
        /// <param name="username"></param>
        /// <param name="querycode"></param>
        /// <returns></returns>
        CRD_HD_REPORTRes GetReport(PbccrcReportQueryReq queryReq);
        /// <summary>
        /// 填写征信空白
        /// </summary>
        /// <param name="identitycard">身份证号</param>
        /// <returns></returns>
        BaseRes QueryApplication_AddBlankRecord(string certNo, string name);

        BaseRes GetListCrdQrrecorddtlByrepid(int repid);
        /// <summary>
        /// 重新发送查询码
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        BaseRes QueryReport_SendQueryCode(string token);
    }
}
