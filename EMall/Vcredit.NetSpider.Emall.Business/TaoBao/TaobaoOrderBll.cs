using Vcredit.NetSpider.Emall.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using ServiceStack.OrmLite;
using Vcredit.NetSpider.Emall.Entity.TaoBao;
using System.Threading.Tasks;
using Vcredit.NetSpider.Emall.Entity.TaoBao.DomainEntity;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Dto;
using System.Data.SqlClient;
using System.Data;

namespace Vcredit.NetSpider.Emall.Business.TaoBao
{

    public class TaobaoOrderBll : Business<TaobaoOrderEntity, SqlConnectionFactory>
    {
        public static readonly TaobaoOrderBll Initialize = new TaobaoOrderBll();
        TaobaoOrderBll() { }

        public TaobaoOrderEntity Load(int id)
        {
            return Single(e => e.Id == id);
        }

        public List<TaobaoOrderEntity> List()
        {
            return Select();
        }

        public List<TaobaoOrderEntity> List(SqlExpression<TaobaoOrderEntity> expression)
        {
            return Select(expression);
        }

        #region ���ݲ�����ѯ�б�
        /// <summary>
        /// ���ݲ�����ѯ�б�
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbOrderEntityDto> QueryList(QueryOrderReq param)
        {
            var userinfo = TaobaoUserInfoBll.Initialize.GetByToken(param.Token);
            if (userinfo == null)
                return null;

            SqlExpression<TaobaoOrderEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userinfo.Account);
            if (param.StartTime != null)
            {
                sqlexp.Where(x => x.OrderTime >= param.StartTime.Value);
            }
            if (param.EndTime != null)
            {
                sqlexp.Where(x => x.OrderTime <= param.EndTime.Value);
            }
            sqlexp.OrderByDescending(x => x.OrderTime);
            var orders = Select(sqlexp);

            //sqlExp.Skip((param.PageIndex - 1) * param.PageSize);
            //sqlExp.Take(param.PageSize);
            var ordersDto = orders.Select(x => MapperHelper.Map<TaobaoOrderEntity, TbOrderEntityDto>(x)).ToList();
            foreach (var o in ordersDto)
            {
                var goodsList = TaobaoGoodsBll.Initialize.Select(x => x.OrderNo == o.OrderNo);
                o.goodsList = goodsList.Select(x => MapperHelper.Map<TaobaoGoodsEntity, TbGoodsEntityDto>(x)).ToList();
            }
            return ordersDto;
        }

        /// <summary>
        /// ���ݲ�����ѯ�б�V2
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public List<TbOrderEntityDto> QueryListV2(QueryOrderReq param, ref long totalCount)
        {
            var userinfo = TaobaoUserInfoBll.Initialize.GetByToken(param.Token);
            if (userinfo == null)
                return null;

            SqlExpression<TaobaoOrderEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userinfo.Account);
            if (param.StartTime != null)
            {
                sqlexp.Where(x => x.OrderTime >= param.StartTime.Value);
            }
            if (param.EndTime != null)
            {
                sqlexp.Where(x => x.OrderTime <= param.EndTime.Value);
            }
            sqlexp.OrderByDescending(x => x.OrderTime);

            // ���ݻ�ȡ
            totalCount = this.Count(sqlexp);   // ����
            sqlexp.Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            var orders = this.Select(sqlexp); // ��ҳ����

            var ordersDto = orders.Select(x => MapperHelper.Map<TaobaoOrderEntity, TbOrderEntityDto>(x)).ToList();
            if (param.IsShowGoods)
            {
                if (ordersDto != null && ordersDto.Any())
                {
                    // ��ǰ�������е���Ʒ
                    var allGoodsData = TaobaoGoodsBll.Initialize.GetByOrderNos(ordersDto.Select(x => x.OrderNo));
                    foreach (var o in ordersDto)
                    {
                        var goodsList = allGoodsData.Where(x => x.OrderNo == o.OrderNo).ToList();
                        o.goodsList = goodsList.Select(x => MapperHelper.Map<TaobaoGoodsEntity, TbGoodsEntityDto>(x)).ToList();
                    }
                }
            }

            return ordersDto;
        }

        #endregion

        #region ��ȡ����Ķ���
        /// <summary>
        /// ����Ķ���
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public TbOrderEntityDto QueryFirstOrder(string token)
        {
            var userinfo = TaobaoUserInfoBll.Initialize.GetByToken(token);
            if (userinfo == null)
                return null;

            SqlExpression<TaobaoOrderEntity> sqlexp = SqlExpression();
            sqlexp.Where(x => x.AccountName == userinfo.Account).OrderBy(x => x.OrderTime).Take(1);

            var order = Select(sqlexp).FirstOrDefault();

            var dto = MapperHelper.Map<TaobaoOrderEntity, TbOrderEntityDto>(order);
            if (dto != null)
            {
                var goodsList = TaobaoGoodsBll.Initialize.Select(x => x.OrderNo == dto.OrderNo);
                dto.goodsList = goodsList.Select(x => MapperHelper.Map<TaobaoGoodsEntity, TbGoodsEntityDto>(x)).ToList();
            }
            return dto;
        }
        #endregion

        #region ��ȡ���µ����ݿ��������һ������
        /// <summary>
        /// ��ȡ���µ����ݿ��������һ������
        /// </summary>
        /// <returns></returns>
        public TaobaoOrderEntity QueryNewerOrder(string accountName)
        {
            if (string.IsNullOrEmpty(accountName))
                return null;

            var sqlExp = SqlExpression();
            sqlExp.Where(x => x.AccountName == accountName).OrderByDescending(x => x.OrderTime).Take(1);
            return this.Select(sqlExp).FirstOrDefault();
        }
        #endregion
    }
}
