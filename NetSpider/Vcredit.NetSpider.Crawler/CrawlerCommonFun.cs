using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.Mongo.Log;
using Vcredit.NetSpider.Entity.Mongo.Mobile;
using Vcredit.Common.Ext;
using System.Text.RegularExpressions;

namespace Vcredit.NetSpider.Crawler
{
    public class CrawlerCommonFun
    {
        /// <summary>
        /// 正则表达式转义
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetMobleCrawlerStatusCode(string message)
        {
            int code = ServiceConsts.StatusCode_fail;
            if (message.Contains("短信随机码有误") || message.Contains("动态验证码错误")
               || message.Contains("短信随机码不正确") || message.Contains("短信码验证失败")
               || message.Contains("手机验证码校验异常") || message.Contains("短信验证码错误")
               || message.Contains("短信验证出错") || message.Contains("短信验证码输入错误")
               || message.Contains("动态密码错误"))
                code = ServiceConsts.CrawlerStatusCode_SmsCodeError;
            else if (message.Contains("用户名或密码不正确") || message.Contains("账户名与密码不匹配")
               || message.Contains("密码错误") || message.Contains("密码长度不正确")
               || message.Contains("服务密码错误") || message.Contains("用户/密码认证不正确")
               || message.Contains("手机号或密码输入不正确"))
                code = ServiceConsts.CrawlerStatusCode_PasswordError;
            else if (message.Contains("简单密码") || message.Contains("密码过于简单")
                || message.Contains("不支持初始密码"))
                code = ServiceConsts.CrawlerStatusCode_PasswordEasy;
            else if (message.Contains("请找回登录密码后登录") || message.Contains("账号已被锁定") || message.Contains("账号已锁定")
                || message.Contains("登录过于频繁") || message.Contains("账号锁定") || message.Contains("帐号被锁定") || message.Contains("账户被锁定")
                || message.Contains("密码错误次数已达上限") || message.Contains("登录密码出错已达上限")
                || message.Contains("您的IP访问已经受限") || message.Contains("密码被锁定") || message.Contains("服务密码已锁")
                || message.Contains("您使用的IP地址有较多登录失败的记录") || message.Contains("用户密码当前处于锁定状态"))
                code = ServiceConsts.CrawlerStatusCode_AccountLock;
            else if (message.Contains("系统正在升级") || message.Contains("系统升级"))
                code = ServiceConsts.CrawlerStatusCode_SystemUpdate;
            else if (message.Contains("系统忙") || message.Contains("服务繁忙") || message.Contains("系统繁忙"))
                code = ServiceConsts.CrawlerStatusCode_SystemBusy;
            else if (message.Contains("验证码错误") || message.Contains("验证码不正确")
                || message.Contains("验证码出错") || message.Contains("请输入正确的验证码")
                || message.Contains("验证码有误"))
                code = ServiceConsts.CrawlerStatusCode_VercodeError;
            else if (message.Contains("短信验证码已失效") || message.Contains("短信随机码已过期")
                || message.Contains("短信随机码不正确或已经过期") || message.Contains("短信随机码不正确或已过期")
                || message.Contains("动态密码失效"))
                code = ServiceConsts.CrawlerStatusCode_SmsCodeInvalidation;
            else if (message.Contains("登录保护") || message.Contains("请您明天再试"))
                code = ServiceConsts.CrawlerStatusCode_AccountProtect;
            else
                code = ServiceConsts.StatusCode_fail;

            return code;
        }

