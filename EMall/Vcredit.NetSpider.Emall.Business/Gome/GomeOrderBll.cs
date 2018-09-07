using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.Gome;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Business.Gome;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Dto.Gome;

namespace Vcredit.NetSpider.Emall.Business.Gome
{
	
	public class GomeOrderBll : Business<GomeOrderEntity, SqlConnectionFactory>
	{
        public static readonly GomeOrderBll Initialize = new GomeOrderBll();
        GomeOrderBll() { }

	    public GomeOrderEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<GomeOrderEntity> List()
        {
            return Select();
        }

        public List<GomeOrderEntity> List(SqlExpression<GomeOrderEntity> expression)
        {
            return Select(expression);
        }

        public GomeOrderEntity QueryNewerOrder(string AccountId)
        {
            if (string.IsNullOrEmpty(AccountId))
                return null;

            var entity = Select(x => x.AccountName == AccountId).OrderByDescending(x => x.OrderTime).Take(1).FirstOrDefault();
            return entity;
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="param"></param>
        /// <param name="totalCount"></param>
        /// <returns></returns>
        public List<GomeOrderEntityDto> QueryList(QueryOrderReq param, ref long totalCount)
        {
            var userinfo = GomeUserinfoBll.Initialize.GetByToken(param.Token);
            if (userinfo == null)
                return null;

            SqlExpression<GomeOrderEntity> sqlexp = SqlExpression();
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
            totalCount = this.Count(sqlexp);   // 总数
            sqlexp.Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            var orders = this.Select(sqlexp); // 分页数据

            var ordersDto = orders.Select(x => MapperHelper.Map<GomeOrderEntity, GomeOrderEntityDto>(x)).ToList();
            if (param.IsShowGoods)
            {
                // 当前订单所有的商品
                var allGoodsData = GomeGoodsBll.Initialize.GetByOrderNos(ordersDto.Select(x => x.OrderNo));
                foreach (var o in ordersDto)
                {
                    var goodsList = allGoodsData.Where(x => x.OrderNo == o.OrderNo).ToList();
                    o.goodsList = goodsList.Select(x => MapperHelper.Map<GomeGoodsEntity, GomeGoodsEntityDto>(x)).ToList();
                }
            }
            return ordersDto;
        }

        /// <summary>
        /// 首个订单
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public GomeOrderEntityDto QueryFirstOrder(string token)
        {
            var userinfo = GomeUserinfoBll.Initialize.GetByToken(token);
            if (userinfo == null)
                return null;

            SqlExpression<GomeOrderEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userinfo.AccountId).OrderBy(x => x.OrderTime).Take(1);

            var order = Select(sqlexp).FirstOrDefault();

            var dto = MapperHelper.Map<GomeOrderEntity, GomeOrderEntityDto>(order);
            if (dto != null)
            {
                var goodsList = GomeGoodsBll.Initialize.Select(x => x.OrderNo == dto.OrderNo);
                dto.goodsList = goodsList.Select(x => MapperHelper.Map<GomeGoodsEntity, GomeGoodsEntityDto>(x)).ToList();
            }
            return dto;
        }
    }
}
