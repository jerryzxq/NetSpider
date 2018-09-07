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
using Vcredit.NetSpider.Crawler;
using Vcredit.NetSpider.Entity.Service.ProvidentFund;
using Vcredit.NetSpider.Crawler.Social;

namespace Vcredit.NetSpider.Processor.Impl
{
    public class ProvidentFundExecutor : IProvidentFundExecutor
    {

        public VerCodeRes Init(string cityCode)
        {
            VerCodeRes Res = new VerCodeRes();
            try
            {
                ProvidentFundOpr Opr = new ProvidentFundOpr();
                IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
                string[] cityArr = cityCode.Split('_');

                Log4netAdapter.WriteInfo(cityCode + "，公积金页面初始化开始");
                switch (cityArr[0])
                {
                    case ServiceConsts.CityCode_Qingdao: Res = Opr.Qingdao_Init(); break;
                    case ServiceConsts.CityCode_Shanghai: Res = Opr.Shanghai_Init(); break;
                    case ServiceConsts.CityCode_Chengdu: Res = Opr.Chengdu_Init(); break;
                    case ServiceConsts.CityCode_Beijing: Res = Opr.Beijing_Init(); break;
                    case ServiceConsts.CityCode_Nanjing: Res = Opr.Nanjing_Init(); break;
                    case ServiceConsts.CityCode_Chongqing: Res = Opr.Chongqing_Init(); break;
                    case ServiceConsts.CityCode_Xiamen: Res = Opr.Xiamen_Init(); break;
                    case ServiceConsts.CityCode_Ningbo: Res = Opr.Ningbo_Init(); break;
                    case ServiceConsts.CityCode_Hangzhou: Res = Opr.Hangzhou_Init(); break;
                    case ServiceConsts.CityCode_Wuxi: Res = Opr.Wuxi_Init(); break;
                    case ServiceConsts.CityCode_Suzhou: Res = Opr.Suzhou_Init(); break;
                    default:
                        IProvidentFundCrawler crawler = CrawlerManager.GetProvidentFundCrawler(cityArr[0].ToUpper(), cityArr[1].ToLower());
                        if (crawler != null)
                        {
                            Res = crawler.ProvidentFundInit();
                        }
                        else
                        {
                            Res.Token = CommonFun.GetGuidID();
                            Res.StatusCode = ServiceConsts.StatusCode_success;
                            Res.StatusDescription = "所选城市无需初始化";
                        }
                        break;
                }
                Log4netAdapter.WriteInfo(cityCode + "，公积金页面初始化结束");

                Res.EndTime = DateTime.Now.ToString();
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "ProvidentFundExecutor报错";
            }
            return Res;
        }