        /// <summary>
        /// 必要字段校验记录
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static void CheckColumn(Basic mobile, ApplyLog appLog, string mobileType = "")
        {
            string numstr = @"^[+-]?([0-9]*\.?[0-9]+|[0-9]+\.?[0-9]*)([eE][+-]?[0-9]+)?$"; //数字
            string hanzi = @"[\u4e00-\u9fa5]"; //汉字
            string times = @"^[1-9][0-9]{3}-(0[1-9]|1[0-2])-([0-2][1-9]|3[0-1])$"; //时间 yyyy-MM-dd

            #region 主表

            //姓名
            if (mobile.Name.IsEmpty() || !Regex.IsMatch(mobile.Name.Trim(), hanzi))
                WritingLog(appLog, "主表：姓名(Name)", mobile.Name);
            //证件号
            if (mobileType != "ChinaMobile" && appLog.Website != "ChinaMobile_CQ" && appLog.Website != "ChinaMobile_GX" && appLog.Website != "ChinaMobile_TJ" && appLog.Website != "ChinaMobile_YN")
            {
                if (mobile.Idcard.IsEmpty() || ((!mobile.Idcard.IsEmpty() && !mobile.Idcard.Trim().Replace("*", "").Replace("x", "").Replace("X", "").IsEmpty() && !mobile.Idcard.Contains("未登记")) && !Regex.IsMatch(mobile.Idcard.Trim().Replace("*", "").Replace("x", "").Replace("X", ""), numstr)))
                    WritingLog(appLog, "主表：证件号(Idcard)", mobile.Idcard);
            }
            //入网时间
            if (appLog.Website != "ChinaNet_CQ")
                if (mobile.Regdate.IsEmpty())
                    WritingLog(appLog, "主表：入网时间(Regdate)", mobile.Regdate);

            #endregion

            #region 月账单

            foreach (MonthBill bill in mobile.BillList)
            {
                // 计费周期
                if (!bill.PlanAmt.IsEmpty() && !Regex.IsMatch(bill.BillCycle.Trim(), times))
                    WritingLog(appLog, "账单：计费周期(BillCycle)", bill.BillCycle);
                //套餐金额
                if (bill.PlanAmt.IsEmpty() || !Regex.IsMatch(bill.PlanAmt.Trim(), numstr))
                    WritingLog(appLog, "账单：套餐金额(PlanAmt)", bill.PlanAmt);
                // 总金额
                if (bill.TotalAmt.IsEmpty() || !Regex.IsMatch(bill.TotalAmt.Trim(), numstr))
                    WritingLog(appLog, "账单：总金额(TotalAmt)", bill.TotalAmt);
            }

            #endregion

            #region 通话详单
            foreach (Call call in mobile.CallList)
            {
                //对方号码
                if (!call.OtherCallPhone.IsEmpty() && !Regex.IsMatch(call.OtherCallPhone.Trim(), numstr))
                    WritingLog(appLog, "通话详单：对方号码(OtherCallPhone)", call.OtherCallPhone);
                //通话时长
                if (!call.UseTime.IsEmpty() && !Regex.IsMatch(call.UseTime.Trim(), numstr))
                    WritingLog(appLog, "通话详单：通话时长(UseTime)", call.UseTime);
                //通话费用
                if (!Regex.IsMatch(call.SubTotal.ToString().Trim(), numstr))
                    WritingLog(appLog, "通话详单：通话费用(SubTotal)", call.SubTotal);
            }

            #endregion

            #region 短信详单

            foreach (Sms sms in mobile.SmsList)
            {
                //对方号码
                if (!sms.OtherSmsPhone.IsEmpty() && !Regex.IsMatch(sms.OtherSmsPhone.Trim(), numstr))
                    WritingLog(appLog, "短信详单：对方号码(OtherSmsPhone)", sms.OtherSmsPhone);
                //通信费用
                if (!Regex.IsMatch(sms.SubTotal.ToString().Trim(), numstr))
                    WritingLog(appLog, "短信详单：通信费用(SubTotal)", sms.SubTotal);
            }

            #endregion

            #region 流量详单

            foreach (Net net in mobile.NetList)
            {
                //单次费用
                if (!Regex.IsMatch(net.SubTotal.ToString().Trim(), numstr))
                    WritingLog(appLog, "流量详单：单次费用(SubTotal)", net.SubTotal);
                //单词流量
                if (!net.SubFlow.IsEmpty() && !Regex.IsMatch(net.SubFlow.Trim(), numstr))
                    WritingLog(appLog, "流量详单：单词流量(SubFlow)", net.SubFlow);
                //上网时间
                if (!net.UseTime.IsEmpty() && !Regex.IsMatch(net.UseTime.Trim(), numstr))
                    WritingLog(appLog, "流量详单：上网时间(UseTime)", net.UseTime);
            }

            #endregion

            appLog.Token = mobile.Token;
            appLog.EndDate = DateTime.Now.ToString(Consts.DateFormatString9);
        }

        private static void WritingLog(ApplyLog appLog, string column, object content)
        {
            ApplyLogDtl logDtl = new ApplyLogDtl(column);
            logDtl.StatusCode = ServiceConsts.CrawlerStatusCode_ColmonError;
            logDtl.CreateTime = DateTime.Now.ToString(Consts.DateFormatString9);
            logDtl.Description = column + "不合法:" + content;
            appLog.LogDtlList.Add(logDtl);
        }

