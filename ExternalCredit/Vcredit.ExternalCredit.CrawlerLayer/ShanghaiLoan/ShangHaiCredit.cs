using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.CommonLayer.Extension;
using System.Data;
using Vcredit.ExtTrade.ModelLayer.Common;
using ServiceStack.Text;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.Common.Helper;
using System.IO;
using System.Net;
using Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade;
using System.IO.Compression;
namespace Vcredit.ExternalCredit.CrawlerLayer.ShanghaiLoan
{
    

    public class ShangHaiCredit
    {
        const string headPath = "HEAD/";
        const string bodyPath = "MSG/QueryResultMessage/";
        const string reportPath = bodyPath + "ReportMessage";
        private  XmlHelper xml = null;
        NewForeignContainer container = null;
        readonly HttpHelper httpHelper = new HttpHelper();
        readonly CRD_CD_CreditUserInfoBusiness creditBus = new CRD_CD_CreditUserInfoBusiness();
        FileUp fileUp = new FileUp();
        string aeskey =null;
        string dateStr = DateTime.Now.ToString("yyyyMMdd");
        #region 创建request xml message
      
        /// <summary>
        /// 直接从数据库中获取数据查询
        /// </summary>
        public void PostReqMeg()
        {
            var cuinfoList =creditBus.GetAllList(SysEnums.SourceType.ShangHai);
            foreach (var item in cuinfoList)
            {
                var sqName = "SQ_" + item.CreditUserInfo_Id.ToString() + "_" + item.Cert_No + ".jpg";
                var zjName = "ZJ_" + item.CreditUserInfo_Id.ToString() + "_" + item.Cert_No + ".jpg";
                var sqfileName = ConfigData.requestFileSavePath  + sqName;
                var zjfileName = ConfigData.requestFileSavePath  + zjName;

                if (System.IO.File.Exists(sqfileName) && System.IO.File.Exists(zjfileName))
                {
                    item.CertFileBase64String = VariableCommonFun.ReadBase64Str(sqfileName);
                    item.AuthorizationFileBase64String = VariableCommonFun.ReadBase64Str(zjfileName);
                }
                else
                {
                    Log4netAdapter.WriteInfo(item.Cert_No + "缺失授权文件");
                    creditBus.SaveCredit(item.CreditUserInfo_Id.Value, (byte)RequestState.UpLoadFail, "缺失授权文件");
                    continue;
                }
                //发送请求
                if (ScationRequest(item))
                {
                    //接收请求
                    if(GetJinXmlCreditInfo(item))
                    {
                        var dir = ConfigData.RequestedFileSavePath + dateStr;
                        //迁移授权文件
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FileMove(sqfileName, dir + "\\" + sqName);
                        Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FileMove(zjfileName, dir + "\\" + zjName);
                    }
                }
            }
        }
        
        /// <summary>
        /// 扫描件主数据项
        /// </summary>
        /// <param name="requestMessage"></param>
        private bool ScationRequest(CRD_CD_CreditUserInfoEntity requestMessage)
        {
            RequestXml ReqXml = new RequestXml();
            var array = DateTime.Now.ToString("yyyyMMdd hhmmss").Split(' ');
            RequestHead HEAD = new RequestHead()
                {
                    VersionID = "10",
                    Sender = ConfigData.Sender,
                    SenderID = ConfigData.SenderID ?? string.Empty,
                    Receiver = ConfigData.Receiver ?? string.Empty,
                    ReceiverID = ConfigData.ReceiverID ?? string.Empty,
                    SendDate = array[0],
                    SendTime = array[1],
                    MesgID =requestMessage.PactNo==null?array[0]+array[1]+ VariableCommonFun.GetRandomString(8):requestMessage.PactNo,
                    TradeType =requestMessage.PactNo==null?个人扫描签名:个人电子或者扫描,
                    MesgRefID = requestMessage.PactNo ?? string.Empty//原报文流水

                };
            RequestMsg MSG = new RequestMsg()
            {
                QueryRequestMessage = new ScantionQueryRequestMsg()
                {
                    QueryName = requestMessage.Name,
                    QueryCertType = requestMessage.Cert_Type,
                    QueryCredNum = requestMessage.Cert_No,
                    UserCode = ConfigData.userCode ?? string.Empty,
                    UserCodePw = ConfigData.UserCodePw ?? string.Empty,
                    OrgCode = ConfigData.orgCode ?? string.Empty,
                    AuthoFile = requestMessage.AuthorizationFileBase64String,
                    CertFile = requestMessage.CertFileBase64String,
                    ReportVersion = 银行版,
                    QueryReason = requestMessage.QueryReason,
                    QueryType = 信用报告查询
                }
            };
            ReqXml.HEAD = HEAD;
            ReqXml.MSG = MSG;
            string reqxmlStr = XmlHelper.XmlSerialize(ReqXml);
            reqxmlStr = System.Text.RegularExpressions.Regex.Replace(reqxmlStr, "^[^<]", "");
            string enReqxml = EncryRequest(reqxmlStr);
           //Log4netAdapter.WriteInfo("请求报文:" + reqxmlStr);
            try
            {
                var result = GetHttpResult(enReqxml);
               // Log4netAdapter.WriteInfo("返回的结果" + result.Html);
                if (result.StatusCode != HttpStatusCode.OK)
                {
                    string str = requestMessage.Cert_No + "无法成功获取请求信息";
                    Log4netAdapter.WriteInfo(str);
                    creditBus.SaveCredit(requestMessage.CreditUserInfo_Id.Value, (byte)RequestState.QueryFail, str);
                    return false;
                }
                if (!result.Html.StartsWith("<"))//如果是密文
                {
                    var DeHtml = SharpZip.Decompress(result.Html);
                    string xmlResult = AESHelper.AesDecrypt(DeHtml, aeskey);
                    //Log4netAdapter.WriteInfo("解密后秘钥：" + aeskey);
                    xml = new XmlHelper(xmlResult);
                 
                }
                else
                {
                    xml = new XmlHelper(result.Html);
                }
            }
            catch (Exception ex)
            {
                string str = requestMessage.Cert_No + "调接口失败";
                Log4netAdapter.WriteError(str, ex);
                creditBus.SaveCredit(requestMessage.CreditUserInfo_Id.Value, (byte)RequestState.QueryFail, str);
                return false;
            }
            SaveRespone(requestMessage);
            creditBus.UpdatePactNOAndState(requestMessage.CreditUserInfo_Id.Value, (byte)RequestState.UpLoadSuccess, HEAD.MesgID);
            return true;

        }
        //保存返回的原始xml
        private void SaveRespone(CRD_CD_CreditUserInfoEntity credit)
        {

            try
            {
                var saveDirectory = ConfigData.ShanghaiResponeSavePath + dateStr;
                Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FolderCreate(saveDirectory);
                xml.SaveToXMLFile(saveDirectory + "\\" + credit.CreditUserInfo_Id.ToString()+"_"+credit.Cert_No+ ".xml");
            }
            catch (Exception ex )
            {
                Log4netAdapter.WriteError(credit.Cert_No+"保存返回的原始xml失败", ex);
   
            }
  
        }

