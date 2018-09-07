using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aspose.Words;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.NetSpider.Entity;
using Vcredit.Common.Constants;

namespace Vcredit.WeiXin.RestService.Operation
{
    public class MakeContract
    {
        public MakeContract()
        { }
        /// <summary>
        /// 生成pdf
        /// </summary>
        /// <param name="wordFilePath">word路径</param>
        /// <param name="pdfFilaPath">生成pdf路径</param>
        /// <param name="contractEntity">传入实体</param>
        public void FillWordContent(string wordFilePath, string pdfFilaPath, ContractEntity contractEntity)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("Name", contractEntity.Name);
            dic.Add("IdentityCard", contractEntity.IdentityCard);
            dic.Add("CreditCardNo", contractEntity.CreditCardNo);
            dic.Add("CreditCardBankName", contractEntity.CreditCardBankName);
            dic.Add("WeChatId", contractEntity.WeChatId);

            dic.Add("LoanAmount", contractEntity.LoanAmount);
            dic.Add("LoanPeriod", (contractEntity.Type == "day" ? (contractEntity.ApproveDay.IsEmpty() ? "0" : contractEntity.ApproveDay) : contractEntity.LoanPeriod));
            dic.Add("LoanPeriodType", (contractEntity.Type == "day" ? "天" : "期"));
            dic.Add("MonthlyRepay", (contractEntity.Type == "day" ? (contractEntity.LoanAmount.ToDecimal(0) + contractEntity.LoanAmount.ToDecimal(0) * 0.85M / 100 * 7 / 30).ToString("N") : contractEntity.MonthlyRepay));
            dic.Add("ApplyAmount", contractEntity.ApplyAmount.IsEmpty() ? "0" : contractEntity.ApplyAmount);
            dic.Add("MonthlyBaseAndInterestPmt", (contractEntity.Type == "day" ? contractEntity.ApplyAmount : contractEntity.MonthlyBaseAndInterestPmt));
            dic.Add("YearInterestRate", contractEntity.YearInterestRate);
            dic.Add("MonthlyServiceRate", contractEntity.MonthlyServiceRate);
            dic.Add("CashCardBankName", contractEntity.CashCardBankName);
            dic.Add("CashCardNo", contractEntity.CashCardNo);
            dic.Add("ChangeCardBankName", (contractEntity.ChangeCardBankName.IsEmpty() ? "" : contractEntity.ChangeCardBankName));
            dic.Add("ChangeCardNo", (contractEntity.ChangeCardNo.IsEmpty() ? "" : contractEntity.ChangeCardNo));
            dic.Add("Phone", contractEntity.Phone);
            dic.Add("ContractNo", contractEntity.ContractNo);
            dic.Add("ConsultingServiceTime", contractEntity.ConsultingServiceTime.ToString(Consts.DateFormatString9));
            dic.Add("DeductTime", contractEntity.DeductTime.ToString(Consts.DateFormatString9));
            dic.Add("DeductTime1", contractEntity.DeductTime.ToString(Consts.DateFormatString4));
            dic.Add("LoanTime", contractEntity.LoanTime.ToString(Consts.DateFormatString9));
            dic.Add("PromptTime", contractEntity.PromptTime.ToString(Consts.DateFormatString9));
            dic.Add("PromptTime1", contractEntity.PromptTime.ToString(Consts.DateFormatString4));
            dic.Add("ServiceTime", contractEntity.ServiceTime.ToString(Consts.DateFormatString9));
            dic.Add("SignTime", contractEntity.SignTime.ToString(Consts.DateFormatString4));//签约时间
            dic.Add("ImageSignature", contractEntity.Signature.IsEmpty() ? contractEntity.Name : contractEntity.Signature);//签名
            if (!contractEntity.ClientAddress.IsEmpty())
            {
                dic.Add("ClientAddress", contractEntity.ClientAddress);
            }

