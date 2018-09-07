using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Linq;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Dto.DangDang;
using Vcredit.NetSpider.Emall.Entity.DangDang;
using Vcredit.NetSpider.Emall.Framework;

namespace Vcredit.NetSpider.Emall.Business.DangDang
{
	public class DdOrderBll : Business<DdOrderEntity, SqlConnectionFactory>
	{
		public static readonly DdOrderBll Initialize = new DdOrderBll();
		DdOrderBll() { }

		public DdOrderEntity Load(int id)
		{
			return Single(e => e.Id == id);
		}

		public List<DdOrderEntity> List()
		{
			return Select();
		}

		public List<DdOrderEntity> List(SqlExpression<DdOrderEntity> expression)
		{
			return Select(expression);
		}

		/// <summary>
		/// 获取当前最新入库的订单
		/// </summary>
		public DdOrderEntity QueryNewerOrder(string account)
		{
			if (string.IsNullOrEmpty(account))
				return null;

			var entity = Select(x => x.AccountName == account).OrderByDescending(x => x.OrderTime).Take(1).FirstOrDefault();
			return entity;
		}

		/// <summary>
		/// 根据接口参数查询订单列表
		/// </summary>
		public List<DdOrderEntityDto> QueryList(QueryOrderReq param, ref long totalCount)
		{
			var userinfo = DdUserinfoBll.Initialize.GetByToken(param.Token);
			if (userinfo == null)
				return null;

			SqlExpression<DdOrderEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.AccountName == userinfo.AccountId);
			if (param.StartTime != null)
			{
				sqlexp.Where(x => x.OrderTime >= param.StartTime.Value);
			}
			if (param.EndTime != null)
			{
				sqlexp.Where(x => x.OrderTime <= param.EndTime.Value);
			}
			sqlexp.OrderByDescending(x => x.OrderTime);

			// 数据获取
			totalCount = this.Count(sqlexp);	// 总数
			sqlexp.Skip((param.PageIndex - 1) * param.PageSize);
			sqlexp.Take(param.PageSize);
			var orders = this.Select(sqlexp);	// 分页数据

			var ordersDto = orders.Select(x => MapperHelper.Map<DdOrderEntity, DdOrderEntityDto>(x)).ToList();
			if (param.IsShowGoods)
			{
				if (ordersDto != null && ordersDto.Any())
				{
					// 当前订单所有的商品
					var allGoodsData = DdGoodsBll.Initialize.GetByOrderNos(ordersDto.Select(x => x.OrderNo));
					foreach (var o in ordersDto)
					{
						var goodsList = allGoodsData.Where(x => x.OrderNo == o.OrderNo).ToList();
						o.goodsList = goodsList.Select(x => MapperHelper.Map<DdGoodsEntity, DdGoodsEntityDto>(x)).ToList();
					}
				}
			}

			return ordersDto;
		}

		#region 获取最早的订单
		/// <summary>
		/// 最早的订单
		/// </summary>
		public DdOrderEntityDto QueryFirstOrder(string token)
		{
			var userinfo = DdUserinfoBll.Initialize.GetByToken(token);
			if (userinfo == null)
				return null;

			SqlExpression<DdOrderEntity> sqlexp = SqlExpression();
			sqlexp.Where(x => x.AccountName == userinfo.AccountId).OrderBy(x => x.OrderTime).Take(1);

			var order = Select(sqlexp).FirstOrDefault();

			var dto = MapperHelper.Map<DdOrderEntity, DdOrderEntityDto>(order);
			if (dto != null)
			{
				var goodsList = DdGoodsBll.Initialize.Select(x => x.OrderNo == dto.OrderNo);
				dto.goodsList = goodsList.Select(x => MapperHelper.Map<DdGoodsEntity, DdGoodsEntityDto>(x)).ToList();
			}
			return dto;
		}
		#endregion
	}
}
