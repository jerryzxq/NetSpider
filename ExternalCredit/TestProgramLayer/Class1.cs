using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Ext;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
//using Microsoft.Office.Interop.Excel;
//using DataTable = System.Data.DataTable;
using Vcredit.ExternalCredit.CrawlerLayer.CreditVariable;
using Aspose.Cells;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml;
using ServiceStack.OrmLite;
using Newtonsoft.Json;

namespace TestProgramLayer
{
    class BaseCompliance
    {
        public string BirthDay { get; set; }
    }
    class ComplianceQueryReqParam : BaseCompliance
    {

        public int ApplyID { get; set; }
        public string Name { get; set; }
        public string IdentityNo { get; set; }

    }
    class ComplianceQueryResParam
    {
        public bool IsSeccess { get; set; }
        public string Message { get; set; }
        public bool IsSignature { get; set; }
        public string SignatureTime { get; set; }
    }
    class ComplianceApplyResParam
    {
        public bool IsSeccess { get; set; }
        public string Message { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string IdentityNo { get; set; }
    }
    class MydataRow : IEqualityComparer<DataRow>
    {

        public bool Equals(DataRow x, DataRow y)
        {
            if (x[2].ToString() == y[2].ToString() && x[3].ToString().Replace(" ", "") == y[3].ToString().Replace(" ", ""))
            {
                return true;
            }
            return false;
        }

        public int GetHashCode(DataRow obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
    class TestCompliance
    {
        static string PostDataToUrl(string url, string data)
        {
            HttpItem httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Encoding = Encoding.UTF8,
                Postdata = data,
                ContentType = "application/json",
                PostEncoding = Encoding.UTF8

            };
            HttpResult httpResult = new HttpHelper().GetHtml(httpItem);
            return httpResult.Html;
        }
        public void Test()
        {
            ComplianceQueryReqParam param = new ComplianceQueryReqParam()
            {
                ApplyID = 2,
                IdentityNo = "1111",
                Name = "测试"

            };
            // var result = PostDataToUrl("http://10.138.60.115:8099/Compliance/Apply", JsonConvert.SerializeObject(param));
            var result = PostDataToUrl("http://10.138.60.115:8099/Compliance/Query", JsonConvert.SerializeObject(param));
            Log4netAdapter.WriteInfo("合规接口返回的信息：" + result);
            var entity = JsonConvert.DeserializeObject<ComplianceQueryResParam>(result.Trim('\"').Replace("\\", ""));

            if (entity.IsSeccess)
            {
                if (entity.IsSignature)
                {

                }
                else
                {
                    Log4netAdapter.WriteInfo("还未签名不能查征");
                }
            }
            else
            {
                Log4netAdapter.WriteInfo("程序报错：" + entity.Message);

            }
        }
        public void TestApply()
        {
            var json = File.ReadAllText(@"D:\vcData\91UU\00100101142\RecvFile\张辉_100100923\测试数据(1).txt", Encoding.GetEncoding("GBK"));
            var req = JsonConvert.DeserializeObject<ComplianceApplyResParam>(json);
            var result = PostDataToUrl("http://10.138.60.115:8099/Compliance/Apply", json);
            var entity = JsonConvert.DeserializeObject<ComplianceApplyResParam>(result);
            if (entity.IsSeccess)
            {
                Log4netAdapter.WriteInfo("申请接口返回的参数：" + result);
            }
            else
            {
                Log4netAdapter.WriteInfo("程序报错：" + entity.Message);

            }
        }
    }
    class TestExtTradeData
    {
        public static void AnalyextTradeHtml()
        {
            Analisis a = new Analisis();
            DirectoryInfo dic = new DirectoryInfo(@"D:\征信报告文档\新外贸\htmltest\html文件");
            foreach (var item in dic.GetFiles())
            {
                try
                {
                    a.SaveData(item.Name, File.ReadAllText(item.FullName, Encoding.GetEncoding("GBK")));
                }
                catch
                {

                }

            }
        }
        public static void compare()
        {
            BaseDao dao = new BaseDao();
            var result = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 1341);
            var group = result.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == 10).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == 11).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        static void comparetoenttiy<T>(T entity1, T entity2)
        {
            var pros = entity1.GetType().GetProperties();
            var pros1 = entity2.GetType().GetProperties();
            var className = entity1.GetType().Name;
            var className1 = entity2.GetType().Name;
            object reportid = entity1.GetPropertyValue("Report_Id");
            object reportid1 = entity2.GetPropertyValue("Report_Id");
            for (int i = 0; i < pros.Length; i++)
            {
                if (pros[i].Name.Contains("_Id") || pros[i].Name == "Time_Stamp" || pros[i].Name == "TIMESTAMP")
                    continue;
                if (pros[i].Name == pros1[i].Name)
                {
                    if ((pros[i].GetValue(entity1) ?? string.Empty).ToString() == (pros[i].GetValue(entity2) ?? string.Empty).ToString())
                    {

                    }
                    else
                    {
                        Console.WriteLine(reportid.ToString() + "表：" + className + "属性" + pros[i].Name + "的值" + pros[i].GetValue(entity1) + ",不等于" + reportid1.ToString() + "表：" + className1 + "属性" + pros1[i].Name + "的值" + pros1[i].GetValue(entity2));
                    }
                }
            }
        }