            dic.Add("AmerceRate", contractEntity.AmerceRate);
            dic.Add("PayForB", contractEntity.PayForB);
            dic.Add("PayForD", contractEntity.PayForD);
            dic.Add("PayForBandD", contractEntity.PayForBandD);

            dic.Add("deductYear", contractEntity.DeductTime.Year.ToString());
            dic.Add("deductMonth", contractEntity.DeductTime.Month.ToString());
            dic.Add("deductDay", contractEntity.DeductTime.Day.ToString());

            dic.Add("LoanYear", contractEntity.LoanTime.Year.ToString());
            dic.Add("LoanMonth", contractEntity.LoanTime.Month.ToString());
            dic.Add("LoanDay", contractEntity.LoanTime.Day.ToString());

            dic.Add("MonthlyTxt", (contractEntity.Type == "day" ? "到期" : "每月"));
            dic.Add("DayTxt", (contractEntity.Type == "day" ? "您在还款日时的应付金额为" + contractEntity.ApplyAmount + "元，还款日为：放款后的第 " + contractEntity.ApproveDay + " 天（放款日次日视为放款后的第1天）。" : "您的每月应付金额共计为_" + contractEntity.MonthlyRepay + "_元，从借款发放的下月起，请在每月还款日将还款存入合同约定账户。还款日为每月与放款日对应的日期的前一日；若放款日晚于当月27日，则还款日为每月26日。"));

            //dic.Add("ContractRegionCourt", contractEntity.ContractRegionCourt);
            //dic.Add("Serve", contractEntity.Serve);
            //dic.Add("ServeAddress", contractEntity.ServeAddress);
            //dic.Add("City", contractEntity.City);

            #region 通过计算得到的参数
            int LoanPeriod = contractEntity.LoanPeriod.ToInt(0);
            decimal LoanAmount = contractEntity.LoanAmount.ToDecimal(0);//贷款金额
            decimal MonthlyServiceRate = contractEntity.MonthlyServiceRate.ToDecimal(0);//月服务费率(百分制)
            decimal FormalitiesRate = contractEntity.FormalitiesRate.ToDecimal(0);//手续费率(百分制)
            decimal FormalitiesAmt = (LoanAmount * FormalitiesRate) / 100;//手续费
            decimal LoanAmountToCreditCard = LoanAmount - FormalitiesAmt;//打款到信用卡金额
            decimal KSAmt = LoanAmount * (decimal)0.005;//扣失费
            if (KSAmt < 100)
            {
                KSAmt = 100;
            }
            else if (KSAmt > 200)
            {
                KSAmt = 200;
            }

            decimal MonthlyInterestRate = contractEntity.Type == "day" ? 0.85M : contractEntity.YearInterestRate.ToDecimal(0) / 12;//月利率
            decimal MonthlyService = MonthlyServiceRate * LoanAmount / 100;//月服务费
            decimal ServiceRate = contractEntity.Type == "day" ? 0 : MonthlyServiceRate * LoanPeriod;//总服务费率
            decimal ServiceAmt = (ServiceRate * LoanAmount) / 100;//总服务费

            dic.Add("FormalitiesRate", FormalitiesRate.ToString());
            dic.Add("FormalitiesAmt", FormalitiesAmt.ToString("N"));
            dic.Add("FormalitiesAmtChinese", MoneyToChinese.FromMoneyGetChinese(FormalitiesAmt));
            dic.Add("LoanAmountToCreditCard", LoanAmountToCreditCard.ToString("N"));
            dic.Add("LoanAmountToCreditCardChinese", MoneyToChinese.FromMoneyGetChinese(LoanAmountToCreditCard));
            dic.Add("LoanAmountChinese", MoneyToChinese.FromMoneyGetChinese(LoanAmount));
            dic.Add("KSAmt", KSAmt.ToString("N"));
            dic.Add("MonthlyInterestRate", MonthlyInterestRate.ToString());
            dic.Add("InterestRate", contractEntity.Type == "day" ? "0" : MonthlyInterestRate.ToString());
            dic.Add("MonthlyServiceAmt", (contractEntity.Type == "day" ? 0 : MonthlyService).ToString("N"));
            dic.Add("MonthlyServiceAmtChinese", MoneyToChinese.FromMoneyGetChinese(MonthlyService));
            dic.Add("MonthlyRepayChinese", MoneyToChinese.FromMoneyGetChinese((contractEntity.Type == "day" ? (contractEntity.ApplyAmount.IsEmpty() ? "0" : contractEntity.ApplyAmount) : contractEntity.MonthlyRepay).ToDecimal(0)));
            dic.Add("ServiceRate", ServiceRate.ToString());
            dic.Add("ServiceAmt", ServiceAmt.ToString("N"));

