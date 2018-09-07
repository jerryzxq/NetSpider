using System.ComponentModel.DataAnnotations;
using Vcredit.ExternalCredit.Attributes;

namespace Vcredit.ExternalCredit.Dto
{
	/// <summary>
	/// 征信查询(合规申请)实体
	/// </summary>
	public class AssureApplyParamDto
	{
		/// <summary>
		/// 身份证号码
		/// </summary>
		//[Required(ErrorMessage = "身份证号码不能为空")]
		public string IdentityNo { get; set; }

		/// <summary>
		/// 姓名
		/// </summary>
		//[Required(ErrorMessage = ("姓名不能为空"))]
		public string Name { get; set; }

		/// <summary>
		/// 客户签名时间
		/// </summary>
		//[Required(ErrorMessage = ("客户签名时间不能为空"))]
		public string CustSignatrueTime { get; set; }

		/// <summary>
		/// 产品种类
		/// </summary>
		//[Required(ErrorMessage = "产品种类不能为空")]
		public string ProductKind { get; set; }

		/// <summary>
		/// 平台来源
		/// </summary>
		//[Required(ErrorMessage = "平台来源不能为空")]
		public string Source { get; set; }

		/// <summary>
		/// 手写签名图片文件名(包含后缀名)
		/// </summary>
		//[Required(ErrorMessage = "手写签名图片文件名(包含后缀名)不能为空")]
		public string WritingImgName { get; set; }

		/// <summary>
		/// 手写签名图片（byte[] Base64编码）
		/// </summary>
		//[Required(ErrorMessage = "手写签名图片（byte[] Base64编码）不能为空")]
		public string WritingImg { get; set; }

		/// <summary>
		/// CFCA签名盖章的地区（可传空，传空时默认取“上海”)
		/// </summary>
		//[Required(ErrorMessage = "CFCA签名盖章的地区（可传空，传空时默认取“上海”)不能为空")]
		public string SealLocation { get; set; }

		/// <summary>
		/// 身份证正面文件名(包含后缀名)
		/// </summary>
		//[Required(ErrorMessage = "身份证正面文件名(包含后缀名)不能为空")]
		public string IdCardForntImgName { get; set; }

		/// <summary>
		/// 身份证正面（byte[] Base64编码）
		/// </summary>
		//[Required(ErrorMessage = "身份证正面（byte[] Base64编码）不能为空")]
		public string IdCardForntImg { get; set; }

		/// <summary>
		/// 身份证反面文件名(包含后缀名)
		/// </summary>
		//[Required(ErrorMessage = "身份证反面文件名(包含后缀名)不能为空")]
		public string IdCardReverseImgName { get; set; }

		/// <summary>
		/// 身份证反面（byte[] Base64编码）
		/// </summary>
		//[Required(ErrorMessage = "身份证反面（byte[] Base64编码）不能为空")]
		public string IdCardReverseImg { get; set; }

		/// <summary>
		/// 手持身份证文件名(包含后缀名)
		/// </summary>
		//[Required(ErrorMessage = "手持身份证文件名(包含后缀名)不能为空")]
		public string IdCardHandImgName { get; set; }

		/// <summary>
		/// 手持身份证（byte[] Base64编码）
		/// </summary>
		//[Required(ErrorMessage = "手持身份证（byte[] Base64编码）不能为空")]
		public string IdCardHandImg { get; set; }
        /// <summary>
        /// 其他需要生成的文件：例如数据采集及芝麻信用授权服务协议,逗号分隔(ZMXYXINXISHOUQUANSHU)
        /// </summary>
        public string ExtDocuments { get; set; }
		/// <summary>
		/// 客户申请实体信息
		/// </summary>
		//[Required(ErrorMessage = "客户申请实体信息不能为空")]
		public ApplyInfoEntityDto ApplyInfo { get; set; }

	}

	/// <summary>
	/// 客户申请实体信息
	/// </summary>
	public class ApplyInfoEntityDto
	{
		/// <summary>
		/// 姓名
		/// </summary>
		//[Required(ErrorMessage = ("姓名不能为空"))]
		public string Name { get; set; }

		/// <summary>
		/// 身份证
		/// </summary>
		//[Required(ErrorMessage = ("身份证不能为空"))]
		public string IdentityNo { get; set; }

