using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.CommonLayer.helper;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExtTrade.BusinessLayer
{
	/// <summary>
	/// 查询申请限制配置表
	/// </summary>
	public class CRD_CD_CreditApplyLimitBusiness
	{
		BaseDao dao = new BaseDao();

		public List<CRD_CD_CreditApplyLimitEntity> GetList(string strBusType, int iSourceType)
		{
			return dao.Select<CRD_CD_CreditApplyLimitEntity>(x => x.BusType == strBusType && x.SourceType == iSourceType);
		}

		/// <summary>
		/// 判断是否限制请求(分项目组)
		/// </summary>
		/// <param name="strBusType">业务类型</param>
		/// <param name="iSourceType">外贸、担保</param>
		/// <returns>true允许请求，false禁止提交请求</returns>
		public bool IsApply(string strBusType, int iSourceType)
		{
			if (ConfigData.ApplyLimitSwitch != "OPEN")
			{
				return true;
			}
			var strPrefix = "OrgCredit:";
			int iLimitCount;
			var limitCountKey = string.Format("LimitCount:{0}_{1}", strBusType, iSourceType);
			var limitCountValue = RedisHelper.GetCache(limitCountKey, strPrefix);
			if (limitCountValue != null)
			{
				iLimitCount = Convert.ToInt32(limitCountValue);
			}
			else
			{
				var applyLimitEntitys = this.GetList(strBusType, iSourceType);
				if (applyLimitEntitys != null && applyLimitEntitys.Any())
				{
					iLimitCount = applyLimitEntitys.FirstOrDefault().LimitCount;
				}
				else
				{
					throw new Exception("允许查询数量未配置");
				}
				RedisHelper.SetCache(limitCountKey, iLimitCount, strPrefix, 1440);
			}

			long lApplyCount;
			var applyCountKey = string.Format("ApplyCount:{0}:{1}_{2}", DateTime.Now.ToString("yyyyMMdd"), strBusType, iSourceType);
			var applyCountValue = RedisHelper.GetCache(applyCountKey, strPrefix);
			if (applyCountValue != null)
			{
				lApplyCount = Convert.ToInt32(applyCountValue) + 1;
			}
			else
			{
				var userInfoBusiness = new CRD_CD_CreditUserInfoBusiness();
				lApplyCount = userInfoBusiness.GetApplyCount(strBusType, iSourceType) + 1;
			}

			if (lApplyCount > iLimitCount)
			{
				return false;
			}

			RedisHelper.SetCache(applyCountKey, lApplyCount, strPrefix, 1440);

			return true;
		}

		/// <summary>
		/// 判断是否限制请求
		/// </summary>
		/// <param name="iSourceType">外贸、担保</param>
		/// <returns>true允许请求，false禁止提交请求</returns>
		public bool IsApply(int iSourceType)
		{
			if (ConfigData.ApplyLimitSwitch != "OPEN")
			{
				return true;
			}
			var strPrefix = "";
			int iLimitCount = SysConfigs.AssureDayLimitCount;
			if (iLimitCount == 0)
			{
				throw new Exception("允许查询数量未配置");
			}

			long lApplyCount;
			var applyCountKey = string.Format("ApplyCount:{0}_{1}", DateTime.Now.ToString("yyyyMMdd"), iSourceType);
			var applyCountValue = RedisHelper.GetCache(applyCountKey, strPrefix);
			if (applyCountValue != null)
			{
				lApplyCount = Convert.ToInt32(applyCountValue) + 1;
			}
			else
			{
				var userInfoBusiness = new CRD_CD_CreditUserInfoBusiness();
				lApplyCount = userInfoBusiness.GetApplyCount(iSourceType) + 1;
			}

			if (lApplyCount > iLimitCount)
			{
				return false;
			}

			RedisHelper.SetCache(applyCountKey, lApplyCount, strPrefix, 1440);

			return true;
		}
	}
}
