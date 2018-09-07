using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;


namespace Vcredit.NetSpider.Service
{
    // SummaryEntity服务对象
    internal class SummaryImpl : BaseService<SummaryEntity>, ISummary
    {
        IOperationLogDao OperationLogDao = null;
        /// <summary>
        /// 根据Bid,查询Summary
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        public SummaryEntity GetByBid(int bid)
        {
            var ls = base.Find("from SummaryEntity where Bid=?", new object[] { bid });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 根据业务信息,查询操作日志
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        public SummaryEntity GetByBusiness(string BusId, string BusType)
        {
            var ls = base.Find("from SummaryEntity where BusId=? and BusType=? order by id desc", new object[] { BusId, BusType });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 修改手机实名认证
        /// </summary>
        /// <param name="Mobile">手机号</param>
        /// <param name="IsAuth">认证状态</param>
        /// <returns></returns>
        public bool UpdateRealNameAuth(string IdentityNo, string Mobile, int IsAuth)
        {
            try
            {
                var entity = base.FindListByHql(@"from SummaryEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by Oper_id desc", new object[] { IdentityNo, Mobile }, 1, 1).FirstOrDefault();
                if (entity != null && entity.IsRealNameAuth != IsAuth)
                {
                    entity.IsRealNameAuth = IsAuth;
                    base.Update(entity);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 根据OperId，查询账单统计
        /// </summary>
        /// <param name="OperId"></param>
        /// <returns></returns>
        public SummaryEntity GetByOperId(int OperId)
        {
            var ls = base.Find("from SummaryEntity where Oper_id=?", new object[] { OperId });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 根据手机号，查询账单统计
        /// </summary>
        /// <param name="Mobile"></param>
        /// <returns></returns>
        public SummaryEntity GetByMobile(string Mobile)
        {
            var ls = base.Find("from SummaryEntity where Mobile=?  order by id desc", new object[] { Mobile });

            return ls.FirstOrDefault();
        }


        public SummaryEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile, string BusType)
        {
            SummaryEntity summaryEntity = null;
            if (BusType == null)
            {
                summaryEntity = base.FindListByHql(@"from SummaryEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by Oper_id desc", new object[] { IdentityNo, Mobile}, 1, 1).FirstOrDefault();
            }
            else
            {
                summaryEntity = base.FindListByHql(@"from SummaryEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? and  OperationLog.BusType=? order by Oper_id desc", new object[] { IdentityNo, Mobile,BusType }, 1, 1).FirstOrDefault();
            }
            //var ls = base.FindListByHql(@"from SummaryEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by Oper_id desc", new object[] { IdentityNo, Mobile },1,1);
            //var ls1 = base.GetSession().CreateSQLQuery("select top 1 basicentit0_.reg_time as col_0_0_, 1 as col_1_0_ from [jxl].[basic] basicentit0_  WITH(NOLOCK), [jxl].[OperationLog]  operationl1_ WITH(NOLOCK) where basicentit0_.oper_id=operationl1_.Id and operationl1_.IdentityNo='370103199306148024' and operationl1_.Mobile='15589922029' order by basicentit0_.basic_id desc").List<SummaryEntity>();
            //var ls = base.GetSession().CreateSQLQuery(string.Format("select top 1 basicentit0_.reg_time as col_0_0_, 1 as col_1_0_ from [jxl].[basic] basicentit0_  WITH(NOLOCK), [jxl].[OperationLog]  operationl1_ WITH(NOLOCK) where basicentit0_.oper_id=operationl1_.Id and operationl1_.IdentityNo='{0}' and operationl1_.Mobile='{1}' order by basicentit0_.basic_id desc", IdentityNo, Mobile)).List<SummaryEntity>();

            return summaryEntity;
        }
        /// <summary>
        /// 获取手机已使用月份
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        public int GetUseMonths(string IdentityNo, string Mobile)
        {
            int UseMonths = 0;
            //var ls = base.FindObjsByHql(@"select Reg_time,1 from BasicEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=? order by id desc", new object[] { IdentityNo, Mobile }, 1, 1);
            var ls = base.GetSession().CreateSQLQuery(string.Format("select top 1 basicentit0_.reg_time as col_0_0_, 1 as col_1_0_ from [jxl].[basic] basicentit0_  WITH(NOLOCK), [jxl].[OperationLog]  operationl1_ WITH(NOLOCK) where basicentit0_.oper_id=operationl1_.Id and operationl1_.IdentityNo='{0}' and operationl1_.Mobile='{1}' order by basicentit0_.basic_id desc", IdentityNo, Mobile)).List<object[]>();

            if (ls != null && ls.Count > 0 && ls[0][0] != null && ls[0][0] != "")
            {
                object Reg_time = ls[0][0];
                if (Reg_time != null && !String.IsNullOrEmpty(Reg_time.ToString()))
                {
                    var regtime = DateTime.Parse(Reg_time.ToString());
                    UseMonths = Vcredit.Common.Utility.CommonFun.GetIntervalOf2DateTime(DateTime.Now, regtime, "M");
                }
            }
            else
            {
                //var Calls = base.FindObjsByHql("select Start_time,1 from CallsEntity where OperationLog.IdentityNo=? and  OperationLog.Mobile=?  order by id desc", new object[] { IdentityNo, Mobile },1,1);
                var opr = OperationLogDao.FindObjsByHql(@"select id,1 from OperationLogEntity where IdentityNo=? and  Mobile=? order by id desc", new object[] { IdentityNo, Mobile }, 1, 1).FirstOrDefault();

                if (opr != null)
                {
                    var Calls = base.GetSession().CreateSQLQuery(string.Format("select top 1 start_time, 1  from [jxl].[calls]   WITH(NOLOCK) where Oper_id={0} order by call_id desc", opr[0])).List<object[]>();

                    if (Calls != null && Calls.Count > 0)
                    {
                        object Start_time = Calls[0][0];
                        if (Start_time != null && !String.IsNullOrEmpty(Start_time.ToString()))
                        {
                            var starttime = DateTime.Parse(Start_time.ToString());
                            UseMonths = Vcredit.Common.Utility.CommonFun.GetIntervalOf2DateTime(DateTime.Now, starttime, "M");
                        }
                    }
                }
            }

            return UseMonths;
        }
    }
}

