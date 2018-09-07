using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Entity.Service;
using Vcredit.NetSpider.PluginManager;
using Vcredit.NetSpider.Processor.Operation;
using Vcredit.NetSpider.Service;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;
using Vcredit.NetSpider.Crawler.Social;
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Entity.Service.SocialSecurity;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class SociaSecurityExecutor : ISociaSecurityExecutor
    {
        public SocialSecurityQueryRes GetSocialSecurity(string cityCode, SocialSecurityReq socialReq)
        {
            SocialSecurityQueryRes Res = new SocialSecurityQueryRes();
            SociaSecurityOpr Opr = new SociaSecurityOpr();
            IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
            string[] cityArr = cityCode.Split('_');

            string token = socialReq.Token;
            string username = socialReq.Username;
            string password = socialReq.Password;
            string vercode = socialReq.Vercode;
            Log4netAdapter.WriteInfo(cityCode + "，社保查询开始");
            switch (cityArr[0])
            {
                case ServiceConsts.CityCode_Shanghai: Res = Opr.Shanghai_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Qingdao: Res = Opr.Qingdao_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Chengdu: Res = Opr.Chengdu_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Shenzhen: Res = Opr.Shenzhen_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Beijing: Res = Opr.Beijing_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Nanjing: Res = Opr.Nanjing_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Guangzhou: Res = Opr.Guangzhou_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Chongqing: Res = Opr.Chongqing_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Xiamen: Res = Opr.Xiamen_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Ningbo: Res = Opr.Ningbo_GetSocialSecurity(username, password); break;
                case ServiceConsts.CityCode_Hangzhou: Res = Opr.Hangzhou_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Wuxi: Res = Opr.Wuxi_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Suzhou: Res = Opr.Suzhou_GetSocialSecurity(username, password, token, vercode); break;
                case ServiceConsts.CityCode_Dalian: Res = Opr.Dalian_GetSocialSecurity(username, password); break;
                case ServiceConsts.CityCode_Fuzhou: Res = Opr.Fuzhou_GetSocialSecurity(username, password, token, vercode); break;
                default:
                    ISocialSecurityCrawler crawler = null;
                    if (cityArr.Length == 2)
                    {
                        crawler = CrawlerManager.GetSocialSecurityCrawler(cityArr[0].ToUpper(), cityArr[1].ToLower());
                    }
                    if (crawler != null)
                    {
                        Res = crawler.SocialSecurityQuery(socialReq);
                    }
                    break;
            }
            Log4netAdapter.WriteInfo(cityCode + "，社保查询结束");

            #region 分析数据
            if (Res.StatusCode == ServiceConsts.StatusCode_success)
            {
                ////不判断是否本地户籍
                //if (!Res.IdentityCard.IsEmpty())
                //{
                //    string NativeCity = ChinaIDCard.GetAddress(Res.IdentityCard.Substring(0, 4) + "00");//户籍城市
                //    if (NativeCity.Contains(Res.SocialSecurityCity))
                //    {
                //        Res.IsLocal = true;
                //    }
                //}

                int CalType = 0;//月数相关统计按照保险类型分优先级（养老>医疗>失业>工伤>生育）
                if (Res.Details.Where(o => o.PensionAmount != 0 || o.CompanyPensionAmount != 0).FirstOrDefault() != null)
                {
                    CalType = 1;//养老
                }
                else if (Res.Details.Where(o => o.MedicalAmount != 0 || o.CompanyMedicalAmount != 0).FirstOrDefault() != null)
                {
                    CalType = 2;
                }
                else if (Res.Details.Where(o => o.UnemployAmount != 0).FirstOrDefault() != null)
                {
                    CalType = 3;
                }
                else if (Res.Details.Where(o => o.EmploymentInjuryAmount != 0).FirstOrDefault() != null)
                {
                    CalType = 4;
                }
                else if (Res.Details.Where(o => o.MaternityAmount != 0).FirstOrDefault() != null)
                {
                    CalType = 5;
                }
                bool hasback = false;
                //如果该城市未独立计算（沈阳/上海/淄博社保独立计算）
                string recent_company = string.Empty;
                int recent_company_month = 0;
                bool recent_company_needadd = true;
                if (string.IsNullOrEmpty(Res.Payment_State))
                {
                    //计算最近24个月的缴费情况
                    string SocialInsuranceTime = string.Empty;
                    string Payment_State = string.Empty;
                    int PaymentMonths_Continuous = 0;
                    int maxmonth = 24;//20160316修改，最小抓25个月
                    //判断现公司缴金情况
                    for (int i = 0; i <= maxmonth; i++)//20160314修改，若最近2个月缴费情况不为正常则，提前2个月开始计算24个月的缴费情况
                    {
                        SocialInsuranceTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                        SocialSecurityDetailQueryRes query = new SocialSecurityDetailQueryRes();
                        switch (CalType)
                        {
                            case 1:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back) && (o.PensionAmount != 0 || o.CompanyPensionAmount != 0)).FirstOrDefault();
                                break;
                            case 2:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back) && (o.MedicalAmount != 0 || o.CompanyMedicalAmount != 0)).FirstOrDefault();
                                break;
                            case 3:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back) && o.UnemployAmount != 0).FirstOrDefault();
                                break;
                            case 4:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back) && o.EmploymentInjuryAmount != 0).FirstOrDefault();
                                break;
                            case 5:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back) && o.MaternityAmount != 0).FirstOrDefault();
                                break;
                            default:
                                query = Res.Details.Where(o => o.SocialInsuranceTime == SocialInsuranceTime && (o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back)).FirstOrDefault();
                                break;
                        }
                        if (query != null)
                        {
                            Payment_State += "N ";//缴费

                            if (query.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Back)
                            {
                                hasback = true;
                            }
                            if (recent_company_needadd)
                            {
                                if (recent_company.IsEmpty())
                                {
                                    recent_company = query.CompanyName;
                                    recent_company_month++;
                                }
                                else
                                {
                                    if (query.CompanyName == recent_company)
                                    {
                                        recent_company_month++;
                                    }
                                    else
                                    {
                                        recent_company_needadd = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (i == 0)
                            {
                                maxmonth = 25;//如果最近一个月不正常则往前推1个月
                            }
                            else if (i == 1)
                            {
                                if (Payment_State != "N ")
                                {
                                    maxmonth = 26;//如果最近两个月不正常则往前推2个月
                                }
                                else
                                {
                                    Payment_State += "/ ";//未缴费
                                }
                            }
                            else
                            {
                                Payment_State += "/ ";//未缴费
                            }
                        }
                    }
                    //最近24个月连续缴费情况
                    var Continuous = Payment_State.ToTrim().Split('/');
                    foreach (string item in Continuous)
                    {
                        if (!item.IsEmpty())
                        {
                            PaymentMonths_Continuous = item.Split('N').Count() - 1;
                            break;
                        }
                    }
                    Res.Payment_State = Payment_State;
                    if (Res.PaymentMonths_Continuous == 0)
                    {
                        Res.PaymentMonths_Continuous = PaymentMonths_Continuous;
                    }
                    else if (Res.PaymentMonths_Continuous < 0)
                    {
                        PaymentMonths_Continuous = 0;
                    }
                }
                //最后一个月缴费
                var lastDetail = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && o.SocialInsuranceBase != 0).OrderByDescending(o => o.SocialInsuranceTime).FirstOrDefault();

                if (lastDetail != null)
                {
                    //上一年平均缴费基数
                    if (Res.SocialInsuranceBase == 0)
                    {
                        var SocialInsuranceBaseList = Res.Details.Where(o => o.SocialInsuranceTime != null && o.SocialInsuranceTime.Substring(0, 4) == DateTime.Now.AddYears(-1).Year.ToString() && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).Select(o => o.SocialInsuranceBase).ToList();
                        if (SocialInsuranceBaseList.Count > 0)
                        {
                            Res.SocialInsuranceBase = SocialInsuranceBaseList.Average();
                        }
                        var SocialInsurancebase = Res.Details.Where(o => o.SocialInsuranceTime != null && o.SocialInsuranceTime.Substring(0, 4) == DateTime.Now.AddYears(-1).Year.ToString() && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).OrderByDescending(o => o.SocialInsuranceTime).Select(o => o.SocialInsuranceBase).FirstOrDefault();
                        if (SocialInsurancebase != null)
                        {
                            Res.SocialInsuranceBase = SocialInsurancebase;
                        }

                        Res.SocialInsuranceBase = lastDetail.SocialInsuranceBase;
                    }
                    else if (Res.SocialInsuranceBase < 0)//如果主表中缴费基数小于0，则判断为0
                    {
                        Res.SocialInsuranceBase = 0;
                    }
                    if (Res.CompanyName.IsEmpty())
                    {
                        Res.CompanyName = lastDetail.CompanyName;
                    }
                }
                //实际缴保月数
                if (Res.PaymentMonths == 0)
                {
                    switch (CalType)
                    {
                        case 1:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && (o.PensionAmount != 0 || o.CompanyPensionAmount != 0)).Count();
                            break;
                        case 2:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && (o.MedicalAmount != 0 || o.CompanyMedicalAmount != 0)).Count();
                            break;
                        case 3:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && o.UnemployAmount != 0).Count();
                            break;
                        case 4:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && o.EmploymentInjuryAmount != 0).Count();
                            break;
                        case 5:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal && o.MaternityAmount != 0).Count();
                            break;
                        default:
                            Res.PaymentMonths = Res.Details.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).Count();
                            break;
                    }
                }
                //性别
                if (Res.Sex.IsEmpty() && !Res.IdentityCard.IsEmpty())
                {
                    Res.Sex = ChinaIDCard.GetSex(Res.IdentityCard);
                }
                //出生日期
                if (Res.BirthDate.IsEmpty() && !Res.IdentityCard.IsEmpty())
                {
                    Res.BirthDate = ChinaIDCard.GetBirthDate(Res.IdentityCard);
                    try
                    {
                        int.Parse(Res.BirthDate.ToTrim("-"));
                    }
                    catch
                    {
                        Res.BirthDate = string.Empty;
                    }
                }
                System.Text.RegularExpressions.Regex _regex_isbreak = new System.Text.RegularExpressions.Regex("/{0,}N{0,}/{1,}");//正则判断是否有断缴
                //断缴、支取显示
                if (Res.Description.IsEmpty())
                {
                    if (_regex_isbreak.IsMatch(Res.Payment_State.ToTrim()) || hasback)
                    {
                        Res.Description = "有断缴、补缴，请人工校验";
                    }
                }
                //int a = Res.Payment_State.ToArray().Count(e => e == '/');
                if ((recent_company_month < 6 && !recent_company.IsEmpty()) || Res.Payment_State.ToArray().Count(e => e == 'N') < 6)
                {
                    if (Res.Description.IsEmpty())
                    {
                        Res.Description = "现单位缴金不足6个月，请人工校验";
                    }
                    else
                    {
                        Res.Description = "现单位缴金不足6个月，" + Res.Description;
                    }
                }

            }
            #endregion
            //if (Res.StatusCode == ServiceConsts.StatusCode_success)
            //{
            //    System.Threading.Tasks.Task.Factory.StartNew(() =>
            //    {
            //        try
            //        {
            //            ISocialSecurity service = NetSpiderFactoryManager.GetSocialSecurityService();
            //            SocialSecurityEntity entity = jsonParser.DeserializeObject<SocialSecurityEntity>(jsonParser.SerializeObject(Res));
            //            entity.Loginname = username;
            //            entity.Password = password;
            //            service.Save(entity);
            //        }
            //        catch (Exception e)
            //        {
            //            Log4netAdapter.WriteError("保存社保数据异常", e);
            //        }
            //    });
            //}

            return Res;
        }

        public VerCodeRes Init(string cityCode)
        {
            VerCodeRes Res = new VerCodeRes();
            SociaSecurityOpr Opr = new SociaSecurityOpr();
            IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();

            string[] cityArr = cityCode.Split('_');
            Log4netAdapter.WriteInfo(cityCode + "，社保页面初始化开始");
            switch (cityArr[0])
            {
                case ServiceConsts.CityCode_Shanghai: Res = Opr.Shanghai_Init(); break;
                case ServiceConsts.CityCode_Qingdao: Res = Opr.Qingdao_Init(); break;
                case ServiceConsts.CityCode_Chengdu: Res = Opr.Chengdu_Init(); break;
                case ServiceConsts.CityCode_Shenzhen: Res = Opr.Shenzhen_Init(); break;
                case ServiceConsts.CityCode_Beijing: Res = Opr.Beijing_Init(); break;
                case ServiceConsts.CityCode_Nanjing: Res = Opr.Nanjing_Init(); break;
                case ServiceConsts.CityCode_Guangzhou: Res = Opr.Guangzhou_Init(); break;
                case ServiceConsts.CityCode_Chongqing: Res = Opr.Chongqing_Init(); break;
                case ServiceConsts.CityCode_Xiamen: Res = Opr.Xiamen_Init(); break;
                case ServiceConsts.CityCode_Wuxi: Res = Opr.Wuxi_Init(); break;
                case ServiceConsts.CityCode_Suzhou: Res = Opr.Suzhou_Init(); break;
                case ServiceConsts.CityCode_Fuzhou: Res = Opr.Fuzhou_Init(); break;
                case ServiceConsts.CityCode_Hangzhou: Res = Opr.Hangzhou_Init(); break;
                default:
                    ISocialSecurityCrawler crawler = null;
                    if (cityArr.Length == 2)
                    {
                        crawler = CrawlerManager.GetSocialSecurityCrawler(cityArr[0].ToUpper(), cityArr[1].ToLower());
                    }
                    if (crawler != null)
                    {
                        Res = crawler.SocialSecurityInit();
                    }
                    else
                    {
                        Res.Token = CommonFun.GetGuidID();
                        Res.StatusCode = ServiceConsts.StatusCode_success;
                        Res.StatusDescription = "所选城市无需初始化";
                    }
                    break;
            }
            Log4netAdapter.WriteInfo(cityCode + "，社保页面初始化结束");

            return Res;
        }
    }
}
