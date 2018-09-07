using System;
using Vcredit.Common.Ext;
using Vcredit.Framework.Config.Apollo;

namespace Vcredit.ExternalCredit.CommonLayer
{
	public class SysConfigs
	{
		/// <summary>
		/// 担保每天查询上限
		/// </summary>
		public static readonly int AssureDayLimitCount = GetApolloConfig("AssureDayLimitCount").ToInt(0);

		/// <summary>
		/// 从配置中心获取配置信息
		/// </summary>
		/// <param name="key"></param>
		private static string GetApolloConfig(string key)
		{
			string result = string.Empty;
			try
			{
				result = ApolloConfig.GetProperty(key);
			}
			catch (Exception e)
			{

			}
			return result;
		}
	}
}
