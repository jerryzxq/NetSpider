using Newtonsoft.Json;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Mvc;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.AssureWeb.Tools;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.AssureWeb.Controllers
{
	public class ApplyLimitMaintainController : BaseController
	{
		readonly static List<BusTypeDto> BUS_TYPE_LIST = JsonConvert.DeserializeObject<List<BusTypeDto>>(System.IO.File.ReadAllText(UniversalFilePathResolver.ResolvePath("~\\Configs\\BusTypeList.json")));

		#region View视图
		public ActionResult ApplyLimitList()
		{
			return View();
		}

		public ActionResult ApplyLimitModify(int Id)
		{
			ViewData["Id"] = Id;
			return View();
		}
		#endregion

		#region 业务代码
		public string Get_BusType()
		{
			var strResult = string.Empty;

			if (BUS_TYPE_LIST.Count > 0)
			{
				var strBuilder = new StringBuilder();
				foreach (var item in BUS_TYPE_LIST)
				{
					strBuilder.Append("<option value=" + item.BusType + ">" + item.Name + "</option>");
				}
				strResult = strBuilder.ToString();
			}

			return strResult;
		}
		#endregion

		#region 数据查询

		/// <summary>
		/// 获取数据列表
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult Get_ApplyLimitList(ApplyLimitMaintainDto param)
		{
			var data = new DataResponseDto<List<CRD_CD_CreditApplyLimitEntity>>();

			BaseDao dao = new BaseDao();
			SqlExpression<CRD_CD_CreditApplyLimitEntity> sqlexp = dao.SqlExpression<CRD_CD_CreditApplyLimitEntity>();
			if (param.LimitCount > 0)
			{
				sqlexp.Where(x => x.LimitCount == param.LimitCount);
			}
			if (param.BusType != null)
			{
				sqlexp.Where(x => x.BusType == param.BusType);
			}
			if (param.SourceType > 0)
			{
				sqlexp.Where(x => x.SourceType == param.SourceType);
			}

			data.TotalCount = dao.Count(sqlexp);	// 总数

			sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
			sqlexp.Take(param.PageSize);
			data.Result = dao.Select(sqlexp);		// 分页数据

			data.PageIndex = param.PageIndex;
			data.PageSize = param.PageSize;
			data.EndTime = DateTime.Now.ToString();
			data.StatusCode = StatusCode.Success;
			data.StatusDescription = "数据查询成功";

			return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
		}

		#endregion

		#region 数据添加
		/// <summary>
		/// 申请限制次数添加
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult Add_ApplyLimit(ApplyLimitMaintainDto param)
		{
			BaseDao dao = new BaseDao();
			var data = new DataResponseDto<List<CRD_CD_CreditApplyLimitEntity>>();
			SqlExpression<CRD_CD_CreditApplyLimitEntity> sqlexp = dao.SqlExpression<CRD_CD_CreditApplyLimitEntity>();

			var strSourceType = string.Empty;
			if (param.SourceType == 10)
			{
				strSourceType = "外贸征信";
			}
			else if (param.SourceType == 11)
			{
				strSourceType = "担保征信";
			}

			if (param.LimitCount <= 0 || param.BusType.IsEmpty() || param.SourceType <= 0)
			{
				data.StatusDescription = string.Format("数据不能为空，允许查询数量:{0}，业务类型:{1}，数据来源:{2}", param.LimitCount, param.BusDesc, strSourceType);
				return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
			}
			sqlexp.Where(x => x.BusType == param.BusType && x.SourceType == param.SourceType);

			if (dao.Count(sqlexp) > 0)
			{
				data.StatusDescription = string.Format("数据重复，业务类型:{0}，数据来源:{1}", param.BusDesc, strSourceType);
				return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
			}
			sqlexp.ClearLimits();
			var insertEntity = MapperHelper.Map<ApplyLimitMaintainDto, CRD_CD_CreditApplyLimitEntity>(param);
			insertEntity.CreateTime = DateTime.Now;
			if (dao.Insert(insertEntity))
			{
				data.StatusCode = StatusCode.Success;
				data.StatusDescription = "数据新增成功";
			}
			else
			{
				data.StatusDescription = "数据新增失败";
			}

			data.EndTime = DateTime.Now.ToString();

			return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
		}

		#endregion

		#region 数据修改

		/// <summary>
		/// 依据ID获取编辑数据
		/// </summary>
		/// <param name="param"></param>
		/// <returns></returns>
		[HttpGet]
		public JsonResult Get_ApplyLimit(int Id)
		{
			var data = new DataResponseDto<CRD_CD_CreditApplyLimitEntity>();

			BaseDao dao = new BaseDao();
			SqlExpression<CRD_CD_CreditApplyLimitEntity> sqlexp = dao.SqlExpression<CRD_CD_CreditApplyLimitEntity>();
			if (Id > 0)
			{
				sqlexp.Where(x => x.Id == Id);
			}
			else
			{
				data.StatusDescription = string.Format("Id:{0},不存在", Id);
				return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
			}

			data.Result = dao.Single(sqlexp);

			data.EndTime = DateTime.Now.ToString();
			data.StatusCode = StatusCode.Success;
			data.StatusDescription = "数据查询成功";

			return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
		}

		/// <summary>
		/// 申请限制次数据修改
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		[HttpPost]
		public JsonResult Modify_ApplyLimit(ApplyLimitMaintainDto param)
		{
			BaseDao dao = new BaseDao();
			var data = new DataResponseDto<CRD_CD_CreditApplyLimitEntity>();

			var strSourceType = string.Empty;
			if (param.SourceType == 10)
			{
				strSourceType = "外贸征信";
			}
			else if (param.SourceType == 11)
			{
				strSourceType = "担保征信";
			}

			if (param.LimitCount <= 0 || param.BusType.IsEmpty() || param.SourceType <= 0)
			{
				data.StatusDescription = string.Format("数据不能为空允许查询数量:{0}，业务类型:{1}，数据来源:{2}", param.LimitCount, param.BusDesc, strSourceType);
				return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
			}

			var lsEntitys = dao.Select<CRD_CD_CreditApplyLimitEntity>(x => x.BusType == param.BusType && x.SourceType == param.SourceType);
			if (lsEntitys.Count > 0 && lsEntitys.FindAll(x => x.Id == param.Id).Count <= 0)
			{
				data.StatusDescription = string.Format("数据重复，业务类型:{0}，数据来源:{1}", param.BusDesc, strSourceType);
				return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
			}

			var entity = dao.SingleById<CRD_CD_CreditApplyLimitEntity>(param.Id);
			entity.LimitCount = param.LimitCount;
			entity.BusType = param.BusType;
			entity.BusDesc = param.BusDesc;
			entity.SourceType = param.SourceType;
			entity.IpAddr = param.IpAddr;
			entity.UpdateTime = DateTime.Now;

			if (dao.Update(entity))
			{
				data.StatusCode = StatusCode.Success;

				data.StatusDescription = "数据修改成功";
			}
			else
			{
				data.StatusDescription = "数据修改失败";
			}

			data.EndTime = DateTime.Now.ToString();

			return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
		}

		#endregion
	}

}