        public ProvidentFundQueryRes GetProvidentFund(string cityCode, ProvidentFundReq fundReq)
        {
            ProvidentFundQueryRes Res = new ProvidentFundQueryRes();
            try
            {
                ProvidentFundOpr Opr = new ProvidentFundOpr();
                IPluginJsonParser jsonParser = PluginServiceManager.GetJsonParserPlugin();
                string[] cityArr = cityCode.Split('_');

                Log4netAdapter.WriteInfo(cityCode + "，公积金查询开始");
                string username = fundReq.Username;
                string password = fundReq.Password;
                string vercode = fundReq.Vercode;
                string token = fundReq.Token;
                switch (cityArr[0])
                {
                    case ServiceConsts.CityCode_Qingdao: Res = Opr.Qingdao_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Shanghai: Res = Opr.Shanghai_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Chengdu: Res = Opr.Chengdu_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Beijing: Res = Opr.Beijing_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Nanjing: Res = Opr.Nanjing_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Chongqing: Res = Opr.Chongqing_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Xiamen: Res = Opr.Xiamen_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Ningbo: Res = Opr.Ningbo_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Hangzhou: Res = Opr.Hangzhou_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Wuxi: Res = Opr.Wuxi_GetProvidentFund(username, password, token, vercode); break;
                    case ServiceConsts.CityCode_Suzhou: Res = Opr.Suzhou_GetProvidentFund(username, password, token, vercode); break;
                    default:
                        IProvidentFundCrawler crawler = CrawlerManager.GetProvidentFundCrawler(cityArr[0].ToUpper(), cityArr[1].ToLower());
                        if (crawler != null)
                        {
                            Res = crawler.ProvidentFundQuery(fundReq);
                        }
                        break;
                }
                Log4netAdapter.WriteInfo(cityCode + "，公积金查询结束");
                #region 分析数据
                if (Res.StatusCode == ServiceConsts.StatusCode_success)
                {
                    System.Text.RegularExpressions.Regex _regex_isbreak = new System.Text.RegularExpressions.Regex("/{0,}N{0,}/{1,}");//正则判断是否有断缴
                    #region 基本公积金
                    ////不判断是否本地户籍
                    //if (!Res.IdentityCard.IsEmpty())
                    //{
                    //    string NativeCity = ChinaIDCard.GetAddress(Res.IdentityCard.Substring(0, 4) + "00");//户籍城市
                    //    if (NativeCity.Contains(Res.ProvidentFundCity))
                    //    {
                    //        Res.IsLocal = true;
                    //    }
                    //}
                    //计算最近24个月的缴费情况
                    string ProvidentFundTime = string.Empty;
                    string Payment_State = string.Empty;
                    int PaymentMonths_Continuous = 0;
                    int maxmonth = 24;//20160316修改，最小抓25个月
                    bool hasback = false;//0至24-26个月是否有补缴
                    //判断现公司缴金情况
                    string recent_company = string.Empty;
                    int recent_company_month = 0;
                    bool recent_company_needadd = true;
                    for (int i = 0; i <= maxmonth; i++)//20160314修改，若最近2个月缴费情况不为正常则，提前2个月开始计算24个月的缴费情况
                    {
                        ProvidentFundTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                        var query = Res.ProvidentFundDetailList.Where(o => o.ProvidentFundTime == ProvidentFundTime && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Back).FirstOrDefault();
                        
                        if (query != null)
                        {
                            Payment_State += "N ";//缴费
                            if (query.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Back)
                            {
                                hasback = true;//判断为补缴
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
                    Res.PaymentMonths_Continuous = PaymentMonths_Continuous;
                    //最后一个月缴费
                    var lastDetail = Res.ProvidentFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal).OrderByDescending(o => o.ProvidentFundTime).FirstOrDefault();

                    if (lastDetail != null)
                    {
                        if (Res.SalaryBase == 0)
                        {
                            //var SalaryBaseList = Res.ProvidentFundDetailList.Where(o => o.ProvidentFundTime.Substring(0, 4) == DateTime.Now.AddYears(-1).Year.ToString()).Select(o => o.ProvidentFundBase).ToList();
                            ////上一年平均缴费基数
                            //if (SalaryBaseList.Count > 0)
                            //{
                            //    Res.SalaryBase = SalaryBaseList.Average();
                            //}
                            //var SalaryBase = Res.ProvidentFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal).OrderByDescending(o => o.ProvidentFundTime).Select(o => o.ProvidentFundBase).FirstOrDefault();
                            //if (SalaryBase != null)
                            //{
                            //    Res.SalaryBase = SalaryBase;
                            //}
                            Res.SalaryBase = lastDetail.ProvidentFundBase;
                        }
                        if (Res.CompanyName.IsEmpty())
                        {
                            Res.CompanyName = lastDetail.CompanyName;
                        }
                        Res.LastProvidentFundTime = lastDetail.ProvidentFundTime;
                    }
                    //实际缴保月数
                    if (Res.PaymentMonths == 0)
                    {
                        Res.PaymentMonths = Res.ProvidentFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal).Count();
                    }
                    //性别
                    if (Res.Sex.IsEmpty() && !Res.IdentityCard.IsEmpty())
                    {
                        Res.Sex = ChinaIDCard.GetSex(Res.IdentityCard);
                    }
                    //断缴、支取显示
                    if (Res.Description.IsEmpty())
                    {
                        if (Res.ProvidentFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Draw).FirstOrDefault() == null)
                        {
                            if (_regex_isbreak.IsMatch(Res.Payment_State.ToTrim()) || hasback)
                            {
                                Res.Description = "有断缴、补缴，请人工校验";
                            }
                        }
                        else
                        {
                            if (_regex_isbreak.IsMatch(Res.Payment_State.ToTrim()) || hasback)
                            {
                                Res.Description = "有断缴、补缴，有支取，请人工校验";
                            }
                            else
                            {
                                Res.Description = "有支取，请人工校验";
                            }
                        } 
                    }
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
                    #endregion


                    #region 补充公积金
                    //计算最近24个月的缴费情况
                    ProvidentFundTime = string.Empty;
                    Payment_State = string.Empty;
                    PaymentMonths_Continuous = 0;
                    maxmonth = 24;//20160316修改，最小抓25个月
                    hasback = false;
                    //判断现公司缴金情况
                    recent_company = string.Empty;
                    recent_company_month = 0;
                    recent_company_needadd = true;
                    for (int i = 0; i <= maxmonth; i++)//20160314修改，若最近2个月缴费情况不为正常则提前2个月开始计算24个月的缴费情况
                    {
                        ProvidentFundTime = DateTime.Now.AddMonths(-i).ToString(Consts.DateFormatString7);

                        var query = Res.ProvidentFundReserveRes.ProvidentReserveFundDetailList.Where(o => o.ProvidentFundTime == ProvidentFundTime && o.PaymentFlag == ServiceConsts.SocialSecurity_PaymentFlag_Normal || o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Back).FirstOrDefault();
                        
                        if (query != null)
                        {
                            Payment_State += "N ";//缴费
                            if (query.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Back)
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
                    Continuous = Payment_State.ToTrim().Split('/');
                    foreach (string item in Continuous)
                    {
                        if (!item.IsEmpty())
                        {
                            PaymentMonths_Continuous = item.Split('N').Count() - 1;
                            break;
                        }
                    }
                    Res.ProvidentFundReserveRes.Payment_State = Payment_State;
                    Res.ProvidentFundReserveRes.PaymentMonths_Continuous = PaymentMonths_Continuous;
                    //最后一个月缴费
                    lastDetail = Res.ProvidentFundReserveRes.ProvidentReserveFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal).OrderByDescending(o => o.ProvidentFundTime).FirstOrDefault();

                    if (lastDetail != null)
                    {
                        if (Res.ProvidentFundReserveRes.SalaryBase == 0)
                        {
                            Res.ProvidentFundReserveRes.SalaryBase = lastDetail.ProvidentFundBase;
                        }
                        Res.ProvidentFundReserveRes.LastProvidentFundTime = lastDetail.ProvidentFundTime;
                    }
                    //实际缴保月数
                    if (Res.ProvidentFundReserveRes.PaymentMonths == 0)
                    {
                        Res.ProvidentFundReserveRes.PaymentMonths = Res.ProvidentFundReserveRes.ProvidentReserveFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Normal).Count();
                    }
                    //断缴、支取显示
                    if (Res.ProvidentFundReserveRes.Description.IsEmpty())
                    {
                        if (Res.ProvidentFundReserveRes.ProvidentReserveFundDetailList.Where(o => o.PaymentFlag == ServiceConsts.ProvidentFund_PaymentFlag_Draw).FirstOrDefault() == null)
                        {
                            if (_regex_isbreak.IsMatch(Res.ProvidentFundReserveRes.Payment_State.ToTrim()) || hasback)
                            {
                                Res.ProvidentFundReserveRes.Description = "有断缴、补缴，请人工校验";
                            }
                        }
                        else
                        {
                            if (_regex_isbreak.IsMatch(Res.ProvidentFundReserveRes.Payment_State.ToTrim()) || hasback)
                            {
                                Res.ProvidentFundReserveRes.Description = "有断缴、补缴，有支取，请人工校验";
                            }
                            else
                            {
                                Res.ProvidentFundReserveRes.Description = "有支取，请人工校验";
                            }
                        } 
                    }
                    if ((recent_company_month < 6 && !recent_company.IsEmpty()) || Res.ProvidentFundReserveRes.Payment_State.ToArray().Count(e => e == 'N') < 6)
                    {
                        if (Res.ProvidentFundReserveRes.Description.IsEmpty())
                        {
                            Res.ProvidentFundReserveRes.Description = "现单位缴金不足6个月，请人工校验";
                        }
                        else
                        {
                            Res.ProvidentFundReserveRes.Description = "现单位缴金不足6个月，" + Res.ProvidentFundReserveRes.Description;
                        }
                    }
                    #endregion


                    #region 贷款
                    if (Res.ProvidentFundLoanRes.Name.IsEmpty())//姓名
                    {
                        Res.ProvidentFundLoanRes.Name = Res.Name;
                    }
                    if (Res.ProvidentFundLoanRes.IdentityCard.IsEmpty())//身份证
                    {
                        Res.ProvidentFundLoanRes.IdentityCard = Res.IdentityCard;
                    }
                    if (Res.ProvidentFundLoanRes.Phone.IsEmpty())//电话
                    {
                        Res.ProvidentFundLoanRes.Phone = Res.Phone;
                    }

                    if (Res.ProvidentFundLoanRes.Principal_Payed == 0)//已还本金
                    {
                        Res.ProvidentFundLoanRes.Principal_Payed = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Sum(o => o.Principal + o.Overdue_Principal);
                    }
                    if (Res.ProvidentFundLoanRes.Interest_Payed == 0)//已还利息
                    {
                        Res.ProvidentFundLoanRes.Interest_Payed = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Sum(o => o.Interest + o.Overdue_Interest);
                    }
                    if (Res.ProvidentFundLoanRes.Overdue_Interest_Cal == 0)//逾期利息（计算）
                    {
                        Res.ProvidentFundLoanRes.Overdue_Interest_Cal = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Sum(o => o.Overdue_Interest);
                    }
                    if (Res.ProvidentFundLoanRes.Overdue_Principal_Cal == 0)//逾期本金（计算）
                    {
                        Res.ProvidentFundLoanRes.Overdue_Principal_Cal = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Sum(o => o.Overdue_Principal);
                    }
                    if (Res.ProvidentFundLoanRes.Overdue_Period_Cal == 0)//逾期期数（计算）
                    {
                        Res.ProvidentFundLoanRes.Overdue_Period_Cal = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Where(o => o.Overdue_Principal != 0 || o.Overdue_Interest != 0).Count();
                    }
                    if (Res.ProvidentFundLoanRes.Interest_Penalty == 0)//罚息
                    {
                        Res.ProvidentFundLoanRes.Interest_Penalty = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Sum(o => o.Interest_Penalty);
                    }
                    if (Res.ProvidentFundLoanRes.Period_Payed == 0)//已还期数
                    {
                        Res.ProvidentFundLoanRes.Period_Payed = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Where(o => (o.Principal != 0 || o.Interest != 0 || o.Overdue_Principal != 0 || o.Overdue_Interest != 0) && (!o.Record_Period.IsEmpty() || !o.Record_Month.IsEmpty())).Count();
                    }


                    //最后一个月还款
                    ProvidentFundLoanDetail lastLoanDetail = Res.ProvidentFundLoanRes.ProvidentFundLoanDetailList.Where(o => o.Principal != 0 || o.Interest != 0 || o.Overdue_Principal != 0 || o.Overdue_Interest != 0).FirstOrDefault();
                    if (lastLoanDetail != null)
                    {
                        if (Res.ProvidentFundLoanRes.LatestRepayTime.IsEmpty())//末次还款时间
                        {
                            Res.ProvidentFundLoanRes.LatestRepayTime = lastLoanDetail.Record_Month;
                        }
                    }
                    //逾期、罚息显示
                    if (Res.ProvidentFundLoanRes.Description.IsEmpty())
                    {
                        if (Res.ProvidentFundLoanRes.Overdue_Period_Cal != 0 || Res.ProvidentFundLoanRes.Overdue_Period != 0)
                        {
                            Res.ProvidentFundLoanRes.Description = "有逾期，";
                        }
                        if (Res.ProvidentFundLoanRes.Interest_Penalty != 0)
                        {
                            Res.ProvidentFundLoanRes.Description += "有罚息，";
                        }
                        if (!Res.ProvidentFundLoanRes.Description.IsEmpty())
                        {
                            Res.ProvidentFundLoanRes.Description += "请人工校验";
                        }
                    }

                    #endregion
                }
                #endregion

                //if (Res.StatusCode == ServiceConsts.StatusCode_success)
                //{
                //    System.Threading.Tasks.Task.Factory.StartNew(() =>
                //    {
                //        try
                //        {
                //            IProvidentFund service = NetSpiderFactoryManager.GetProvidentFundService();
                //            ProvidentFundEntity entity = jsonParser.DeserializeObject<ProvidentFundEntity>(jsonParser.SerializeObject(Res));
                //            entity.Loginname = username;
                //            entity.Password = password;
                //            service.Save(entity);
                //        }
                //        catch (Exception e)
                //        {
                //            Log4netAdapter.WriteError("保存公积金数据异常", e);
                //        }
                //    });
                //}
            }
            catch (Exception e)
            {
                Res.StatusCode = ServiceConsts.StatusCode_error;
                Res.StatusDescription = "ProvidentFundQueryRes报错";
            }
            return Res;
        }
    }
}