        /// <summary>
        /// 获取运行商省份信息
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetProvinceInfoByWebsite(string website)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            if (website.IsEmpty()) return dic;
            string[] mobileStr = website.Split('_');
            switch (mobileStr[1])
            {
                case "BJ":
                    dic.Add("ProvinceID", "01");
                    dic.Add("ProvinceCode", "bj");
                    dic.Add("ProvinceName", "北京");
                    break;
                case "SH":
                    dic.Add("ProvinceID", "02");
                    dic.Add("ProvinceCode", "sh");
                    dic.Add("ProvinceName", "上海");
                    break;
                case "TJ":
                    dic.Add("ProvinceID", "03");
                    dic.Add("ProvinceCode", "tj");
                    dic.Add("ProvinceName", "天津");
                    break;
                case "CQ":
                    dic.Add("ProvinceID", "04");
                    dic.Add("ProvinceCode", "cq");
                    dic.Add("ProvinceName", "重庆");
                    break;
                case "SD":
                    dic.Add("ProvinceID", "16");
                    dic.Add("ProvinceCode", "sd");
                    dic.Add("ProvinceName", "山东");
                    break;
                //case "AH":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "ah");
                //    dic.Add("ProvinceName", "安徽");
                //    break;
                case "FJ":
                    dic.Add("ProvinceID", "14");
                    dic.Add("ProvinceCode", "fj");
                    dic.Add("ProvinceName", "福建");
                    break;
                //case "GS":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "gs");
                //    dic.Add("ProvinceName", "甘肃");
                //    break;
                case "GD":
                    dic.Add("ProvinceID", "20");
                    dic.Add("ProvinceCode", "gd");
                    dic.Add("ProvinceName", "广东");
                    break;
                //case "GX":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "gx");
                //    dic.Add("ProvinceName", "广西");
                //    break;
                case "GZ":
                    dic.Add("ProvinceID", "24");
                    dic.Add("ProvinceCode", "gz");
                    dic.Add("ProvinceName", "贵州");
                    break;
                //case "HI":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "hi");
                //    dic.Add("ProvinceName", "海南");
                //    break;
                //case "HE":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "he");
                //    dic.Add("ProvinceName", "河北");
                //    break;
                //case "HA":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "ha");
                //    dic.Add("ProvinceName", "河南");
                //    break;
                //case "HL":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "hl");
                //    dic.Add("ProvinceName", "黑龙江");
                //    break;
                case "HB":
                    dic.Add("ProvinceID", "18");
                    dic.Add("ProvinceCode", "hb");
                    dic.Add("ProvinceName", "湖北");
                    break;
                //case "HN":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "hn");
                //    dic.Add("ProvinceName", "湖南");
                //    break;
                //case "JL":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "jl");
                //    dic.Add("ProvinceName", "吉林");
                //    break;
                case "JS":
                    dic.Add("ProvinceID", "11");
                    dic.Add("ProvinceCode", "js");
                    dic.Add("ProvinceName", "江苏");
                    break;
                //case "JX":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "jx");
                //    dic.Add("ProvinceName", "江西");
                //    break;
                //case "LN":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "ln");
                //    dic.Add("ProvinceName", "辽宁");
                //    break;
                //case "NM":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "nm");
                //    dic.Add("ProvinceName", "内蒙古");
                //    break;
                //case "NX":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "nx");
                //    dic.Add("ProvinceName", "宁夏");
                //    break;
                //case "QH":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "qh");
                //    dic.Add("ProvinceName", "青海");
                //    break;
                //case "SX":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "sx");
                //    dic.Add("ProvinceName", "山西");
                //    break;
                //case "SN":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "sn");
                //    dic.Add("ProvinceName", "陕西");
                //    break;
                case "SC":
                    dic.Add("ProvinceID", "23");
                    dic.Add("ProvinceCode", "sc");
                    dic.Add("ProvinceName", "四川");
                    break;
                //case "XZ":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "xz");
                //    dic.Add("ProvinceName", "西藏");
                //    break;
                //case "XJ":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "xj");
                //    dic.Add("ProvinceName", "新疆");
                //    break;
                //case "YN":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "yn");
                //    dic.Add("ProvinceName", "云南");
                //    break;
                case "ZJ":
                    dic.Add("ProvinceID", "12");
                    dic.Add("ProvinceCode", "zj");
                    dic.Add("ProvinceName", "浙江");
                    break;
                //case "MO":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "mo");
                //    dic.Add("ProvinceName", "澳门");
                //    break;
                //case "HK":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "hk");
                //    dic.Add("ProvinceName", "香港");
                //    break;
                //case "TW":
                //    dic.Add("ProvinceID", "03");
                //    dic.Add("ProvinceCode", "tw");
                //    dic.Add("ProvinceName", "台湾");
                //    break;
                default: break;
            }
            return dic;
        }

    }
}
