using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Vcredit.Common.Ext;

namespace Vcredit.ExtTrade.CommonLayer
{
    public class ConfigData
    {

        public static readonly string ftpHost = ConfigurationManager.AppSettings["FtpAddress"];

        public static readonly string ftpUser = ConfigurationManager.AppSettings["FtpUserName"];

        public static readonly string ftpPassword = ConfigurationManager.AppSettings["FtpPassword"];

        public static readonly string localFileSaveDir = ConfigurationManager.AppSettings["LocalFileSaveDir"];

        public static readonly string uploadFTPPath = ConfigurationManager.AppSettings["UploadFTPPath"];

        public static readonly string downLoadFTPPath = ConfigurationManager.AppSettings["DownLoadFTPPath"];

        public static readonly string MappingTablePath = ConfigurationManager.AppSettings["MappingTablePath"];

        public static readonly string todayDealingPoint = ConfigurationManager.AppSettings["TodayDealingPoint"];

        public static readonly string requestFileSavePath = ConfigurationManager.AppSettings["RequestFileSavePath"];

        public static readonly string requestState = ConfigurationManager.AppSettings["RequestState"];

        public static readonly string directoryNames = ConfigurationManager.AppSettings["DirectoryNames"];

        public static readonly string LocalReadDateDirectroy = ConfigurationManager.AppSettings["LocalReadDateDirectroy"];

        public static readonly string FTPDownLoadDateDirectroy = ConfigurationManager.AppSettings["FTPDownLoadDateDirectroy"];

        public static readonly string RequestedFileSavePath = ConfigurationManager.AppSettings["RequestedFileSavePath"];

        public static readonly string jinCityCreditXmlPath = ConfigurationManager.AppSettings["jinCityCreditXmlPath"];

        public static readonly string userCode = ConfigurationManager.AppSettings["UserCode"];

        public static readonly string orgCode = ConfigurationManager.AppSettings["OrgCode"];

        public static readonly string cert_Nos = ConfigurationManager.AppSettings["Cert_Nos"];

        public static readonly int upLoadLimitedDays = (ConfigurationManager.AppSettings["UpLoadLimitedDays"]).ToInt(30);
        /// <summary>
        /// 担保重复查询时间限制
        /// </summary>
        public static readonly int reTryLimitedDays = (ConfigurationManager.AppSettings["ReTryLimitedDays"]).ToInt(30);

        /// <summary>
        /// 担保征信登陆重试次数
        /// </summary>
        public static readonly int tryLoginCount = (ConfigurationManager.AppSettings["TryLoginCount"]).ToInt(5);

        /// <summary>
        /// 担保征信报告下载路径
        /// </summary>
        public static readonly string AssureDownloadReportPath = ConfigurationManager.AppSettings["DownloadReportFilePath"];
        /// <summary>
        /// 外贸征信一天最多处理的请求的数量
        /// </summary>
        public static readonly int everyDayMaxDealingNum = ConfigurationManager.AppSettings["EveryDayMaxDealingNum"].ToInt(100);

        /// <summary>
        /// 担保征信时间限制
        /// </summary>
        public static string[] TimeLimit
        {
            get
            {
                var limits = ConfigurationManager.AppSettings["TimeLimit"].Split(new[] { ',' });

                return limits;
            }
        }

        /// <summary>
        /// 验证码解析程序tmpe路径
        /// </summary>
        public static readonly string AnalysisVCodeTempPath = ConfigurationManager.AppSettings["AnalysisVCodeTempPath"];

