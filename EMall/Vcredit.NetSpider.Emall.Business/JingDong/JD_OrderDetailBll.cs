using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vcredit.NetSpider.Emall.Data;
using Vcredit.NetSpider.Emall.Entity;

using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Dto;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Dto.JingDong;

namespace Vcredit.NetSpider.Emall.Business.JingDong
{
    public class OrderDetailComparers : IEqualityComparer<OrderEntity>
    {

        public bool Equals(OrderEntity x, OrderEntity y)
        {
            return x.OrderNo == y.OrderNo;
        }

        public int GetHashCode(OrderEntity obj)
        {
            return obj.ToString().GetHashCode();
        }
    }
    public class JD_OrderDetailBll : Business<OrderEntity, SqlConnectionFactory>
    {
        readonly JD_GoodsBll goodbll = new JD_GoodsBll();
        public List<OrderEntity> GetOrderListByUserID(int userid)
        {
            var orderList = Select(x => x.UserId == userid);
            var goodsList = goodbll.Select(x => x.UserId == userid);
            AssignGoodsListForOrderList(orderList, goodsList);
            return orderList;
        }

        private static void AssignGoodsListForOrderList(List<OrderEntity> orderList, List<GoodsEntity> goodsList)
        {
            foreach (var item in orderList)
            {
                var list = goodsList.Where(x => x.OrderNo == item.OrderNo);
                if (list != null && list.Count() != 0)
                    item.GoodsList = list.ToList();
            }
        }

        public List<OrderEntity> GetOrderListByUserName(string username)
        {
            var orderList = Select(x => x.AccountName == username);
            var goodsList = goodbll.Select(x => x.AccountName == username);
            AssignGoodsListForOrderList(orderList, goodsList);
            return orderList;
        }

        public List<JdOrderEntityDto> QueryOrderList(QueryOrderReq param, ref long totalCount)
        {
            var userinfo = new JD_UserInfoBll().Select(x => x.Token == param.Token).FirstOrDefault();
            if (userinfo == null)
                return null;

            SqlExpression<OrderEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userinfo.AccountName);

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

            var ordersDto = orders.Select(x => MapperHelper.Map<OrderEntity, JdOrderEntityDto>(x)).ToList();
            if (param.IsShowGoods)
            {
                if (ordersDto != null && ordersDto.Any())
                {
                    // 当前订单所有的商品
                    var allGoodsData = new JD_GoodsBll().GetByOrderNos(orders.Select(x => x.OrderNo));
                    foreach (var o in ordersDto)
                    {
                        var goodsList = allGoodsData.Where(x => x.OrderNo == o.OrderNo).ToList();
                        o.GoodsList = goodsList.Select(x => MapperHelper.Map<GoodsEntity, JdGoodsEntityDto>(x)).ToList();
                    }
                }
            }
            return ordersDto;
        }

        public DateTime? GetLastOrderTime(string userName)
        {
            var list = Select(" select max(orderTime) OrderTime  from JD_Order where AccountName='" + userName + "'");
            if (list.Count != 0)
                return list[0].OrderTime;
            return null;
        }
        /// <summary>
        /// 过滤掉数据库已经存在的数据
        /// </summary>
        /// <param name="orderList"></param>
        /// <returns></returns>
        public List<OrderEntity> FilterExistOrders(List<OrderEntity> orderList, string username)
        {
            if (orderList.Count == 0)
            {
                return orderList;
            }
            StringBuilder sqlsb = new StringBuilder(@" select OrderNo from JD_Order od");
            if (orderList.Count == 1)
            {
                sqlsb.Append(" where AccountName='" + username + "' and OrderNo='" + orderList[0].OrderNo + "'");
            }
            else if (orderList.Count < 5)
            {
                sqlsb.Append(" where AccountName='" + username + "' and OrderNo in('" + string.Join("','", orderList.Select(x => x.OrderNo)) + "') ");
            }
            else
            {
                sqlsb.Append(@"  inner join (");
                foreach (var item in orderList)
                {
                    sqlsb.AppendFormat("select '{0}' OrderNo1 union all ", item.OrderNo);
                }
                sqlsb = sqlsb.Remove(sqlsb.Length - 10, 9);
                sqlsb.Append(") temp on temp.OrderNo1=od.OrderNo  where AccountName='" + username + "'");
            }
            var haveList = this.Select(sqlsb.ToString());
            if (haveList.Count == 0)
                return orderList;
            else if (haveList.Count == orderList.Count)
                return new List<OrderEntity>();
            return orderList.Except(haveList, new OrderDetailComparers()).ToList();
        }

        /// <summary>
        /// 获取首个订单
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public OrderEntity FirstOrder(string account)
        {
            var order = Select(x => x.AccountName == account).OrderBy(x => x.OrderTime).Take(1).FirstOrDefault();
            return order;
        }
    }
}