            dic.Add("PayForBUpper", MoneyToChinese.FromMoneyGetChinese(contractEntity.PayForB.ToDecimal(0)));
            dic.Add("PayForDUpper", MoneyToChinese.FromMoneyGetChinese(contractEntity.PayForD.ToDecimal(0)));
            dic.Add("PayForBandDUpper", MoneyToChinese.FromMoneyGetChinese(contractEntity.PayForBandD.ToDecimal(0)));

            dic.Add("city", contractEntity.City);  //工作所在城市
            dic.Add("Education", contractEntity.Education == null ? "" : contractEntity.Education); //学历
            dic.Add("BirthDate", Convert.ToDateTime(contractEntity.IdentityCard.Substring(6, 8).Insert(4, "-").Insert(7, "-")).ToString(Consts.DateFormatString4));//出生日期
            dic.Add("IncomeMonth", contractEntity.IncomeMonth == null ? "0.00" : contractEntity.IncomeMonth); //月收入
            dic.Add("Marriage", contractEntity.Marriage == null ? "" : contractEntity.Marriage);//婚姻状况
            #endregion
            FillWordContent(wordFilePath, pdfFilaPath, dic);
        }
        public void FillWordContent(string wordFilePath, string pdfFilaPath, Dictionary<string, string> dic)
        {
            Document doc = new Document(wordFilePath);
            DocumentBuilder builder = new DocumentBuilder(doc);
            foreach (var key in dic.Keys)
            {
                try
                {
                    var repStr = string.Format("&{0}&", key);
                    int th = doc.Range.Replace(repStr, dic[key], false, false);//获取替换的数量
                    //如果替换的数量为0按照书签插入值（有些字段太短，用替换会影响样式，改用书签）
                    if (th == 0)
                    {
                        if (key.StartsWith("Image"))
                        {
                            try
                            {
                                //string path = AppDomain.CurrentDomain.BaseDirectory + "Images\\" + dic[key];
                                if (builder.MoveToBookmark(key))
                                {
                                    //Byte[] image = GetBytesByImagePath(path);
                                    Byte[] image = Convert.FromBase64String(dic[key]);
                                    builder.InsertImage(image, 70, 30);
                                }
                            }
                            catch
                            {
                                Bookmark bookmark = doc.Range.Bookmarks[key];
                                if (bookmark != null)
                                {
                                    bookmark.Text = dic[key];
                                }
                            }
                        }
                        else
                        {
                            Bookmark bookmark = doc.Range.Bookmarks[key];
                            if (bookmark != null)
                            {
                                bookmark.Text = dic[key];
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(String.Format("合同生成错误：关键字：{0}，错误信息：{1}", key, ex.Message));
                }
            }
            try
            {
                doc.Save(pdfFilaPath, SaveFormat.Pdf);
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("合同生成错误：转化为PDF错误，错误信息：{0}", ex.Message));
            }

        }

        public static byte[] GetBytesByImagePath(string strFile)
        {
            byte[] photo_byte = null;
            using (System.IO.FileStream fs = new System.IO.FileStream(strFile, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                using (System.IO.BinaryReader br = new System.IO.BinaryReader(fs))
                {
                    photo_byte = br.ReadBytes((int)fs.Length);
                }
            }

            return photo_byte;
        }

    }

}