        #region  对提交数据进行加密处理
        private string EncryRequest(string requestxml)
        {

            string publickey = RSAHelp.GetPublickey(ConfigData.ProvicePlanPublicKeyPath);
            //Log4netAdapter.WriteInfo("获取到的公钥" + publickey);
            // step 1: 获取随机生成的AES秘钥
            aeskey = AESHelper.getAESKey();
           // Log4netAdapter.WriteInfo("随机生成的秘钥:" + aeskey);
            // step2 : 用省级服务平台公钥加密随机生成的AES秘钥
            string pkEncAesStr = RSAHelp.EncryptKey(aeskey, publickey);
           // Log4netAdapter.WriteInfo("aeskeyRSA加密后的数据:" + pkEncAesStr);
            // step3: 用base64加密省级服务平台公钥加密的随机生成的AES秘钥密文
            string pkEncAesBase64Str = StrToUTF8Base64(pkEncAesStr);

            // step4: 获取省级服务平台公钥加密随机生成的AES秘钥的密文base64后的字符串长度
            int pkEncAesBase64StrLen = pkEncAesBase64Str.Length;

            // step5: 用AES秘钥加密请求报文XML
            string AESEncXmlStr = AESHelper.AesEncrypt(requestxml, aeskey);
           // Log4netAdapter.WriteInfo("报文加密后的:" + pkEncAesStr);
            // step6: 用base64加密AES秘钥加密的请求报文XML
            String AESEncXmlBase64Str = StrToUTF8Base64(AESEncXmlStr);

            // step7: 组装请求报文 【4字节长度省级服务平台的公钥加密金融机构随机产生的AES秘钥的密文长度（右靠齐，左端补0）+用公钥加密金融机构随机产生的AES秘钥的密文BASE64字符串+AES加密"各交易码"XML密文BASE64字符串
            string reqStr = InstallReq(pkEncAesBase64StrLen, pkEncAesBase64Str, AESEncXmlBase64Str);
            return reqStr;
        }
        private  string  StrToUTF8Base64(string str)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(str));
        }
        private string InstallReq(int pkEncAesBase64StrLen, string  pkEncAesBase64Str, string  AESEncXmlBase64Str)
        {

            return flushLeft('0', 4, pkEncAesBase64StrLen.ToString()) + pkEncAesBase64Str + AESEncXmlBase64Str;
        
        }
        private  string flushLeft(char c, long l, string lenStr)
        {
            string str = "";
            string cs = "";
            if (lenStr.Length > l)
                str = lenStr;
            else
                for (int i = 0; i < l - lenStr.Length; i++)
                    cs = cs + c;
            str = cs + lenStr;
            return str;
        }
    
        #endregion
        private HttpResult GetHttpResult(string data)
        {
            var httpItem = new HttpItem()
            {
                Method = "POST",
                URL = ConfigData.ShangHaiLoanWebUrl,
                Postdata = "inXML=" + data + "&programType=CSharp" //System.Web.HttpUtility.UrlEncode(data, System.Text.Encoding.UTF8)
               
            };

           return  httpHelper.GetHtml(httpItem);
        }
   
        #endregion

        #region 解析 respone xml message
        private void SaveFile(CRD_CD_CreditUserInfoEntity requestMessage)
        {
            var saveXmlDirectory = ConfigData.ShanghaiResponeSavePath + dateStr;
            var saveHtmlDirectory = ConfigData.HtmlSavePath;
            try
            {
                var result = xml.GetCurrentXmlNodeValue(bodyPath + "ReportPdf");
                if (!string.IsNullOrEmpty(result))
                {

                    Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FolderCreate(saveXmlDirectory);
                    fileUp.SaveFile(Convert.FromBase64String(result),
                saveXmlDirectory + "\\" + requestMessage.CreditUserInfo_Id + "_" + requestMessage.Cert_No + ".pdf");
                }
                result = xml.GetCurrentXmlNodeValue(bodyPath + "ReportHtml");
                if (!string.IsNullOrEmpty(result))
                {
                    Console.WriteLine("保存html");
                    Vcredit.ExtTrade.CommonLayer.FileOperateHelper.FolderCreate(saveHtmlDirectory);
                    fileUp.SaveFile(Convert.FromBase64String(result),
                saveHtmlDirectory + "\\" + requestMessage.CreditUserInfo_Id + "_" + requestMessage.Cert_No + ".html");
                }
            }
            catch
            {
                creditBus.SaveCredit(requestMessage.CreditUserInfo_Id.Value, (byte)RequestState.QueryFail, requestMessage.Cert_No+"保存附带文件失败");
                Log4netAdapter.WriteInfo(requestMessage.Cert_No + "pdf或者html保存失败");
   
            }
        
        }
        /// <summary>
        /// 解析xml
        /// </summary>
        /// <param name="credit"></param>
        /// <param name="xmlStr"></param>
        public  void AnalyXmlCreditInfo(CRD_CD_CreditUserInfoEntity  credit,string xmlStr)
        {
            xml = new XmlHelper(xmlStr);
            GetJinXmlCreditInfo(credit);
        }
        private bool GetJinXmlCreditInfo(CRD_CD_CreditUserInfoEntity creditEntity)
        {
            try
            {
                var resultCode = xml.GetCurrentXmlNodeValue(bodyPath + "ResultCode");
                if (resultCode == "0115" || resultCode == "0117")
                {
                    var error = xml.GetCurrentXmlNodeValue(reportPath + "/ErrorInfo");
                    creditBus.SaveCredit(creditEntity.CreditUserInfo_Id.Value, (byte)RequestState.UpLoadFail, errorCodeDic.ContainsKey(resultCode) ? errorCodeDic[resultCode] : resultCode);
                    Log4netAdapter.WriteInfo(creditEntity.Cert_No + "正在审核或者已经审核通过正在查询，返回码：" + resultCode + "，错误信息：" + error);
                    return false;
                }
                if (resultCode == "0019")
                {
                    var error = xml.GetCurrentXmlNodeValue(reportPath + "/ErrorInfo");
                    creditBus.SaveCredit(creditEntity.CreditUserInfo_Id.Value, (byte)RequestState.HaveNoData, errorCodeDic.ContainsKey(resultCode) ? errorCodeDic[resultCode] : resultCode);
                    Log4netAdapter.WriteInfo(creditEntity.Cert_No + "征信空白，返回码：" + resultCode + "，错误信息：" + error);
                    return true;
                }
                if (resultCode != "0000")
                {
                    var error = xml.GetCurrentXmlNodeValue(reportPath + "/ErrorInfo");
                    creditBus.SaveCredit(creditEntity.CreditUserInfo_Id.Value, (byte)RequestState.QueryFail, errorCodeDic.ContainsKey(resultCode) ? errorCodeDic[resultCode] : resultCode);
                    Log4netAdapter.WriteInfo(creditEntity.Cert_No + "请求失败，返回码：" + resultCode+"，错误信息："+error);
                    return true;
                }
                else
                {
                    SaveFile(creditEntity);//保持pdf和html文件
                    return true;
                }
          
                //container = new NewForeignContainer()
                //{
                //    CRD_CD_STNCARD = new List<CRD_CD_STNCARDEntity>(),
                //    CRD_CD_STN_SPL = new List<CRD_CD_STN_SPLEntity>(),
                //    CRD_CD_STN_OVD = new List<CRD_CD_STN_OVDEntity>(),
                //    CRD_CD_OverDueBreake = new List<CRD_CD_OverDueBreakeEntity>(),
                //    CRD_CD_LN_SPL = new List<CRD_CD_LN_SPLEntity>(),
                //    CRD_CD_LN_OVD = new List<CRD_CD_LN_OVDEntity>(),
                //    CRD_CD_LND_SPL = new List<CRD_CD_LND_SPLEntity>(),
                //    CRD_CD_LND_OVD = new List<CRD_CD_LND_OVDEntity>(),
                //    CRD_CD_LND = new List<CRD_CD_LNDEntity>(),
                //    CRD_CD_LN = new List<CRD_CD_LNEntity>(),
                //    CRD_CD_GUARANTEESummery = new List<CRD_CD_GUARANTEESummeryEntity>(),
                //    CRD_AN_ANCINFO = new List<CRD_AN_ANCINFOEntity>(),
                //    CRD_AN_DSTINFO = new List<CRD_AN_DSTINFOEntity>(),
                //    CRD_CD_ASRREPAY = new List<CRD_CD_ASRREPAYEntity>(),
                //    CRD_CD_ASSETDPST = new List<CRD_CD_ASSETDPSTEntity>(),
                //    CRD_CD_GUARANTEE = new List<CRD_CD_GUARANTEEEntity>(),
                //    CRD_IS_CREDITCUE = new List<CRD_IS_CREDITCUEEntity>(),
                //    CRD_IS_OVDSUMMARY = new List<CRD_IS_OVDSUMMARYEntity>(),
                //    CRD_IS_SHAREDEBT = new List<CRD_IS_SHAREDEBTEntity>(),
                //    CRD_PI_ACCFUND = new List<CRD_PI_ACCFUNDEntity>(),
                //    CRD_PI_ADMINAWARD = new List<CRD_PI_ADMINAWARDEntity>(),
                //    CRD_PI_ADMINPNSHM = new List<CRD_PI_ADMINPNSHMEntity>(),
                //    CRD_PI_CIVILJDGM = new List<CRD_PI_CIVILJDGMEntity>(),
                //    CRD_PI_COMPETENCE = new List<CRD_PI_COMPETENCEEntity>(),
                //    CRD_PI_ENDINSDLR = new List<CRD_PI_ENDINSDLREntity>(),
                //    CRD_PI_ENDINSDPT = new List<CRD_PI_ENDINSDPTEntity>(),
                //    CRD_PI_FORCEEXCTN = new List<CRD_PI_FORCEEXCTNEntity>(),
                //    CRD_PI_IDENTITY = new CRD_PI_IDENTITYEntity(),
                //    CRD_PI_PROFESSNL = new List<CRD_PI_PROFESSNLEntity>(),
                //    CRD_PI_RESIDENCE = new List<CRD_PI_RESIDENCEEntity>(),
                //    CRD_PI_SALVATION = new List<CRD_PI_SALVATIONEntity>(),
                //    CRD_PI_TAXARREAR = new List<CRD_PI_TAXARREAREntity>(),
                //    CRD_PI_TELPNT = new List<CRD_PI_TELPNTEntity>(),
                //    CRD_PI_VEHICLE = new List<CRD_PI_VEHICLEEntity>(),
                //    CRD_QR_RECORDDTLINFO = new List<CRD_QR_RECORDDTLINFOEntity>(),
                //    CRD_QR_REORDSMR = new List<CRD_QR_REORDSMREntity>()

                //};
                //container.CRD_HD_REPORT.SourceType = (byte)SysEnums.SourceType.ShangHai;//设置征信类型
                //xml.SetAndReturnCurrentXmlNode(reportPath);
                ////报告基本信息
                //AnalysisHeaderInfo();
                ////个人信息
                //AnalysisPersonalInfo();
                ////信息汇总
                //AnalysisInfoSummary();
                ////信用卡信息
                //AnalysisCreditDetail();
                ////公共信息
                //AnalysisPublicInfo();
                ////查询记录
                //AnalysisQueryRecord();
            }
            catch(Exception ex)
            {
                var error=  creditEntity.Cert_No + "解析失败";
                Log4netAdapter.WriteError(error,ex);
                creditBus.SaveCredit(creditEntity.CreditUserInfo_Id.Value, (byte)RequestState.AnalysisFail, error);
                return false;
            }
            //return new ForeignComBus().SaveShangHaiCreditInfoToDB(container,creditEntity);
        }



        #region  Header （报告基本信息）

        private void AnalysisHeaderInfo()
        {
            var node = xml.GetCurrentXmlNodeNode("ReportBasics");
            if (node == null)
            {
                Log4netAdapter.WriteInfo("没有报告信息");
                return;
            }
            container.CRD_HD_REPORT.Report_Sn = xml.GetNodeValue(node, "ReportId");
            container.CRD_HD_REPORT.Query_Time = xml.GetNodeValue(node, "ReportReqTime").ToDateTime();
            container.CRD_HD_REPORT.Report_Create_Time = xml.GetNodeValue(node, "ReportTime").ToDateTime();
            container.CRD_HD_REPORT.Name = xml.GetNodeValue(node, "QueryName");
            container.CRD_HD_REPORT.Cert_Type = xml.GetNodeValue(node, "QueryCertType");
            container.CRD_HD_REPORT.Cert_No = xml.GetNodeValue(node, "QueryCredNum");
            container.CRD_HD_REPORT.Query_Reason = xml.GetNodeValue(node, "QueryReason");
            container.CRD_HD_REPORT.Query_Org = xml.GetNodeValue(node, "QueryOperator");

        }
        #endregion

        #region PersonalInfo（个人信息）
        private void AnalysisPersonalInfo()
        {
            //获取身份信息
            GetIdentityInfo();
            //获取居住信息
            GetResidenceInfo();
            //获取职业信息
            GetProfessionInfo();
        }
        private void GetIdentityInfo()
        {
            var node = xml.GetCurrentXmlNodeNode("IdentityInfo");
            if (node == null)
                return;
            container.CRD_PI_IDENTITY.Gender = xml.GetNodeValue(node, "Gender");
            container.CRD_PI_IDENTITY.Birthday = xml.GetNodeValue(node, "DateOfBirth").ToDateTime();
            container.CRD_PI_IDENTITY.Marital_State = xml.GetNodeValue(node, "MaritalStatus");
            container.CRD_PI_IDENTITY.Mobile = xml.GetNodeValue(node, "MobilePhone");
            container.CRD_PI_IDENTITY.Office_Telephone_No = xml.GetNodeValue(node, "CompanyPhone");
            container.CRD_PI_IDENTITY.Home_Telephone_No = xml.GetNodeValue(node, "HomePhone");
            container.CRD_PI_IDENTITY.Edu_Level = xml.GetNodeValue(node, "Education");
            container.CRD_PI_IDENTITY.Edu_Degree = xml.GetNodeValue(node, "Degree");
            container.CRD_PI_IDENTITY.Post_Address = xml.GetNodeValue(node, "ContactAddress");
            container.CRD_PI_IDENTITY.Registered_Address = xml.GetNodeValue(node, "ResidenceAddress");

            node = xml.GetCurrentXmlNodeNode("SpouseInfo");
            if (node == null)
                return;
            container.CRD_PI_IDENTITY.Mate_Name = xml.GetNodeValue(node, "SpouseName");
            container.CRD_PI_IDENTITY.Mate_Cert_Type = xml.GetNodeValue(node, "SpouseCertType");
            container.CRD_PI_IDENTITY.Mate_Cert_No = xml.GetNodeValue(node, "SpouseCredNum");
            container.CRD_PI_IDENTITY.Mate_Employer = xml.GetNodeValue(node, "SpouseCompany");
            container.CRD_PI_IDENTITY.Mate_Telephone_No = xml.GetNodeValue(node, "SpousePhone");
        }
        private void GetResidenceInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_RESIDENCEEntity residence = new CRD_PI_RESIDENCEEntity()
                {
                    Address = xml.GetNodeValue(param, "ResideAddr"),
                    Residence_Type = xml.GetNodeValue(param, "ResideStatus"),
                    Get_Time = xml.GetNodeValue(param, "InfoUpdateDate").ToDateTime()
                };
                container.CRD_PI_RESIDENCE.Add(residence);
            };
            AddCurrentXmlNodesInfo("ResideInfo", aciton);
        }
        private void GetProfessionInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_PROFESSNLEntity profession = new CRD_PI_PROFESSNLEntity()
                {
                    Employer = xml.GetNodeValue(param, "CompanyName"),
                    Employer_Address = xml.GetNodeValue(param, "CompanyAddress"),
                    Occupation = xml.GetNodeValue(param, "Occupation"),
                    Industry = xml.GetNodeValue(param, "CompanyType"),
                    Duty = xml.GetNodeValue(param, "Position"),
                    Title_Dw = xml.GetNodeValue(param, "PostTitle"),
                    Start_Year = xml.GetNodeValue(param, "EmployedYear"),
                    Get_Time = xml.GetNodeValue(param, "InfoUpdateDate").ToDateTime()
                };
                container.CRD_PI_PROFESSNL.Add(profession);
            };
            AddCurrentXmlNodesInfo("OccupationInfo", aciton);
        }
        #endregion

        #region InfoSummary（信息汇总）

        private void AnalysisInfoSummary()
        {
            //获取未销户准贷记卡信息
            GetUndestoryLoancard();
            //获取为销户贷记卡信息
            GetUndestoryStandardLoancard();
            //获取未结清贷款信息
            GetUnpaidLoan();
            //逾期（透支）信息汇总
            GetBeOverdueSummaryInfo();
            //外担保信息汇总
            GetPerSummaryGuarantee();
            //查询记录汇总
            GetQueryRecordSum();
        }
        private void GetQueryRecordSum()
        {
            var node = xml.GetCurrentXmlNodeNode("QueryRecordSum");
            if (node == null)
                return;
            AddCRD_QR_REORDSMR( node,"Last1MonthsOrgLoanApprovalSum","1","最近1个月内的查询机构数，贷款审批。" );
            AddCRD_QR_REORDSMR(node, "Last1MonthsOrgCreditCardApprovalSum", "2", "最近1个月内的查询机构数，信用卡审批。");
            AddCRD_QR_REORDSMR(node, "Last1MonthsLoanApprovalSum", "3", "最近1个月内的查询次数，贷款审批");
            AddCRD_QR_REORDSMR(node, "Last1MonthsCreditCardApprovalSum", "4", "最近1个月内的查询次数，信用卡审批。");
            AddCRD_QR_REORDSMR(node, "Last1MonthsSelfQuerySum", "5", "最近1个月内的查询次数，本人查询。");
            AddCRD_QR_REORDSMR(node, "Last2YearsLoanMangeSum", "6", "最近2年内的查询次数，贷后管理。");
            AddCRD_QR_REORDSMR(node, "Last2YearsGuarApproSum", "7", "最近2年内的查询次数，担保资格审查。");
            AddCRD_QR_REORDSMR(node, "Last2YearsSpeMerchApproSum", "8", "最近2年内的查询次数，特约商户实名审查");
        }
        private  void AddCRD_QR_REORDSMR(XmlNode node,string nodeName,string typeid,string reason )
        {
            var num = xml.GetNodeValue(node, nodeName).ToDecimal();
            if (num != null)
            {
                container.CRD_QR_REORDSMR.Add(new CRD_QR_REORDSMREntity()
                {
                    Type_Id = typeid,
                    Sum_Dw = num,
                    Reason = reason
                });
            }
        }
        private void GetUndestoryLoancard()
        {
            var node = xml.GetCurrentXmlNodeNode("NoCAQuasiCreditCardInfoSum");
            if (node == null)
                return;
            CRD_IS_SHAREDEBTEntity noCancellLND = new CRD_IS_SHAREDEBTEntity()
            {
                Type_Dw = "未销户准贷记卡信息汇总",
                Finance_Corp_Count = xml.GetNodeValue(node, "SendCardLegalPersonOrgNum").ToDecimal(),
                Finance_Org_Count = xml.GetNodeValue(node, "SendCardOrgNum").ToDecimal(),
                Account_Count = xml.GetNodeValue(node, "AccountNumber").ToDecimal(),
                Credit_Limit = xml.GetNodeValue(node, "CreditTotalAmount").ToDecimal(),
                Max_Credit_Limit_Per_Org = xml.GetNodeValue(node, "AloneBankHighestCreditAmount").ToDecimal(),
                Min_Credit_Limit_Per_Org = xml.GetNodeValue(node, "AloneBankLowestCreditAmount").ToDecimal(),
                Used_Credit_Limit = xml.GetNodeValue(node, "OverdraftAmount").ToDecimal(),
                Latest_6M_Used_Avg_Amount = xml.GetNodeValue(node, "Recent6MonthAveOverdAmount").ToDecimal()
            };
            container.CRD_IS_SHAREDEBT.Add(noCancellLND);

        }
        private void GetUndestoryStandardLoancard()
        {

            var node = xml.GetCurrentXmlNodeNode("NoCancelCreditCardSummaryInfo");
            if (node == null)
                return;
            CRD_IS_SHAREDEBTEntity noCancellstncard = new CRD_IS_SHAREDEBTEntity()
            {
                Type_Dw = "未销户贷记卡信息汇总",
                Finance_Corp_Count = xml.GetNodeValue(node, "HairpinLegalOrgNum").ToDecimal(),
                Finance_Org_Count = xml.GetNodeValue(node, "HairpinOrgNum").ToDecimal(),
                Account_Count = xml.GetNodeValue(node, "AccountNum").ToDecimal(),
                Credit_Limit = xml.GetNodeValue(node, "FinanceProfits").ToDecimal(),
                Max_Credit_Limit_Per_Org = xml.GetNodeValue(node, "SingleBankMaxFinanceLimit").ToDecimal(),
                Min_Credit_Limit_Per_Org = xml.GetNodeValue(node, "SingleBankMinFinanceLimit").ToDecimal(),
                Used_Credit_Limit = xml.GetNodeValue(node, "UsedCreditLimit").ToDecimal(),
                Latest_6M_Used_Avg_Amount = xml.GetNodeValue(node, "Last6MonthsAvgUseLimit").ToDecimal()
            };
            container.CRD_IS_SHAREDEBT.Add(noCancellstncard);


        }
        private void GetUnpaidLoan()
        {
            var node = xml.GetCurrentXmlNodeNode("NotSettledLoanSummaryInfo");
            if (node == null)
                return;
            CRD_IS_SHAREDEBTEntity noClearLoan = new CRD_IS_SHAREDEBTEntity()
            {
                Type_Dw = "未结清贷款信息汇总",
                Finance_Corp_Count = xml.GetNodeValue(node, "LoanLegalOrgNum").ToDecimal(),
                Finance_Org_Count = xml.GetNodeValue(node, "LoanOrgNum").ToDecimal(),
                Account_Count = xml.GetNodeValue(node, "CountNum").ToDecimal(),
                Credit_Limit = xml.GetNodeValue(node, "ContractProfits").ToDecimal(),
                Balance = xml.GetNodeValue(node, "Balance").ToDecimal(),
                Latest_6M_Used_Avg_Amount = xml.GetNodeValue(node, "Last6MothsAvgRepayAmount").ToDecimal()

            };
            container.CRD_IS_SHAREDEBT.Add(noClearLoan);

        }
        private void GetBeOverdueSummaryInfo()
        {
            var node = xml.GetCurrentXmlNodeNode("BeOverdueSummaryInfo");
            if (node == null)
                return;
            CRD_IS_OVDSUMMARYEntity ovdsumln = new CRD_IS_OVDSUMMARYEntity() //贷款
            {
                Type_Dw = "贷款逾期",
                Count_Dw = xml.GetNodeValue(node, "LoanOverdueAccountNum").ToDecimal(),
                Months = xml.GetNodeValue(node, "LoanOverdueMonthNum").ToDecimal(),
                Highest_Oa_Per_Mon = xml.GetNodeValue(node, "LoanOverdueHighestSingleMothOverdueAmount").ToDecimal(),
                Max_Duration = xml.GetNodeValue(node, "LoanOverdueLongOverdueMonths").ToDecimal(),
            };
            CRD_IS_OVDSUMMARYEntity ovdsumlnd = new CRD_IS_OVDSUMMARYEntity()//贷记卡
            {
                Type_Dw = "贷记卡逾期",
                Count_Dw = xml.GetNodeValue(node, "CreditCardOverdueAccountNum").ToDecimal(),
                Months = xml.GetNodeValue(node, "CreditCardOverdueMothNum").ToDecimal(),
                Highest_Oa_Per_Mon = xml.GetNodeValue(node, "CreditCardOverdueMaxSingleMothOverdueAmount").ToDecimal(),
                Max_Duration = xml.GetNodeValue(node, "CreditCardOverdraftLongOverdueMonths").ToDecimal(),

            };
            CRD_IS_OVDSUMMARYEntity ovdsumstn = new CRD_IS_OVDSUMMARYEntity()//准贷记卡
            {
                Type_Dw = "准贷记卡60天以上透支",
                Count_Dw = xml.GetNodeValue(node, "QuasiCreditCardOverdraftAccountNum").ToDecimal(),
                Months = xml.GetNodeValue(node, "QuasiCreditCardOverdraft0DaysMothNum").ToDecimal(),
                Highest_Oa_Per_Mon = xml.GetNodeValue(node, "QuasiCreditCardMaxSingleMothOverdueAmount").ToDecimal(),
                Max_Duration = xml.GetNodeValue(node, "QuasiCreditCardOverdueLongOverdueMonths").ToDecimal(),

            };
            container.CRD_IS_OVDSUMMARY.Add(ovdsumln);
            container.CRD_IS_OVDSUMMARY.Add(ovdsumlnd);
            container.CRD_IS_OVDSUMMARY.Add(ovdsumstn);
        }
        private void GetPerSummaryGuarantee()
        {
            var node = xml.GetCurrentXmlNodeNode("PerSummaryGuarantee");
            if (node == null)
                return;
            CRD_CD_GUARANTEESummeryEntity gua = new CRD_CD_GUARANTEESummeryEntity()
            {
                GuaranteeNum = xml.GetNodeValue(node, "PerGuarMount").ToInt(),
                GuaranteeMoney = xml.GetNodeValue(node, "PerGuarAmount").ToDecimal(),
                PrincipalBalance = xml.GetNodeValue(node, "PerGuarPrincipalAmount").ToDecimal()

            };
            container.CRD_CD_GUARANTEESummery.Add(gua);
        }


        #endregion

        #region CreditDetail（金融信息）
        private void AnalysisCreditDetail()
        {
            //获取贷款信息
            GetLoad();
            //获取贷记卡信息
            GetLoadcard();
            //获取准贷记卡信息
            GetStandardLoancard();
            //获取对外担保信息
            GetExternalGuaranteeLoan();

        }

        #region 获取贷款信息

        private void GetLoad()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_LNEntity currentLn = new CRD_CD_LNEntity();
                currentLn.Cue = xml.GetNodeValue(param, "TitleInfo");
                currentLn.State = xml.GetNodeValue(param, "AccountStatus");
                currentLn.Class5_State = xml.GetNodeValue(param, "Fiveclasscode");
                currentLn.Balance = xml.GetNodeValue(param, "PrincipalAmount").ToDecimal();
                currentLn.Remain_Payment_Cyc = xml.GetNodeValue(param, "RemainRepayNum").ToDecimal();
                currentLn.Scheduled_Payment_Amount = xml.GetNodeValue(param, "ThisMonthRepayAmount").ToDecimal();
                currentLn.Scheduled_Payment_Date = xml.GetNodeValue(param, "ThisMonthRepayDay").ToDateTime();
                currentLn.Actual_Payment_Amount = xml.GetNodeValue(param, "ThisMonthActualRepayAmount").ToDecimal();
                currentLn.Recent_Pay_Date = xml.GetNodeValue(param, "TheLastestRepayDay").ToDateTime();
                currentLn.Curr_Overdue_Cyc = xml.GetNodeValue(param, "CurrentOverdueNum").ToInt();
                currentLn.Curr_Overdue_Amount = xml.GetNodeValue(param, "CurrentOverdueAmount").ToDecimal();
                currentLn.Overdue31_To60_Amount = xml.GetNodeValue(param, "Overdue31To60Days").ToDecimal();
                currentLn.Overdue61_To90_Amount = xml.GetNodeValue(param, "Overdue61To90Days").ToDecimal();
                currentLn.Overdue91_To180_Amount = xml.GetNodeValue(param, "Overdue91To180Days").ToDecimal();
                currentLn.Overdue_Over180_Amount = xml.GetNodeValue(param, "Overdue180Days").ToDecimal();
                string month24RepayTitle = xml.GetNodeValue(param, "Month24RepayTitle");  
                if (!string.IsNullOrEmpty(month24RepayTitle))
                {
                    string beginDate = null;
                    string endDate = null;
                    GetBeginAndEndTime(month24RepayTitle, ref beginDate, ref endDate);
                    currentLn.Payment_State_Begin_Month = beginDate;
                    currentLn.Payment_State_End_Month = endDate;
                    currentLn.Payment_State = xml.GetNodeValue(param, "Month24RepayStatus");
                }
                string overdueTitle1 = xml.GetNodeValue(param, "OverdueTitle1");
                if (!string.IsNullOrEmpty(overdueTitle1))
                {
                    string beginDate = null;
                    string endDate = null;
                    GetBeginAndEndTime(overdueTitle1, ref beginDate, ref endDate);
                    currentLn.Overdue_Record_Begin_Month = beginDate;
                    currentLn.Overdue_Record_End_Month = endDate;
                }

                var title = CreateStrData(currentLn.Cue);
                currentLn.Finance_Org = title.Finance_Org;
                currentLn.Account_Dw = title.Account_Dw;
                currentLn.Type_Dw = title.Type_Dw;
                currentLn.Currency = title.Currency;
                currentLn.Open_Date = title.Open_Date.ToDateTime();
                currentLn.End_Date = title.End_Date.ToDateTime();
                currentLn.Credit_Limit_Amount = title.Credit_Limit_Amount.ToDecimal();
                currentLn.Guarantee_Type = title.Guarantee_Type;
                currentLn.Payment_Rating = title.Payment_Rating;
                currentLn.Payment_Cyc = title.Payment_Cyc;
                if (string.IsNullOrEmpty(currentLn.State))
                    currentLn.State = title.State;
                // title .Share_Credit_Limit_Amount
                currentLn.GetTime = title.GetTime.ToDateTime();
                GetLatest5YearOverdueRecord(currentLn, param);//添加贷款逾期信息
                GetSpecialTrade(currentLn, param);//获取特殊交易信息
                container.CRD_CD_LN.Add(currentLn);
            };
            AddCurrentXmlNodesInfo("LoanInfo", aciton);
        }

        private void GetLatest5YearOverdueRecord(CRD_CD_LNEntity currentLn, XmlNode param)
        {
            string istr = null;
            for (int i = 1; i < 3;i++ )
            {
                istr = i.ToString();
                var overdueContMonthEles= xml.GetNodeList(param, "OverdueContMonths" + istr);
                var overdueMonthEles = xml.GetNodeList(param, "OverdueMonth" + istr);
                var overdueAmountEles = xml.GetNodeList(param, "OverdueAmount" + istr);
                if (overdueContMonthEles.Count == 0)
                    break;
                if (overdueContMonthEles.Count == overdueMonthEles.Count && overdueContMonthEles.Count == overdueAmountEles.Count)
                {
                    int index = 0;
                    foreach(XmlNode item in overdueContMonthEles)
                    {
                        currentLn.LnoverList.Add(new CRD_CD_LN_OVDEntity()
                        {
                            Last_Months =item.InnerText.ToDecimal(),
                            Month_Dw = overdueMonthEles[index].InnerText,
                            Amount = overdueAmountEles[index].InnerText.ToDecimal()
                        });
                        index++;
                    }
                }
                else
                {
                    throw new Exception("征信报告贷记卡逾期信息不对称肯定有问题");
                }
            }
        }

        private void GetSpecialTrade(CRD_CD_LNEntity ln, XmlNode node)
        {
            var contents = xml.GetNodeList(node, "SpecTraDetailed");
            if (contents.Count == 0)
                return;
            var Changing_Amounts = xml.GetNodeList(node, "SpecTraAmount");
            var Changing_Months = xml.GetNodeList(node, "SpecTraChanMonthNum");
            var types = xml.GetNodeList(node, "SpecTraType");
            var Get_Times = xml.GetNodeList(node, "SpecTraDate");
            if (Changing_Amounts.Count == Changing_Months.Count && Changing_Amounts.Count == types.Count
                && Changing_Amounts.Count == Get_Times.Count&&Changing_Amounts.Count==contents.Count)
            {
                int index = 0;
                foreach (XmlNode item in contents)
                {
                    ln.LnSPLList.Add(new CRD_CD_LN_SPLEntity()
                    {
                        Changing_Amount = Changing_Amounts[index].InnerText.ToDecimal(),
                        Changing_Months = Changing_Months[index].InnerText.ToDecimal(),
                        Content = item.InnerText,
                        Get_Time = Get_Times[index].InnerText.ToDateTime(),
                        Type_Dw = types[index].InnerText
                    });
                    index++;
                }
            }
            else
                throw new Exception("征信报告贷款特使交易不对称肯定有问题");
        }
        #endregion

        #region 获取贷记卡信息
        private void GetLoadcard()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_LNDEntity currentLnd = new CRD_CD_LNDEntity();
                currentLnd.Cue = xml.GetNodeValue(param, "TitleInfo");
                currentLnd.State = xml.GetNodeValue(param, "AccountStatus");
                currentLnd.Share_Credit_Limit_Amount = xml.GetNodeValue(param, "SharedCreditLimit").ToDecimal();
                currentLnd.Used_Credit_Limit_Amount = xml.GetNodeValue(param, "UsedCreditLimit").ToDecimal();
                currentLnd.Latest6_Month_Used_Avg_Amount = xml.GetNodeValue(param, "Last6MonthsAveUsedCreditLimit").ToDecimal();
                currentLnd.Used_Highest_Amount = xml.GetNodeValue(param, "MaxUsedCreditLimit").ToDecimal();
                //  currentLnd.Remain_Payment_Cyc = xml.GetNodeValue(param, "RemainRepayNum").ToDecimal();
                currentLnd.Scheduled_Payment_Amount = xml.GetNodeValue(param, "ThisMonthRepayAmount").ToDecimal();
                currentLnd.Scheduled_Payment_Date = xml.GetNodeValue(param, "BillingDate").ToDateTime();
                currentLnd.Actual_Payment_Amount = xml.GetNodeValue(param, "ThisMonthActualRepayAmount").ToDecimal();
                currentLnd.Recent_Pay_Date = xml.GetNodeValue(param, "TheLastestRepayDay").ToDateTime();
                currentLnd.Curr_Overdue_Cyc = xml.GetNodeValue(param, "CurrentOverdueNum").ToInt();
                currentLnd.Curr_Overdue_Amount = xml.GetNodeValue(param, "CurrentOverdueAmount").ToDecimal();
                //currentLnd.Overdue31_To60_Amount = xml.GetNodeValue(param, "Overdue31To60Days").ToDecimal();
                //currentLnd.Overdue61_To90_Amount = xml.GetNodeValue(param, "Overdue61To90Days").ToDecimal();
                //currentLnd.Overdue91_To180_Amount = xml.GetNodeValue(param, "Overdue91To180Days").ToDecimal();
                //currentLnd.Overdue_Over180_Amount = xml.GetNodeValue(param, "Overdue180Days").ToDecimal();
                string month24RepayTitle = xml.GetNodeValue(param, "Month24RepayTitle");
                if (!string.IsNullOrEmpty(month24RepayTitle))
                {
                    var array = month24RepayTitle.Split('-');
                    currentLnd.Payment_State_Begin_Month = array[0].Replace("年", ".").TrimEnd('月');
                    currentLnd.Payment_State_End_Month = array[1].Substring(0, 8).Replace("年", ".").TrimEnd('月');
                    currentLnd.Payment_State = xml.GetNodeValue(param, "Month24RepayStatus");
                }
                string OverdueTile = xml.GetNodeValue(param, "OverdueTile");
                if (!string.IsNullOrEmpty(OverdueTile))
                {
                    string beginDate = null;
                    string endDate = null;
                    GetBeginAndEndTime(OverdueTile, ref beginDate, ref endDate);
                    currentLnd.Overdue_Record_Begin_Month = beginDate;
                    currentLnd.Overdue_Record_End_Month = endDate;
                }
                var title = CreateStrData(currentLnd.Cue);
                currentLnd.Finance_Org = title.Finance_Org;
                currentLnd.Account_Dw = title.Account_Dw;
                //currentLnd.Type_Dw = title.Type_Dw;
                currentLnd.Currency = title.Currency;
                currentLnd.Open_Date = title.Open_Date.ToDateTime();
                //currentLnd.End_Date = title.End_Date.ToDateTime();
                currentLnd.Credit_Limit_Amount = title.Credit_Limit_Amount.ToDecimal();
                currentLnd.Guarantee_Type = title.Guarantee_Type;
                //currentLnd.Payment_Rating = title.Payment_Rating;
                //currentLnd.Payment_Cyc = title.Payment_Cyc;
                if (string.IsNullOrEmpty(currentLnd.State))
                    currentLnd.State = title.State;
                if (currentLnd.Share_Credit_Limit_Amount == null)
                    currentLnd.Share_Credit_Limit_Amount = title.Share_Credit_Limit_Amount.ToDecimal();
                currentLnd.GetTime = title.GetTime.ToDateTime();
                GetLoadcardLatest5YearOverdueRecord(currentLnd, param);//添加贷款逾期信息
                container.CRD_CD_LND.Add(currentLnd);
            };
            AddCurrentXmlNodesInfo("CreditCardInfo", aciton);


        }

        private void GetLoadcardLatest5YearOverdueRecord(CRD_CD_LNDEntity currentLnd, XmlNode param)
        {
            string istr = null;
            for (int i = 1; i < 3; i++)
            {
                istr = i.ToString();
                var overdueContMonthEles = xml.GetNodeList(param, "OverdueMonthNum" + istr);
                var overdueMonthEles = xml.GetNodeList(param, "OverdueMonth" + istr);
                var overdueAmountEles = xml.GetNodeList(param, "OverdueAmount" + istr);
                if (overdueContMonthEles.Count == 0)
                    break;
                if (overdueContMonthEles.Count == overdueMonthEles.Count && overdueContMonthEles.Count == overdueAmountEles.Count)
                {
                    int index = 0;
                    foreach (XmlNode item in overdueContMonthEles)
                    {
                        currentLnd.LndoverList.Add(new CRD_CD_LND_OVDEntity()
                        {
                            Last_Months = item.InnerText.ToDecimal(),
                            Month_Dw = overdueMonthEles[index].InnerText,
                            Amount = overdueAmountEles[index].InnerText.ToDecimal()
                        });
                        index++;
                    }
                }
                else
                {
                    throw new Exception("征信报告贷记卡逾期信息不对称肯定有问题");
                }
            }
        }
    
        #endregion

        #region 获取准贷记卡信息


        private void GetStandardLoancard()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_STNCARDEntity currentStn = new CRD_CD_STNCARDEntity();
                currentStn.Cue = xml.GetNodeValue(param, "TitleInfo");
                currentStn.State = xml.GetNodeValue(param, "AccountStatus");
                currentStn.Share_Credit_Limit_Amount = xml.GetNodeValue(param, "SharedCreditLimit").ToDecimal();
                currentStn.Used_Credit_Limit_Amount = xml.GetNodeValue(param, "OverdraftBalance").ToDecimal();
                currentStn.Latest6_Month_Used_Avg_Amount = xml.GetNodeValue(param, "Last6MonthsAveOverBalan").ToDecimal();
                currentStn.Used_Highest_Amount = xml.GetNodeValue(param, "MaxOverBalan").ToDecimal();
                //  currentLnd.Remain_Payment_Cyc = xml.GetNodeValue(param, "RemainRepayNum").ToDecimal();
                currentStn.Scheduled_Payment_Amount = xml.GetNodeValue(param, "ThisMonthRepayAmount").ToDecimal();
                currentStn.Scheduled_Payment_Date = xml.GetNodeValue(param, "BillingDate").ToDateTime();
                currentStn.Actual_Payment_Amount = xml.GetNodeValue(param, "ThisMonthActualRepayAmount").ToDecimal();
                currentStn.Recent_Pay_Date = xml.GetNodeValue(param, "TheLastestRepayDay").ToDateTime();
               // currentStn.Curr_Overdue_Cyc = xml.GetNodeValue(param, "CurrentOverdueNum").ToInt();
                //currentStn.Curr_Overdue_Amount = xml.GetNodeValue(param, "CurrentOverdueAmount").ToDecimal();
                //currentLnd.Overdue31_To60_Amount = xml.GetNodeValue(param, "Overdue31To60Days").ToDecimal();
                //currentLnd.Overdue61_To90_Amount = xml.GetNodeValue(param, "Overdue61To90Days").ToDecimal();
                //currentLnd.Overdue91_To180_Amount = xml.GetNodeValue(param, "Overdue91To180Days").ToDecimal();
                currentStn.OVERDUE_OVER180_AMOUNT = xml.GetNodeValue(param, "OverOver180UnpaidBalan").ToDecimal();//透支180天以上未付余额
                string month24RepayTitle = xml.GetNodeValue(param, "Month24RepayTitle");
                if (!string.IsNullOrEmpty (month24RepayTitle))
                {
                    var array = month24RepayTitle.Split('-');
                    currentStn.Payment_State_Begin_Month = array[0].Replace("年", ".").TrimEnd('月');
                    currentStn.Payment_State_End_Month = array[1].Substring(0, 8).Replace("年", ".").TrimEnd('月');
                    currentStn.Payment_State = xml.GetNodeValue(param, "Month24RepayStatus");
                }
                string OverdueTile = xml.GetNodeValue(param, "OverdueTile");
                if (!string.IsNullOrEmpty(OverdueTile))
                {
                    string beginDate = null;
                    string endDate = null;
                    GetBeginAndEndTime(OverdueTile, ref beginDate, ref endDate);
                    currentStn.Overdue_Record_Begin_Month = beginDate;
                    currentStn.Overdue_Record_End_Month = endDate;
                }
                var title = CreateStrData(currentStn.Cue);
                currentStn.Finance_Org = title.Finance_Org;
                currentStn.Account_Dw = title.Account_Dw;
                //currentStn.Type_Dw = title.Type_Dw;
                currentStn.Currency = title.Currency;
                currentStn.Open_Date = title.Open_Date.ToDateTime();
                //currentStn.End_Date = title.End_Date.ToDateTime();
                currentStn.Credit_Limit_Amount = title.Credit_Limit_Amount.ToDecimal();
                currentStn.Guarantee_Type = title.Guarantee_Type;
                //currentStn.Payment_Rating = title.Payment_Rating;
                //currentStn.Payment_Cyc = title.Payment_Cyc;
                if (string.IsNullOrEmpty(currentStn.State))
                    currentStn.State = title.State;
                if (currentStn.Share_Credit_Limit_Amount == null)
                    currentStn.Share_Credit_Limit_Amount = title.Share_Credit_Limit_Amount.ToDecimal();
                currentStn.GetTime = title.GetTime.ToDateTime();
                GetStnLatest5YearOverdueRecord(currentStn, param);//添加贷款逾期信息
                container.CRD_CD_STNCARD.Add(currentStn);
            };
            AddCurrentXmlNodesInfo("QuasiCreditCard", aciton);
        }
        private void GetStnLatest5YearOverdueRecord(CRD_CD_STNCARDEntity stn, XmlNode param)
        {
            string istr = null;
            for (int i = 1; i < 3; i++)
            {
                istr = i.ToString();
                var overdueContMonthEles = xml.GetNodeList(param, "OverdueMonthNum" + istr);
                var overdueMonthEles = xml.GetNodeList(param, "OverdueMonth" + istr);
                var overdueAmountEles = xml.GetNodeList(param, "OverdueAmount" + istr);
                if (overdueContMonthEles.Count == 0)
                    break;
                if (overdueContMonthEles.Count == overdueMonthEles.Count && overdueContMonthEles.Count == overdueAmountEles.Count)
                {
                    int index = 0;
                    foreach (XmlNode item in overdueContMonthEles)
                    {
                        stn.StnoverList.Add(new CRD_CD_STN_OVDEntity()
                        {
                            Last_Months = item.InnerText.ToDecimal(),
                            Month_Dw = overdueMonthEles[index].InnerText,
                            Amount = overdueAmountEles[index].InnerText.ToDecimal()
                        });
                        index++;
                    }
                }
                else
                {
                    throw new Exception("征信报告贷记卡逾期信息不对称肯定有问题");
                }
            }
        }
     
        #endregion

        /// <summary>
        /// 获取对外担保信息
        /// </summary>
        private void GetExternalGuaranteeLoan()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_GUARANTEEEntity guarantee = new CRD_CD_GUARANTEEEntity()
                {
                    Organ_Name = xml.GetNodeValue(param, "GuarLoanIssuingAgency"),
                    Contract_Money = xml.GetNodeValue(param, "GuarLoanContractAmount").ToDecimal(),
                    Begin_Date = xml.GetNodeValue(param, "GuarLoanIssueDate").ToDateTime(),
                    End_Date = xml.GetNodeValue(param, "GuarLoanDueDate").ToDateTime(),
                    Guarantee_Money = xml.GetNodeValue(param, "GuarAmount").ToDecimal(),
                    Guarantee_Balance = xml.GetNodeValue(param, "GuarLoanPrincipalAmount").ToDecimal(),
                    Class5_State = xml.GetNodeValue(param, "GuarLoanFiveclasscode"),
                    Billing_Date = xml.GetNodeValue(param, "SettlementDate").ToDateTime()
                };
                container.CRD_CD_GUARANTEE.Add(guarantee);
            };
            AddCurrentXmlNodesInfo("PerGuaranteeInfo", aciton);
        }

        #endregion

        #region PublicInfo（公共信息）
        private void AnalysisPublicInfo()
        {
            //逾期及违约信息概要
            GetBeOverdueInfoOverview();
            //信用提示
            GetCreditPromptInfo();
            //获取公积金缴存记录
            GetAccFund();
            //获取养老缴存记录
            GetEndowmentInsuranceDeposit();
            //强制执行记录
            GetCompulsoryExecutionRecord();
            //保证人代偿信息
            GetGuarantorCompenInfo();
            //资产处置信息
            GetAssetDisposalInfo();

        }
        private void GetGuarantorCompenInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_ASRREPAYEntity entity = new CRD_CD_ASRREPAYEntity()
                {
                    Organ_Name = xml.GetNodeValue(param, "CompensationOrganization"),
                    Latest_Assurer_Repay_Date = xml.GetNodeValue(param, "LastCompensationDate").ToDateTime(),
                    Money = xml.GetNodeValue(param, "CompensationSum").ToDecimal(),
                    Latest_Repay_Date = xml.GetNodeValue(param, "LastPayOffDate").ToDateTime(),
                    Balance = xml.GetNodeValue(param, "CompensationOverage").ToDecimal()
                };
                container.CRD_CD_ASRREPAY.Add(entity);
            };
            AddCurrentXmlNodesInfo("GuarantorCompenInfo", aciton);
        }
        private void GetAssetDisposalInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_ASSETDPSTEntity entity = new CRD_CD_ASSETDPSTEntity()
                {
                    Organ_Name = xml.GetNodeValue(param, "AssetManagementCo"),
                    Get_Time = xml.GetNodeValue(param, "DebtReceiveDate").ToDateTime(),
                    Money = xml.GetNodeValue(param, "ReceiveRightsAmount").ToDecimal(),
                    Latest_Repay_Date = xml.GetNodeValue(param, "LastPayOffDate"),
                    Balance = xml.GetNodeValue(param, "AssetDisposalOverage").ToDecimal()
                };
                container.CRD_CD_ASSETDPST.Add(entity);
            };
            AddCurrentXmlNodesInfo("AssetsDisposalInfo", aciton);
        }
        private void GetCompulsoryExecutionRecord()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_FORCEEXCTNEntity entity = new CRD_PI_FORCEEXCTNEntity()
                {
                    Court = xml.GetNodeValue(param, "ExecutiveCourt"),
                    Case_Reason = xml.GetNodeValue(param, "ExecutiveCaseReason"),
                    Register_Date = xml.GetNodeValue(param, "FilingDate").ToDateTime(),
                    Closed_Date = xml.GetNodeValue(param, "ClosedDate").ToDateTime(),
                    Closed_Type = xml.GetNodeValue(param, "ClosedWay"),
                    Case_State = xml.GetNodeValue(param, "CaseState"),
                    Enforce_Object = xml.GetNodeValue(param, "ApplyExecSubject"),
                    Enforce_Object_Money = xml.GetNodeValue(param, "ApplyExecSubjectValue").ToDecimal(),
                    Already_Enforce_Object = xml.GetNodeValue(param, "AlreadyExecSubject"),
                    Already_Enforce_Object_Money = xml.GetNodeValue(param, "AlreadyExecSubjectValue").ToDecimal(),

                };
                container.CRD_PI_FORCEEXCTN.Add(entity);
            };
            AddCurrentXmlNodesInfo("CompulsoryExecutionRecord", aciton);
        }
        private void GetBeOverdueInfoOverview()
        {

            Action<XmlNode> aciton = (param) =>
            {
                CRD_CD_OverDueBreakeEntity entity = new CRD_CD_OverDueBreakeEntity()
                {
                    BadBebtNum = xml.GetNodeValue(param, "BadDebtsInfoSumNumber").ToInt(),
                    BadBebtMoney = xml.GetNodeValue(param, "BadDebtsInfoSumAmount").ToDecimal(),
                    AssetDisposalNum = xml.GetNodeValue(param, "AssetsDisposalInfoSumNumber").ToInt(),
                    AssetDisposalBalance = xml.GetNodeValue(param, "AssetsDisposalInfoSumAmount").ToDecimal(),
                    GuarantorCompensatoryNum = xml.GetNodeValue(param, "GuarantorCompenInfoSumNumber").ToInt(),
                    GuarantorCompensatoryBalance = xml.GetNodeValue(param, "GuarantorCompenInfoSumAmount").ToDecimal()
                };
                container.CRD_CD_OverDueBreake.Add(entity);
            };
            AddCurrentXmlNodesInfo("BeOverdueInfoOverview", aciton);
        }

        private void GetCreditPromptInfo()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_IS_CREDITCUEEntity entity = new CRD_IS_CREDITCUEEntity()
                {
                    PersonalLoancard_Count = xml.GetNodeValue(param, "PersonalHouseLoanNum").ToDecimal(),
                    House_Loan_Count = xml.GetNodeValue(param, "HouseLoanNum").ToDecimal(),
                    Other_Loan_Count = xml.GetNodeValue(param, "OtherLoanNum").ToDecimal(),
                    First_Loan_Open_Month = xml.GetNodeValue(param, "FristLoanMonth"),
                    Loancard_Count = xml.GetNodeValue(param, "CreditCardAccountNum").ToDecimal(),
                    First_Loancard_Open_Month = xml.GetNodeValue(param, "FristCreditCardMonth"),
                    Standard_Loancard_Count = xml.GetNodeValue(param, "QuasiCreditCardNum").ToDecimal(),
                    First_Sl_Open_Month = xml.GetNodeValue(param, "QuasiCreditCardMonth"),
                    Announce_Count = xml.GetNodeValue(param, "DeclareNum").ToDecimal(),
                    Dissent_Count = xml.GetNodeValue(param, "DissentNum").ToDecimal()

                };
                container.CRD_IS_CREDITCUE.Add(entity);
            };
            AddCurrentXmlNodesInfo("CreditPromptInfo", aciton);

        }
        private void GetAccFund()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_ACCFUNDEntity accfund = new CRD_PI_ACCFUNDEntity()
                {
                    Area = xml.GetNodeValue(param, "PayArea"),
                    Register_Date = xml.GetNodeValue(param, "PayDate"),
                    First_Month = xml.GetNodeValue(param, "FirstPayMonth"),
                    To_Month = xml.GetNodeValue(param, "PayToMonth"),
                    State = xml.GetNodeValue(param, "PayState"),
                    Pay = xml.GetNodeValue(param, "MonthPayDeposit").ToDecimal(),
                    Own_Percent = xml.GetNodeValue(param, "PersonPayProportion"),
                    Com_Percent = xml.GetNodeValue(param, "CompanyPayProportion"),
                    Organ_Name = xml.GetNodeValue(param, "PayCompany"),
                    Get_Time = xml.GetNodeValue(param, "InfoUpdateDate").ToDateTime()
                };
                container.CRD_PI_ACCFUND.Add(accfund);
            };
            AddCurrentXmlNodesInfo("HousingReservePayRecord", aciton);

        }
        private void GetEndowmentInsuranceDeposit()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_PI_ENDINSDPTEntity deposit = new CRD_PI_ENDINSDPTEntity()
                {
                    Area = xml.GetNodeValue(param, "PayArea"),
                    Register_Date = xml.GetNodeValue(param, "PayDate"),
                    Month_Duration = xml.GetNodeValue(param, "CumulativePayMonths").ToDecimal(),
                    Work_Date = xml.GetNodeValue(param, "WorkMonth"),
                    State = xml.GetNodeValue(param, "PayState"),
                    Own_Basic_Money = xml.GetNodeValue(param, "PersonPayBase").ToDecimal(),
                    Money = xml.GetNodeValue(param, "ThisMonthPayAmount").ToDecimal(),
                    Organ_Name = xml.GetNodeValue(param, "PayCompany"),
                    Pause_Reason = xml.GetNodeValue(param, "CancelPayReason"),
                    Get_Time = xml.GetNodeValue(param, "InfoUpdateDate").ToDateTime()
                };
                container.CRD_PI_ENDINSDPT.Add(deposit);
            };
            AddCurrentXmlNodesInfo("PensionInsurancePayRecord", aciton);
        }


        #endregion

        #region QueryRecord（查询记录）
        private void AnalysisQueryRecord()
        {
            Action<XmlNode> aciton = (param) =>
            {
                CRD_QR_RECORDDTLINFOEntity queryRecord = new CRD_QR_RECORDDTLINFOEntity()
                {
                    Query_Date = xml.GetNodeValue(param, "QueryDate").ToDateTime(),
                    Querier = xml.GetNodeValue(param, "QueryOperator"),
                    Query_Reason = xml.GetNodeValue(param, "QueryReason")

                };
                container.CRD_QR_RECORDDTLINFO.Add(queryRecord);
            };
            AddCurrentXmlNodesInfo("CreditLoanApproQueryDetailed", aciton);

        }
        #endregion


        #endregion
                                                                                             
        #region 私有公共方法


        #region 解析标题字段
        
        private TitleStrEntity CreateStrData(string str)
        {
            TitleStrEntity titleEntity = new TitleStrEntity();
            titleEntity.Open_Date = GetOpenDate(str);
            titleEntity.Finance_Org = GetfieldData(str, "“", "”发放的");
            GetCurrency(titleEntity,str);//获取币种
            titleEntity.Account_Dw = GetfieldData(str, "业务号", "，");
            var value = GetfieldData(str, "授信额度", "元");
            if (value == null)
            {
                titleEntity.Credit_Limit_Amount = GetfieldData(str, "发放的", "元（");
            }
            else
                titleEntity.Credit_Limit_Amount = value.TrimStart("折合人民币".ToCharArray());
            titleEntity.Guarantee_Type = GetGuarantee_Type(str, "担保");
            titleEntity.Type_Dw = GetType_Dw(str);//贷款种类
            titleEntity.State = GetfieldData(str, "账户状态为“", "”");
            var values = GetfieldData(str, "截至", "，");
            if (values == null)
            {
                values = GetfieldData(str, "截止", "，");
                if (values == null)
                    titleEntity.GetTime = null;
                else
                {
                    titleEntity.GetTime = values.Replace("年", "-").Replace("月", "-").Replace("日", "");
                }
            }
            else
                titleEntity.GetTime = values.Replace("年", "-").Replace("月", "-").Replace("日", "");
            var val = GetfieldData(str, "共享授信额度", "元");
            if (val == null)
                titleEntity.Share_Credit_Limit_Amount = null;
            else
                titleEntity.Share_Credit_Limit_Amount = val.TrimStart("折合人民币".ToCharArray());

            titleEntity.Payment_Cyc = GetLoanDes(str, "期，", "期，");
            titleEntity.Payment_Rating = GetLoanDes(str, "归还");
            var res = GetLoanDes(str, "日到期", "日到期");
            if (res == null)
                titleEntity.End_Date = null;
            else
                titleEntity.End_Date = res.Replace("年", "-").Replace("月", "-");
            TryGetStateAndGettime(titleEntity, str);
            return titleEntity;
        }
        private void  TryGetStateAndGettime( TitleStrEntity title,string titleStr)
        {
            if(title.GetTime==null)
            {
                string time = null;
                string state = null;
                if (titleStr.IndexOf("已于") != -1)
                {
                     time=  GetfieldData(titleStr, "已于", "日");
                    
                    if(time==null)
                    {
                        var str= GetfieldData(titleStr, "已于", "月");
                        state = GetfieldData(titleStr, "已于" + str + "月", "。");
                        time = str.Replace("年", ".");
                    }
                    else
                    {
                        state = GetfieldData(titleStr, "已于" + time + "日", "。");
                        time = time.Replace("年", "-").Replace("月", "-");

                    }
                }
                else if (titleStr.IndexOf("未激活") != -1)
                {
                    state = "未激活";
                    time = GetfieldData(titleStr, "截止", "尚未激活").Replace("年", "-").Replace("月", "-").Replace("日", "");
                }
                else
                {
                    var str= titleStr.Split(',').Last();
                    if(str.IndexOf("日")!=-1)
                    {
                        time = GetfieldData(titleStr, str.Substring(0,2), "日").Replace("年", "-").Replace("月", "-");
                        state = GetfieldData(titleStr, "日", "。");
                    }
                    else
                    {
                        time = GetfieldData(titleStr, str.Substring(0, 2), "月").Replace("年", "-");
                        state = GetfieldData(titleStr, "日", "。");
                    }
                   
                }
                title.GetTime = time;
                title.State = state ;
            }
        }
    
        private void GetCurrency(TitleStrEntity titleEntity, string str)
        {
            var cur= GetfieldData(str, "（", "）");
            if(cur==null)
            {
                cur = GetfieldData(str, "(", "）");
                if(cur==null)
                {
                    cur = GetfieldData(str, "(", ")");
                    if(cur==null)
                    {
                        cur = GetfieldData(str, "（", ")");
                        if(cur==null)
                        {
                            return;
                        }
                    }

                }

            }
            titleEntity.Currency = (cur??string.Empty).TrimEnd("账户".ToCharArray());
        }
        string GetLoanDes(string str, string lastStr, string trimStr = null)
        {
            var result = GetGuarantee_Type(str, lastStr);
            if (result == null || trimStr == null)
                return result;
            else
            {
                var res = result.ToString();
                if (res.LastIndexOf(trimStr) != -1)
                    return res.Remove(res.LastIndexOf(trimStr));
                else
                    return null;
            }

        }
        string GetType_Dw(string str)
        {
            var value = GetfieldData(str, "）", "贷款");
            if (value == null)
            {
                return null;
            }
            else
            {
                return value + "贷款";
            }
        }
        string GetOpenDate(string str)
        {
            DateTime dt = DateTime.Now;
            var opendate = str.Substring(str.IndexOf('.') + 1, 11).Replace("年", "-").Replace("月", "-").Replace("日", "");
            if (DateTime.TryParse(opendate, out dt))
                return opendate;
            return null;


        }
        void GetBeginAndEndTime(string title,ref string beginDate,ref string endDate)
        {
            var array = title.Split('-');
            beginDate= array[0].Replace("年", ".").TrimEnd('月');
            endDate = array[1].Substring(0, 8).Replace("年", ".").TrimEnd('月');
        }
        string GetGuarantee_Type(string str, string endUnitStr)
        {
            int index = str.IndexOf(endUnitStr);
            if (index == -1)
            {
                if (endUnitStr == "担保")
                {
                    index = str.IndexOf("保证");
                    if (index == -1)
                    {
                        index = str.IndexOf("保");
                        if (index == -1)
                            return null;
                        else
                            endUnitStr = "保";
                    }
                }
                else
                {
                    return null;
                }
            }
            var  mark= str.Substring(0, index);
            int lastindex = mark.LastIndexOf("，");
            int last = mark.LastIndexOf(",");
            if(lastindex<last)
            {
                lastindex = last;
            }
            return str.Substring(lastindex + 1, index - lastindex + endUnitStr.Length - 1);
        }
        string GetfieldData(string loadStr, string start, string end)
        {
            string value = Commonfunction.GetMidStr(loadStr, start, end);
            if (value == "")
                return null;
            return value;

        }
        #endregion
      

      
        private void AddCurrentXmlNodesInfo(string xpath, Action<XmlNode> action)
        {
            var nodes = xml.GetCurrentXmlNodeNodeList(xpath);
            if (nodes == null || nodes.Count == 0)
                return;
            foreach (XmlNode item in nodes)
            {
                action(item);
            }
        }
        private byte GetOverdue_Cyc(string payment_state)
        {
            if (string.IsNullOrEmpty(payment_state))
                return 0;
            return (byte)Regex.Matches(payment_state, "[1-7]").Count;

        }

        /// <summary>
        /// 证件类型
        /// </summary>
        private readonly  Dictionary<string, string> errorCodeDic = new Dictionary<string, string>(){
            {"0001","入库出错，退回"},
            {"0002","征信报告查询失败"},
            {"0003","请求报文有误"},
            {"0004","报文头解析错误"},
            {"0005","报文体格式错误"},
            {"0006","查询申请有待审批"},
            {"0008","该金融机构未签接口转换合同"},
            {"0009","请求人行报告失败"},
            {"0010","登录人行账号密码错误"},
            {"0011","登录人行cookie失效"},

            {"0100","请求报文不能为空"},
            {"0101","解析请求报文失败"},
            {"0102","非法交易码TRADE_TYPE"},
            {"0103","节点TRADE_TYPE不能为空"},
            {"0106","申请金融机构代码不符"},
            {"0108","申请操作用户无查询接口权限"},
            {"0109","申请操作用户和密码不符"},

            {"0114","报告已删除"},
            {"0115","审核中"},
            {"0116","被驳回"},
            {"0117","查询申请已审批，正在查询！"},
            {"0118","后台加载失败！"},

            {"1000","个人业务证件类型错误"},
            {"1001","个人业务证件号码错误"},
            {"1002","个人业务电子签名中姓名校验错误"},
            {"1003","个人业务电子签名中证件号码校验错误"},
            {"1004","个人业务电子签名中验证日期校验错误"},
            {"1005","个人业务电子签名中机构代码校验错误"},
            {"1006","个人业务电子签名中银联授权码校验错误"},
            {"1007","个人业务查询类型错误"},
            {"1008","个人业务查询原因错误"},
            {"1009","个人业务查询报告版式错误"},
            {"10010","个人业务查询类型错误"},
            {"1011","个人业务电子签名解密错误"},
            {"1012","个人业务电子签名配置不能为空SEARCH_APPROVE"},
            {"1013","个人业务电子签名系统中没有A/B/C级审批人员，请先添加拥有A/B/C级权限的用户，否则申请无效"},
            {"1014","个人业务电子签名签署授权书的时间有误"},
            {"1015","个人业务电子签名查询发起类型有误"},
            {"1016","个人业务该申请人所有账户已结清或不存在开户信息"},
            {"1017","个人业务电子签名三项标示衍生码（姓名、证件号码、证件类型）验签错误"},
            {"1018","个人业务电子签名身份认证平台公钥配置不能为空"},
            {"1019","个人业务电子签名授权码信息验签错误"},
            {"1020","个人业务电子签名身份认证有误"},
            {"1021","个人业务扫描件配置SEARCH_APPROVE、CREDIT_CUSTCERT_DIR、UPLOADIMG_SIZE不能为空"},
            {"1022","个人业务扫描件解析授权书、身份证扫描文件错误"},
            {"1023","个人业务扫描件上传的附件太大，超过\"读配置\"KB，请重新上传"},
            {"1024","个人业务扫描件上传的附件暂时只支持jpg、png、bmp、gif"},


        };

        #region  信用报告版本
        private const string 银行版 = "30";
        private const string 银行异议版 = "31";
        #endregion

        #region  查询原因
        private const string 贷后管理 = "01";
        private const string 贷款审批 = "02";
        private const string 信用卡审批 = "03";
        private const string 异议核查 = "05";
        private const string 担保资格审查 = "08";
        private const string 公积金提取复核 = "16";
        private const string 特约商户实名审查 = "19";
        #endregion

        #region  查询版式
        private const string 信用报告查询 = "0";
        private const string 身份信息核查 = "1";
        private const string 身份信息查询 = "2";
        private const string 信用报告查询含身份信息核查 = "3";
        private const string 信用报告查询含身份信息查询 = "4";
        #endregion

        #region  交易种类
        private const string 个人电子签名 = "8000";
        private const string 个人扫描签名 = "8001";
        private const string 企业电子签名 = "8002";
        private const string 企业扫描签名 = "8003";
        private const string 个人电子或者扫描 = "8009";
        #endregion

        #endregion
    }
}