        /// <summary>
        /// 新外贸人行查询webservice url
        /// </summary>
        public static readonly string ForeignWebserviceUrl = ConfigurationManager.AppSettings["ForeignWebserviceUrl"];
        /// <summary>
        /// 0调用单笔接口查询，1调用批量接口查询
        /// </summary>
        public static string SwitchInterfaceNum
        {
            get
            {
                 var num= ConfigurationManager.AppSettings["SwitchInterfaceNum"];
                 if (string.IsNullOrEmpty(num))
                     return "0";
                  return num;
            }
        }
        /// <summary>
        ///资料来源 ,一般应该为外部系统代码
        /// </summary>
        public static readonly string Source = ConfigurationManager.AppSettings["Source"];
        /// <summary>
        ///业务类型
        /// </summary>
        public static readonly string BussType = ConfigurationManager.AppSettings["BussType"];
        /// <summary>
        ///查询Ids
        /// </summary>
        public static readonly string Ids = ConfigurationManager.AppSettings["Ids"];


        /// <summary>
        ///报文发起人
        /// </summary>
        public static readonly string SenderID = ConfigurationManager.AppSettings["SenderID"];

        /// <summary>
        ///报文接收人
        /// </summary>
        public static readonly string Receiver = ConfigurationManager.AppSettings["Receiver"];

        /// <summary>
        ///接收系统号
        /// </summary>
        public static readonly string ReceiverID = ConfigurationManager.AppSettings["ReceiverID"];

        /// <summary>
        ///操作用户密码
        /// </summary>
        public static readonly string UserCodePw = ConfigurationManager.AppSettings["UserCodePw"];
        /// <summary>
        ///上海小贷web服务
        /// </summary>
        public static readonly string ShangHaiLoanWebUrl = ConfigurationManager.AppSettings["ShangHaiLoanWebUrl"];
        /// <summary>
        ///省级平台公钥地址
        /// </summary>
        public static readonly string ProvicePlanPublicKeyPath = ConfigurationManager.AppSettings["ProvicePlanPublicKeyPath"];
        /// <summary>
        ///上海小贷征信查询发送请求方
        /// </summary>
        public static readonly string Sender = ConfigurationManager.AppSettings["Sender"];
        /// <summary>
        ///保存上海小贷返回的解密后的xml
        /// </summary>
        public static readonly string ShanghaiResponeSavePath = ConfigurationManager.AppSettings["ShanghaiResponeSavePath"];

     //   public static readonly string xmlSavePath = ConfigurationManager.AppSettings["XmlSavePath"];

        /// <summary>
        /// 手写签名校验地址
        /// </summary>
        public static readonly string ComplianceServiceUrl = ConfigurationManager.AppSettings["ComplianceServiceUrl"];

		/// <summary>
		/// 查询次数开关
		/// </summary>
		public static string ApplyLimitSwitch
		{
			get
			{
				var strSwitch = ConfigurationManager.AppSettings["ApplyLimitSwitch"];
				if (string.IsNullOrEmpty(strSwitch))
					return string.Empty;
				return strSwitch.ToUpper();
			}
		}

		/// <summary>
		/// 二次送查开关
		/// </summary>
		public static string TwoQuerySwitch
		{
			get
			{
				var strSwitch = ConfigurationManager.AppSettings["TwoQuerySwitch"];
				if (string.IsNullOrEmpty(strSwitch))
					return string.Empty;
				return strSwitch.ToUpper();
			}
		}
        /// <summary>
        /// 外贸征信请求批次号
        /// </summary>
        public static readonly string BatNo = ConfigurationManager.AppSettings["BatNo"];
        /// <summary>
        /// 上海小贷html保存路径
        /// </summary>
        public static readonly string HtmlSavePath = ConfigurationManager.AppSettings["HtmlSavePath"];
        /// <summary>
        /// SendKafkaTopic
        /// </summary>
        public static readonly string SendKafkaTopic = ConfigurationManager.AppSettings["SendKafkaTopic"];
        /// <summary>
        /// SendKafkaService
        /// </summary>
        public static readonly string SendKafkaService = ConfigurationManager.AppSettings["SendKafkaService"];

        /// <summary>
        /// 身份证过期修正天数
        /// </summary>
        public static int CertReviseDays
        {
            get
            {
               var  days= ConfigurationManager.AppSettings["CertReviseDays"].ToInt();
                if(days==null)
                    throw new Exception ("过期修正天数配置信息不对");

                return days.Value;
            }
        }
        	

    }
}
