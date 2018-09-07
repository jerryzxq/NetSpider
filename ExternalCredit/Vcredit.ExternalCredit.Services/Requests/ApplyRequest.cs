
namespace Vcredit.ExternalCredit.Services.Requests
{
	/// <summary>
	/// 合规申请接口 request
	/// </summary>
	public class ApplyRequest
	{
		public ApplyRequest()
		{
			if (this.ApplyInfo == null)
			{
				this.ApplyInfo = new ApplyInfoEntity();
			}
		}

		/// <summary>
		/// 姓名
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 身份证号码
		/// </summary>
		public string IdentityNo { get; set; }

		/// <summary>
		/// 客户签名时间(格式为：yyyy-MM-dd HH:mm:ss)
		/// </summary>
		public string CustSignatrueTime { get; set; }

		/// <summary>
		/// 产品种类
		/// </summary>
		public string ProductKind { get; set; }

		/// <summary>
		/// 平台来源
		/// </summary>
		public string Source { get; set; }

		/// <summary>
		/// 手写签名图片文件名(包含后缀名)
		/// </summary>
		public string WritingImgName { get; set; }

		/// <summary>
		/// 手写签名图片（byte[] Base64编码）
		/// </summary>
		public string WritingImg { get; set; }

		/// <summary>
		/// CFCA签名盖章的地区（可传空，传空时默认取“上海”)
		/// </summary>
		public string SealLocation { get; set; }

		/// <summary>
		/// 身份证正面文件名(包含后缀名)
		/// </summary>
		public string IdCardForntImgName { get; set; }

		/// <summary>
		/// 身份证正面（byte[] Base64编码）
		/// </summary>
		public string IdCardForntImg { get; set; }

		/// <summary>
		/// 身份证反面文件名(包含后缀名)
		/// </summary>
		public string IdCardReverseImgName { get; set; }

		/// <summary>
		/// 身份证反面（byte[] Base64编码）
		/// </summary>
		public string IdCardReverseImg { get; set; }

		/// <summary>
		/// 手持身份证文件名(包含后缀名)
		/// </summary>
		public string IdCardHandImgName { get; set; }

		/// <summary>
		/// 手持身份证（byte[] Base64编码）
		/// </summary>
		public string IdCardHandImg { get; set; }
        /// <summary>
        /// 其他需要生成的文件：例如数据采集及芝麻信用授权服务协议,逗号分隔(ZMXYXINXISHOUQUANSHU)
        /// </summary>
        public string ExtDocuments { get; set; }
		/// <summary>
		/// 客户申请实体信息
		/// </summary>
		public ApplyInfoEntity ApplyInfo { get; set; }
	}

	/// <summary>
	/// 客户申请实体信息
	/// </summary>
	public class ApplyInfoEntity
	{
		/// <summary>
		/// 姓名
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 身份证
		/// </summary>
		public string IdentityNo { get; set; }

		/// <summary>
		/// 客户来源(产品名称，若为贷贷看则传“手机贷”)
		/// </summary>
		public string CustomerSource { get; set; }

		/// <summary>
		/// 希望贷款额（元）
		/// </summary>
		public string LoanMoney { get; set; }

		/// <summary>
		/// 贷款用途
		/// </summary>
		public string LoanPurpose { get; set; }

		/// <summary>
		/// 贷款期限
		/// </summary>
		public string LoanPeriod { get; set; }

		/// <summary>
		/// 性别
		/// </summary>
		public string Sex { get; set; }

		/// <summary>
		/// 学历
		/// </summary>
		public string Education { get; set; }

		/// <summary>
		/// 婚姻状况
		/// </summary>
		public string Marriage { get; set; }

		/// <summary>
		/// 居住地址详细地址
		/// </summary>
		public string Address { get; set; }

		/// <summary>
		/// 住宅电话号码
		/// </summary>
		public string Phone { get; set; }

		/// <summary>
		/// 手机
		/// </summary>
		public string Mobile { get; set; }

		/// <summary>
		/// 电子邮箱
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		/// 社保信息
		/// </summary>
		public string SocialSecurityInfo { get; set; }

		/// <summary>
		/// 公积金信息
		/// </summary>
		public string ProvidentFundInfo { get; set; }

		/// <summary>
		/// 工作单位名称
		/// </summary>
		public string Comp { get; set; }

		/// <summary>
		/// 职位名称
		/// </summary>
		public string CompPosi { get; set; }

		/// <summary>
		/// 单位地址详细地址
		/// </summary>
		public string CompAddr { get; set; }

		/// <summary>
		/// 单位电话号码
		/// </summary>
		public string CompPhone { get; set; }

		/// <summary>
		/// 工资收入（元）
		/// </summary>
		public string IncomeMonth { get; set; }

		/// <summary>
		/// 每月发薪日
		/// </summary>
		public string PaidOn { get; set; }

		/// <summary>
		/// 工资发放形式
		/// </summary>
		public string PayWay { get; set; }

		/// <summary>
		/// 联系人姓名
		/// </summary>
		public string Cont { get; set; }

		/// <summary>
		/// 联系人与贷款人关系
		/// </summary>
		public string ContRelation { get; set; }

		/// <summary>
		/// 联系人手机号码
		/// </summary>
		public string ContMobile { get; set; }

	}

}
