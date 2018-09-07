using System.Collections.Generic;
using System.Linq;
using ServiceStack.OrmLite;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity.YiHaoDian;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business.YiHaoDian
{
	public class YhdUserinfoBll : Business<YhdUserinfoEntity, SqlConnectionFactory>
	{
		public static readonly YhdUserinfoBll Initialize = new YhdUserinfoBll();
		YhdUserinfoBll() { }

		public YhdUserinfoEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<YhdUserinfoEntity> List()
		{
			return Select();
		}

		public List<YhdUserinfoEntity> List(SqlExpression<YhdUserinfoEntity> expression)
		{
			return Select(expression);
		}

		public YhdUserinfoEntity GetByToken(string token)
		{
			return this.Select(x => x.Token == token).FirstOrDefault();
		}

		#region 判断是否实名认证
		/// <summary>
		/// 判断是否实名认证
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		public bool IsAuth(EmallCheckIsRealNameReq req)
		{
			var bResult = false;
			var user = this.Select(x => x.Token == req.Token).FirstOrDefault();
			if (user == null)
			{
				return bResult;
			}
			// 个人信息中：真是姓名，身份证，如果个人信息中认证身份证号，认证姓名为空通过下面 else 中根据地址数据判断
			if (!user.IdentityCard.IsEmpty() && !user.AuthName.IsEmpty())
			{
				bResult = FrameWorkCommon.IsAuthV2(req.Name, user.AuthName, req.IdentityCard, user.IdentityCard);
			}
			// 根据地址中的姓名，手机号判断
			else
			{
				bResult = this.CheckUserAddress(req, user);
			}
			return bResult;
		}

		private bool CheckUserAddress(EmallCheckIsRealNameReq req, YhdUserinfoEntity user)
		{
			var addrs = YhdReceiveaddressBll.Initialize.Select(x => x.Id == user.Id);
			if (addrs == null || !addrs.Any())
				return false;

			var result = false;
			// 姓名或手机号码一个匹配即可
			if (addrs.Where(x => x.Receiver.Equals(req.Name)).FirstOrDefault() != null)
			{
				result = true;
			}
			else
			{
				foreach (var addr in addrs)
				{
					result = FrameWorkCommon.CheckMobilePartlyMatch(req.Mobile, addr.Mobile);
					if (result)
						return result;
				}
			}

			return result;
		}

		#endregion
	}
}
