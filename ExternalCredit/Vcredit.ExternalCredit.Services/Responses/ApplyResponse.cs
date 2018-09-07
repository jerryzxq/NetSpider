
namespace Vcredit.ExternalCredit.Services.Responses
{
	/// <summary>
	/// 合规申请接口 response
	/// </summary>
	public class ApplyResponse
	{
		/// <summary>
		/// 查询是否成功  布尔类型
		/// </summary>
		public bool IsSeccess { get; set; }

		/// <summary>
		/// 消息  字符串
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// 申请编号  整型
		/// </summary>
		public int ApplyID { get; set; }

		/// <summary>
		/// 客户姓名  字符串
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 客户身份证号  字符串
		/// </summary>
		public string IdentityNo { get; set; }

	}
}
