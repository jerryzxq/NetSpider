using System;
using System.ComponentModel.DataAnnotations;

namespace Vcredit.ExternalCredit.Attributes
{
	/// <summary>
	/// 姓名格式校验
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public class NameCheckAttribute : ValidationAttribute
	{
		private static string DefaultErrorMessage = "姓名格式错误";

		public NameCheckAttribute() : base(DefaultErrorMessage)
		{
		}

		public override string FormatErrorMessage(string name)
		{
			return DefaultErrorMessage;
		}

		public override bool IsValid(object value)
		{
			if (value == null)
				return true;

			var cardNo = value.ToString();
			if (cardNo.IndexOf(' ') >= 0)  //验证姓名合法性
			{
				DefaultErrorMessage = "姓名不能包含空格";
				return false;
			}
			return true;
		}


	}
}