        public static void compareCAAndSC()
        {
            BaseDao dao = new BaseDao();
            BaseDao scdao = new BaseDao("ExternalTradeDB1");
            //SqlExpression<CRD_HD_REPORTEntity> sqlexp = scdao.SqlExpression<CRD_HD_REPORTEntity>();
            List<CRD_HD_REPORTEntity> scresult = new List<CRD_HD_REPORTEntity>();
            object[] ids = new object[] {252169, 249599, 242136, 235146, 249627, 230737, 247860,
                       212887, 247008, 241023, 236425, 240892, 247026, 239967,
                       242442, 240957, 250672, 249737, 240972, 243086  };

            var result = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 13880 && x.Report_Id <= 13899).OrderBy(x => x.Cert_No).ToList();

            scresult = scdao.SelectByIds<CRD_HD_REPORTEntity>(ids).OrderBy(x => x.Cert_No).ToList();
            int index = 0;
            foreach (var item in result)
            {

                var lndList = dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = scdao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == scresult[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("report_id:" + item.Report_Id + "与reportid1" + scresult[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Count_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Count_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("report_id:" + item.Report_Id + "与reportid1" + scresult[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }

        }
        public static void checktestdbfromdifftype()
        {
            compareCRD_HD_REPORT();
            compareCRD_PI_IDENTITY();
            compareCRD_PI_RESIDENCE();
            compareCRD_PI_PROFESSNL();
            compareCRD_IS_CREDITCUE();
            compareCRD_CD_OverDueBreake();
            compareCRD_IS_OVDSUMMARY();
            compareCRD_IS_SHAREDEBT();
            compareCRD_CD_GUARANTEESummery();

            compareCRD_CD_ASSETDPST();
            compareCRD_CD_ASRREPAY();
            compareCRD_CD_LN();
            compareCRD_CD_LND();
            compareCRD_CD_STNCARD();
            compareCRD_CD_GUARANTEE();
            compareCRD_PI_TAXARREAR();
            compareCRD_PI_CIVILJDGM();
            compareCRD_PI_FORCEEXCTN();


            compareCRD_PI_ADMINPNSHM();
            compareCRD_PI_ACCFUND();
            compareCRD_PI_ENDINSDPT();
            compareCRD_PI_ENDINSDLR();
            compareCRD_PI_SALVATION();
            compareCRD_PI_COMPETENCE();
            compareCRD_PI_ADMINAWARD();
            compareCRD_PI_VEHICLE();
            compareCRD_PI_TELPNT();

            compareCRD_AN_ANCINFO();
            compareCRD_AN_DSTINFO();
            compareCRD_QR_REORDSMR();
            compareCRD_QR_RECORDDTLINFO();
            compareCRD_CD_LN_SPL();
            compareCRD_CD_LN_OVD();
            compareCRD_CD_LND_SPL();
            compareCRD_CD_LND_OVD();
            compareCRD_CD_STN_SPL();
            compareCRD_CD_STN_OVD();


        }
        public static void checkfromdiffdb()
        {
            compare1CRD_HD_REPORT();
            compare1CRD_PI_IDENTITY();
            compare1CRD_PI_RESIDENCE();
            compare1CRD_PI_PROFESSNL();
            compare1CRD_IS_CREDITCUE();
            compare1CRD_CD_OverDueBreake();
            compare1CRD_IS_OVDSUMMARY();
            compare1CRD_IS_SHAREDEBT();
            compare1CRD_CD_GUARANTEESummery();
            compare1CRD_CD_ASSETDPST();
            compare1CRD_CD_ASRREPAY();
            compare1CRD_CD_LN();
            compare1CRD_CD_LND();
            compare1CRD_CD_STNCARD();
            compare1CRD_CD_GUARANTEE();
            compare1CRD_PI_TAXARREAR();
            compare1CRD_PI_CIVILJDGM();
            compare1CRD_PI_FORCEEXCTN();
            compare1CRD_PI_ADMINPNSHM();
            compare1CRD_PI_ACCFUND();
            compare1CRD_PI_ENDINSDPT();
            compare1CRD_PI_ENDINSDLR();
            compare1CRD_PI_SALVATION();
            compare1CRD_PI_COMPETENCE();
            compare1CRD_PI_ADMINAWARD();
            compare1CRD_PI_VEHICLE();
            compare1CRD_PI_TELPNT();
            compare1CRD_AN_ANCINFO();
            compare1CRD_AN_DSTINFO();
            compare1CRD_QR_REORDSMR();
            compare1CRD_QR_RECORDDTLINFO();
            compare1CRD_CD_LN_SPL();
            compare1CRD_CD_LN_OVD();
            compare1CRD_CD_LND_SPL();
            compare1CRD_CD_LND_OVD();
            compare1CRD_CD_STN_SPL();
            compare1CRD_CD_STN_OVD();
        }
        #region 测试库和生产库之间的对比
        public static BaseDao dao1 = new BaseDao();
        public static BaseDao dao2 = new BaseDao("ExternalTradeDB1");
        private static List<CRD_HD_REPORTEntity> re1List;
        private static List<CRD_HD_REPORTEntity> re1List1;

        public static List<CRD_HD_REPORTEntity> reportListcompare1
        {
            get
            {
                if (re1List == null)
                {
                    re1List = dao1.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 13880 && x.Report_Id <= 13899).OrderBy(x => x.Cert_No).ToList();
                }
                return re1List;

            }

        }
        public static List<CRD_HD_REPORTEntity> reportListcompare2
        {
            get
            {
                if (re1List1 == null)
                {
                    object[] ids = new object[] {252169, 249599, 242136, 235146, 249627, 230737, 247860,
                       212887, 247008, 241023, 236425, 240892, 247026, 239967,
                       242442, 240957, 250672, 249737, 240972, 243086  };
                    re1List1 = dao2.SelectByIds<CRD_HD_REPORTEntity>(ids).OrderBy(x => x.Cert_No).ToList();
                }
                return re1List1
              ;
            }

        }
        public static void compare1CRD_HD_REPORT()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                comparetoenttiy(item, reportListcompare2[index]);
                index++;

            }

        }
        public static void compare1CRD_PI_IDENTITY()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {
                var lndList = dao1.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("report_id:" + item.Report_Id + "与reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }

                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Birthday).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Birthday).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("report_id:" + item.Report_Id + "与reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_RESIDENCE()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_RESIDENCE report_id:" + item.Report_Id + "与CRD_PI_RESIDENCE reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Address).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Address).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_RESIDENCE report_id:" + item.Report_Id + "与CRD_PI_RESIDENCE reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_PROFESSNL()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_PROFESSNL report_id:" + item.Report_Id + "与CRD_PI_PROFESSNL reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Duty).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Duty).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_PROFESSNL report_id:" + item.Report_Id + "与CRD_PI_PROFESSNL reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_IS_CREDITCUE()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {
                var lndList = dao1.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_IS_CREDITCUE report_id:" + item.Report_Id + "与 CRD_IS_CREDITCUE reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.House_Loan_Count).OrderBy(x => x.First_Loancard_Open_Month).ToList();
                    lndList1 = lndList1.OrderBy(x => x.House_Loan_Count).OrderBy(x => x.First_Loancard_Open_Month).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_CREDITCUE report_id:" + item.Report_Id + "与CRD_IS_CREDITCUE reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }

        }
        public static void compare1CRD_CD_OverDueBreake()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {
                var lndList = dao1.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_OverDueBreake report_id:" + item.Report_Id + "与CRD_CD_OverDueBreake reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.BadBebtNum).OrderBy(x => x.BadBebtMoney).ToList();
                    lndList1 = lndList1.OrderBy(x => x.BadBebtNum).OrderBy(x => x.BadBebtMoney).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_OverDueBreake report_id:" + item.Report_Id + "与CRD_CD_OverDueBreake reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_IS_OVDSUMMARY()
        {
            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_IS_OVDSUMMARY report_id:" + item.Report_Id + "与 CRD_IS_OVDSUMMARY reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Count_Dw).OrderBy(x => x.Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Count_Dw).OrderBy(x => x.Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_OVDSUMMARY report_id:" + item.Report_Id + "与CRD_IS_OVDSUMMARY reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_IS_SHAREDEBT()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_IS_SHAREDEBT report_id:" + item.Report_Id + "与CRD_IS_SHAREDEBT reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Type_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Type_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_SHAREDEBT report_id:" + item.Report_Id + "与CRD_IS_SHAREDEBT reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_GUARANTEESummery()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_GUARANTEESummery report_id:" + item.Report_Id + "与CRD_CD_GUARANTEESummery reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.GuaranteeNum).OrderBy(x => x.GuaranteeMoney).ToList();
                    lndList1 = lndList1.OrderBy(x => x.GuaranteeNum).OrderBy(x => x.GuaranteeMoney).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_GUARANTEESummery report_id:" + item.Report_Id + "与CRD_CD_GUARANTEESummery reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_ASSETDPST()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_ASSETDPST report_id:" + item.Report_Id + "与CRD_CD_ASSETDPST reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Balance).OrderBy(x => x.Get_Time).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Balance).OrderBy(x => x.Get_Time).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_ASSETDPST report_id:" + item.Report_Id + "与CRD_CD_ASSETDPST reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_ASRREPAY()
        {
            int index = 0;

            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_ASRREPAY report_id:" + item.Report_Id + "与CRD_CD_ASRREPAY reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_ASRREPAY report_id:" + item.Report_Id + "与CRD_CD_ASRREPAY reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LN()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LNEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LNEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN report_id:" + item.Report_Id + "与CRD_CD_LN reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN report_id:" + item.Report_Id + "与CRD_CD_LN reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LND()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LNDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LNDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND report_id:" + item.Report_Id + "与CRD_CD_LND reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND report_id:" + item.Report_Id + "与CRD_CD_LND reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_STNCARD()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_STNCARD report_id:" + item.Report_Id + "与CRD_CD_STNCARD reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_STNCARD report_id:" + item.Report_Id + "与CRD_CD_STNCARD reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_GUARANTEE()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_GUARANTEE report_id:" + item.Report_Id + "与CRD_CD_GUARANTEE reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Contract_Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Contract_Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_GUARANTEE report_id:" + item.Report_Id + "与CRD_CD_GUARANTEE reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_TAXARREAR()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_TAXARREAR report_id:" + item.Report_Id + "与CRD_PI_TAXARREAR reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Tax_Arrear_Date).OrderBy(x => x.Tax_Arrea_Amount).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Tax_Arrear_Date).OrderBy(x => x.Tax_Arrea_Amount).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_TAXARREAR report_id:" + item.Report_Id + "与CRD_PI_TAXARREAR reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_CIVILJDGM()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_CIVILJDGM report_id:" + item.Report_Id + "与CRD_PI_CIVILJDGM reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Court).OrderBy(x => x.Case_Result).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Court).OrderBy(x => x.Case_Result).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_CIVILJDGM report_id:" + item.Report_Id + "与CRD_PI_CIVILJDGM reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_FORCEEXCTN()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_FORCEEXCTN report_id:" + item.Report_Id + "与CRD_PI_FORCEEXCTN reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Court).OrderBy(x => x.Case_Reason).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Court).OrderBy(x => x.Case_Reason).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_FORCEEXCTN report_id:" + item.Report_Id + "与CRD_PI_FORCEEXCTN reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_ADMINPNSHM()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ADMINPNSHM report_id:" + item.Report_Id + "与CRD_PI_ADMINPNSHM reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ADMINPNSHM report_id:" + item.Report_Id + "与CRD_PI_ADMINPNSHM reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_ACCFUND()
        {
            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ACCFUND report_id:" + item.Report_Id + "与CRD_PI_ACCFUND reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.To_Month).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.To_Month).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ACCFUND report_id:" + item.Report_Id + "与CRD_PI_ACCFUND reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_ENDINSDPT()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {
                var lndList = dao1.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_PI_ENDINSDPT report_id:" + item.Report_Id + "与 CRD_PI_ENDINSDPT reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine(" CRD_PI_ENDINSDPT report_id:" + item.Report_Id + "与 CRD_PI_ENDINSDPT reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }

        }
        public static void compare1CRD_PI_ENDINSDLR()
        {
            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ENDINSDLR report_id:" + item.Report_Id + "与CRD_PI_ENDINSDLR reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ENDINSDLR report_id:" + item.Report_Id + "与CRD_PI_ENDINSDLR reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_SALVATION()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_SALVATION report_id:" + item.Report_Id + "与CRD_PI_SALVATION reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_SALVATION report_id:" + item.Report_Id + "与CRD_PI_SALVATION reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_COMPETENCE()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_COMPETENCE report_id:" + item.Report_Id + "与CRD_PI_COMPETENCE reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Award_Date).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Award_Date).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_COMPETENCE report_id:" + item.Report_Id + "与CRD_PI_COMPETENCE Vreportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_ADMINAWARD()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ADMINAWARD report_id:" + item.Report_Id + "与CRD_PI_ADMINAWARD reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ADMINAWARD report_id:" + item.Report_Id + "与CRD_PI_ADMINAWARD reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_VEHICLE()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_VEHICLE report_id:" + item.Report_Id + "与CRD_PI_VEHICLE reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Engine_Code).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Engine_Code).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_VEHICLE report_id:" + item.Report_Id + "与CRD_PI_VEHICLE reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_PI_TELPNT()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_TELPNT report_id:" + item.Report_Id + "与CRD_PI_TELPNT reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Get_Time).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Get_Time).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_TELPNT report_id:" + item.Report_Id + "与CRD_PI_TELPNT reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_AN_ANCINFO()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_AN_ANCINFO report_id:" + item.Report_Id + "与CRD_AN_ANCINFO reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_AN_ANCINFO report_id:" + item.Report_Id + "与CRD_AN_ANCINFO reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_AN_DSTINFO()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_AN_DSTINFO report_id:" + item.Report_Id + "与CRD_AN_DSTINFO reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_AN_DSTINFO report_id:" + item.Report_Id + "与CRD_AN_DSTINFO reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }

        }
        public static void compare1CRD_QR_REORDSMR()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_QR_REORDSMR report_id:" + item.Report_Id + "与CRD_QR_REORDSMR reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Reason).OrderBy(x => x.Sum_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Reason).OrderBy(x => x.Sum_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_QR_REORDSMR report_id:" + item.Report_Id + "与CRD_QR_REORDSMR reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_QR_RECORDDTLINFO()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_QR_RECORDDTLINFO report_id:" + item.Report_Id + "与CRD_QR_RECORDDTLINFO reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Query_Date).OrderBy(x => x.Querier).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Query_Date).OrderBy(x => x.Querier).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_QR_RECORDDTLINFO report_id:" + item.Report_Id + "与CRD_QR_RECORDDTLINFO reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LN_SPL()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN_SPL report_id:" + item.Report_Id + "与CRD_CD_LN_SPL reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN_SPL report_id:" + item.Report_Id + "与CRD_CD_LN_SPL reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LN_OVD()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN_OVD report_id:" + item.Report_Id + "与CRD_CD_LN_OVD reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Month_Dw).OrderBy(x => x.Last_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Month_Dw).OrderBy(x => x.Last_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN_OVD report_id:" + item.Report_Id + "与CRD_CD_LN_OVD reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LND_SPL()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND_SPL report_id:" + item.Report_Id + "与CRD_CD_LND_SPL reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND_SPL report_id:" + item.Report_Id + "与CRD_CD_LND_SPL reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_LND_OVD()
        {

            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND_OVD report_id:" + item.Report_Id + "与CRD_CD_LND_OVD reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND_OVD report_id:" + item.Report_Id + "与CRD_CD_LND_OVD reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_STN_SPL()
        {
            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_CD_STN_SPL report_id:" + item.Report_Id + "与 CRD_CD_STN_SPL reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine(" CRD_CD_STN_SPL report_id:" + item.Report_Id + "与 CRD_CD_STN_SPL reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        public static void compare1CRD_CD_STN_OVD()
        {
            int index = 0;
            foreach (var item in reportListcompare1)
            {

                var lndList = dao1.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == item.Report_Id);
                var lndList1 = dao2.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == reportListcompare2[index].Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_STN_OVD report_id:" + item.Report_Id + "与CRD_CD_STN_OVD reportid1" + reportListcompare2[index].Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_STN_OVD report_id:" + item.Report_Id + "与CRD_CD_STN_OVD reportid1" + reportListcompare2[index].Report_Id + "存储实例的数量不一致");
                }
                index++;
            }


        }
        #endregion
        #region 测试库数据之间的对比
        public static int sourceType = 10;
        public static int sourceType1 = 11;
        public static BaseDao dao = new BaseDao();
        private static List<CRD_HD_REPORTEntity> reList;


        public static List<CRD_HD_REPORTEntity> reportList
        {
            get
            {
                if (reList == null)
                {
                    reList = dao.Select<CRD_HD_REPORTEntity>(x => x.Report_Id >= 1341);
                }
                return reList
              ;
            }

        }

        public static void compareCRD_HD_REPORT()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                comparetoenttiy(en1, en2);

            }

        }
        public static void compareCRD_PI_IDENTITY()
        {
            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_IDENTITYEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Birthday).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Birthday).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("report_id:" + en1.Report_Id + "与reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_RESIDENCE()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_RESIDENCEEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_RESIDENCE report_id:" + en1.Report_Id + "与CRD_PI_RESIDENCE reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Address).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Address).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_RESIDENCE report_id:" + en1.Report_Id + "与CRD_PI_RESIDENCE reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_PROFESSNL()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_PROFESSNLEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_PROFESSNL report_id:" + en1.Report_Id + "与CRD_PI_PROFESSNL reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Duty).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Duty).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_PROFESSNL report_id:" + en1.Report_Id + "与CRD_PI_PROFESSNL reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_IS_CREDITCUE()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_IS_CREDITCUEEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_IS_CREDITCUE report_id:" + en1.Report_Id + "与 CRD_IS_CREDITCUE reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.House_Loan_Count).OrderBy(x => x.First_Loancard_Open_Month).ToList();
                    lndList1 = lndList1.OrderBy(x => x.House_Loan_Count).OrderBy(x => x.First_Loancard_Open_Month).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_CREDITCUE report_id:" + en1.Report_Id + "与CRD_IS_CREDITCUE reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_OverDueBreake()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_OverDueBreakeEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_OverDueBreake report_id:" + en1.Report_Id + "与CRD_CD_OverDueBreake reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.BadBebtNum).OrderBy(x => x.BadBebtMoney).ToList();
                    lndList1 = lndList1.OrderBy(x => x.BadBebtNum).OrderBy(x => x.BadBebtMoney).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_OverDueBreake report_id:" + en1.Report_Id + "与CRD_CD_OverDueBreake reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_IS_OVDSUMMARY()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_IS_OVDSUMMARYEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_IS_OVDSUMMARY report_id:" + en1.Report_Id + "与 CRD_IS_OVDSUMMARY reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Count_Dw).OrderBy(x => x.Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Count_Dw).OrderBy(x => x.Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_OVDSUMMARY report_id:" + en1.Report_Id + "与CRD_IS_OVDSUMMARY reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_IS_SHAREDEBT()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_IS_SHAREDEBTEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_IS_SHAREDEBT report_id:" + en1.Report_Id + "与CRD_IS_SHAREDEBT reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Type_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Type_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_IS_SHAREDEBT report_id:" + en1.Report_Id + "与CRD_IS_SHAREDEBT reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_GUARANTEESummery()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_GUARANTEESummeryEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_GUARANTEESummery report_id:" + en1.Report_Id + "与CRD_CD_GUARANTEESummery reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.GuaranteeNum).OrderBy(x => x.GuaranteeMoney).ToList();
                    lndList1 = lndList1.OrderBy(x => x.GuaranteeNum).OrderBy(x => x.GuaranteeMoney).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_GUARANTEESummery report_id:" + en1.Report_Id + "与CRD_CD_GUARANTEESummery reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_ASSETDPST()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_ASSETDPSTEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_ASSETDPST report_id:" + en1.Report_Id + "与CRD_CD_ASSETDPST reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Balance).OrderBy(x => x.Get_Time).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Balance).OrderBy(x => x.Get_Time).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_ASSETDPST report_id:" + en1.Report_Id + "与CRD_CD_ASSETDPST reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_ASRREPAY()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_ASRREPAYEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_ASRREPAY report_id:" + en1.Report_Id + "与CRD_CD_ASRREPAY reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_ASRREPAY report_id:" + en1.Report_Id + "与CRD_CD_ASRREPAY reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LN()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LNEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN report_id:" + en1.Report_Id + "与CRD_CD_LN reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN report_id:" + en1.Report_Id + "与CRD_CD_LN reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LND()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LNDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND report_id:" + en1.Report_Id + "与CRD_CD_LND reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND report_id:" + en1.Report_Id + "与CRD_CD_LND reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }

        }
        public static void compareCRD_CD_STNCARD()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_STNCARDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_STNCARD report_id:" + en1.Report_Id + "与CRD_CD_STNCARD reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Cue).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Cue).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_STNCARD report_id:" + en1.Report_Id + "与CRD_CD_STNCARD reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_GUARANTEE()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_GUARANTEEEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_GUARANTEE report_id:" + en1.Report_Id + "与CRD_CD_GUARANTEE reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Contract_Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Contract_Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_GUARANTEE report_id:" + en1.Report_Id + "与CRD_CD_GUARANTEE reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_TAXARREAR()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_TAXARREAREntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_TAXARREAR report_id:" + en1.Report_Id + "与CRD_PI_TAXARREAR reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Tax_Arrear_Date).OrderBy(x => x.Tax_Arrea_Amount).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Tax_Arrear_Date).OrderBy(x => x.Tax_Arrea_Amount).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_TAXARREAR report_id:" + en1.Report_Id + "与CRD_PI_TAXARREAR reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_CIVILJDGM()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_CIVILJDGMEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_CIVILJDGM report_id:" + en1.Report_Id + "与CRD_PI_CIVILJDGM reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Court).OrderBy(x => x.Case_Result).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Court).OrderBy(x => x.Case_Result).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_CIVILJDGM report_id:" + en1.Report_Id + "与CRD_PI_CIVILJDGM reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_FORCEEXCTN()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_FORCEEXCTNEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_FORCEEXCTN report_id:" + en1.Report_Id + "与CRD_PI_FORCEEXCTN reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Court).OrderBy(x => x.Case_Reason).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Court).OrderBy(x => x.Case_Reason).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_FORCEEXCTN report_id:" + en1.Report_Id + "与CRD_PI_FORCEEXCTN reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_ADMINPNSHM()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_ADMINPNSHMEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ADMINPNSHM report_id:" + en1.Report_Id + "与CRD_PI_ADMINPNSHM reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ADMINPNSHM report_id:" + en1.Report_Id + "与CRD_PI_ADMINPNSHM reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_ACCFUND()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_ACCFUNDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ACCFUND report_id:" + en1.Report_Id + "与CRD_PI_ACCFUND reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.To_Month).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.To_Month).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ACCFUND report_id:" + en1.Report_Id + "与CRD_PI_ACCFUND reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_ENDINSDPT()
        {
            ;
            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_ENDINSDPTEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_PI_ENDINSDPT report_id:" + en1.Report_Id + "与 CRD_PI_ENDINSDPT reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine(" CRD_PI_ENDINSDPT report_id:" + en1.Report_Id + "与 CRD_PI_ENDINSDPT reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_ENDINSDLR()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_ENDINSDLREntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ENDINSDLR report_id:" + en1.Report_Id + "与CRD_PI_ENDINSDLR reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Money).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ENDINSDLR report_id:" + en1.Report_Id + "与CRD_PI_ENDINSDLR reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_SALVATION()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_SALVATIONEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_SALVATION report_id:" + en1.Report_Id + "与CRD_PI_SALVATION reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_SALVATION report_id:" + en1.Report_Id + "与CRD_PI_SALVATION reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_COMPETENCE()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_COMPETENCEEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_COMPETENCE report_id:" + en1.Report_Id + "与CRD_PI_COMPETENCE reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Area).OrderBy(x => x.Award_Date).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Area).OrderBy(x => x.Award_Date).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_COMPETENCE report_id:" + en1.Report_Id + "与CRD_PI_COMPETENCE Vreportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }
        }
        public static void compareCRD_PI_ADMINAWARD()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_ADMINAWARDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_ADMINAWARD report_id:" + en1.Report_Id + "与CRD_PI_ADMINAWARD reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Organ_Name).OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Organ_Name).OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_ADMINAWARD report_id:" + en1.Report_Id + "与CRD_PI_ADMINAWARD reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_VEHICLE()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_VEHICLEEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_VEHICLE report_id:" + en1.Report_Id + "与CRD_PI_VEHICLE reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Engine_Code).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Engine_Code).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_VEHICLE report_id:" + en1.Report_Id + "与CRD_PI_VEHICLE reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_PI_TELPNT()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_PI_TELPNTEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_PI_TELPNT report_id:" + en1.Report_Id + "与CRD_PI_TELPNT reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Get_Time).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Get_Time).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_PI_TELPNT report_id:" + en1.Report_Id + "与CRD_PI_TELPNT reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_AN_ANCINFO()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_AN_ANCINFOEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_AN_ANCINFO report_id:" + en1.Report_Id + "与CRD_AN_ANCINFO reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_AN_ANCINFO report_id:" + en1.Report_Id + "与CRD_AN_ANCINFO reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_AN_DSTINFO()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_AN_DSTINFOEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_AN_DSTINFO report_id:" + en1.Report_Id + "与CRD_AN_DSTINFO reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Content).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Content).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_AN_DSTINFO report_id:" + en1.Report_Id + "与CRD_AN_DSTINFO reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_QR_REORDSMR()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_QR_REORDSMREntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_QR_REORDSMR report_id:" + en1.Report_Id + "与CRD_QR_REORDSMR reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Reason).OrderBy(x => x.Sum_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Reason).OrderBy(x => x.Sum_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_QR_REORDSMR report_id:" + en1.Report_Id + "与CRD_QR_REORDSMR reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_QR_RECORDDTLINFO()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_QR_RECORDDTLINFO report_id:" + en1.Report_Id + "与CRD_QR_RECORDDTLINFO reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Query_Date).OrderBy(x => x.Querier).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Query_Date).OrderBy(x => x.Querier).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_QR_RECORDDTLINFO report_id:" + en1.Report_Id + "与CRD_QR_RECORDDTLINFO reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LN_SPL()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LN_SPLEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN_SPL report_id:" + en1.Report_Id + "与CRD_CD_LN_SPL reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN_SPL report_id:" + en1.Report_Id + "与CRD_CD_LN_SPL reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LN_OVD()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LN_OVDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LN_OVD report_id:" + en1.Report_Id + "与CRD_CD_LN_OVD reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Month_Dw).OrderBy(x => x.Last_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Month_Dw).OrderBy(x => x.Last_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LN_OVD report_id:" + en1.Report_Id + "与CRD_CD_LN_OVD reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LND_SPL()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LND_SPLEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND_SPL report_id:" + en1.Report_Id + "与CRD_CD_LND_SPL reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND_SPL report_id:" + en1.Report_Id + "与CRD_CD_LND_SPL reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_LND_OVD()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_LND_OVDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_LND_OVD report_id:" + en1.Report_Id + "与CRD_CD_LND_OVD reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_LND_OVD report_id:" + en1.Report_Id + "与CRD_CD_LND_OVD reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_STN_SPL()
        {

            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_STN_SPLEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine(" CRD_CD_STN_SPL report_id:" + en1.Report_Id + "与 CRD_CD_STN_SPL reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Changing_Amount).OrderBy(x => x.Changing_Months).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine(" CRD_CD_STN_SPL report_id:" + en1.Report_Id + "与 CRD_CD_STN_SPL reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        public static void compareCRD_CD_STN_OVD()
        {
            var group = reportList.GroupBy(x => x.Report_Sn);
            var g = group.Where(x => x.Count() > 1);
            foreach (var item in g)
            {
                if (item.Count() == 1)
                    continue;
                var en1 = item.Where(x => x.SourceType == sourceType).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en1 == null)
                    continue;
                var en2 = item.Where(x => x.SourceType == sourceType1).OrderByDescending(x => x.Report_Id).FirstOrDefault();
                if (en2 == null)
                    continue;
                var lndList = dao.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == en1.Report_Id);
                var lndList1 = dao.Select<CRD_CD_STN_OVDEntity>(x => x.Report_Id == en2.Report_Id);
                if ((lndList == null && lndList1 != null) || (lndList != null && lndList1 == null))
                {
                    Console.WriteLine("CRD_CD_STN_OVD report_id:" + en1.Report_Id + "与CRD_CD_STN_OVD reportid1" + en2.Report_Id + "存储的数量不一致");
                    continue;
                }
                if ((lndList == null && lndList1 == null) || lndList.Count() == lndList1.Count())
                {
                    lndList = lndList.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    lndList1 = lndList1.OrderBy(x => x.Last_Months).OrderBy(x => x.Month_Dw).ToList();
                    for (int i = 0; i < lndList1.Count; i++)
                    {
                        comparetoenttiy(lndList[i], lndList1[i]);
                    }
                }
                else
                {
                    Console.WriteLine("CRD_CD_STN_OVD report_id:" + en1.Report_Id + "与CRD_CD_STN_OVD reportid1" + en2.Report_Id + "存储实例的数量不一致");
                }
            }


        }
        #endregion
    }

 
}