		/// <summary>
		/// 客户来源(产品名称，若为贷贷看则传“手机贷”)
		/// </summary>
		//[Required(ErrorMessage = ("客户来源(产品名称，若为贷贷看则传“手机贷”)不能为空"))]
		public string CustomerSource { get; set; }

		/// <summary>
		/// 希望贷款额（元）
		/// </summary>
		//[Required(ErrorMessage = ("希望贷款额（元）不能为空"))]
		public string LoanMoney { get; set; }

		/// <summary>
		/// 贷款用途
		/// </summary>
		//[Required(ErrorMessage = ("贷款用途不能为空"))]
		public string LoanPurpose { get; set; }

		/// <summary>
		/// 贷款期限
		/// </summary>
		//[Required(ErrorMessage = ("贷款期限不能为空"))]
		public string LoanPeriod { get; set; }

		/// <summary>
		/// 性别
		/// </summary>
		//[Required(ErrorMessage = ("性别不能为空"))]
		public string Sex { get; set; }

		/// <summary>
		/// 学历
		/// </summary>
		//[Required(ErrorMessage = ("学历不能为空"))]
		public string Education { get; set; }

		/// <summary>
		/// 婚姻状况
		/// </summary>
		//[Required(ErrorMessage = ("婚姻状况不能为空"))]
		public string Marriage { get; set; }

		/// <summary>
		/// 居住地址详细地址
		/// </summary>
		//[Required(ErrorMessage = ("居住地址详细地址不能为空"))]
		public string Address { get; set; }

		/// <summary>
		/// 住宅电话号码
		/// </summary>
		//[Required(ErrorMessage = ("住宅电话号码不能为空"))]
		public string Phone { get; set; }

		/// <summary>
		/// 手机
		/// </summary>
		//[Required(ErrorMessage = ("手机不能为空"))]
		public string Mobile { get; set; }

		/// <summary>
		/// 电子邮箱
		/// </summary>
		//[Required(ErrorMessage = ("电子邮箱不能为空"))]
		public string Email { get; set; }

		/// <summary>
		/// 社保信息
		/// </summary>
		//[Required(ErrorMessage = ("社保信息不能为空"))]
		public string SocialSecurityInfo { get; set; }

		/// <summary>
		/// 公积金信息
		/// </summary>
		//[Required(ErrorMessage = ("公积金信息不能为空"))]
		public string ProvidentFundInfo { get; set; }

		/// <summary>
		/// 工作单位名称
		/// </summary>
		//[Required(ErrorMessage = ("工作单位名称不能为空"))]
		public string Comp { get; set; }

		/// <summary>
		/// 职位名称
		/// </summary>
		//[Required(ErrorMessage = ("职位名称不能为空"))]
		public string CompPosi { get; set; }

		/// <summary>
		/// 单位地址详细地址
		/// </summary>
		//[Required(ErrorMessage = ("单位地址详细地址不能为空"))]
		public string CompAddr { get; set; }

		/// <summary>
		/// 单位电话号码
		/// </summary>
		//[Required(ErrorMessage = ("单位电话号码不能为空"))]
		public string CompPhone { get; set; }

		/// <summary>
		/// 工资收入（元）
		/// </summary>
		//[Required(ErrorMessage = ("工资收入（元）不能为空"))]
		public string IncomeMonth { get; set; }

		/// <summary>
		/// 每月发薪日
		/// </summary>
		//[Required(ErrorMessage = ("每月发薪日不能为空"))]
		public string PaidOn { get; set; }

		/// <summary>
		/// 工资发放形式
		/// </summary>
		//[Required(ErrorMessage = ("工资发放形式不能为空"))]
		public string PayWay { get; set; }

		/// <summary>
		/// 联系人姓名
		/// </summary>
		//[Required(ErrorMessage = ("联系人姓名不能为空"))]
		public string Cont { get; set; }

		/// <summary>
		/// 联系人与贷款人关系
		/// </summary>
		//[Required(ErrorMessage = ("联系人与贷款人关系不能为空"))]
		public string ContRelation { get; set; }

		/// <summary>
		/// 联系人手机号码
		/// </summary>
		//[Required(ErrorMessage = ("联系人手机号码不能为空"))]
		public string ContMobile { get; set; }

	}
}