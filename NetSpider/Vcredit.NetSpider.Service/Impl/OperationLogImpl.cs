using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;
using System.Text.RegularExpressions;
using Vcredit.Common.Ext;
using Vcredit.Common.Constants;


namespace Vcredit.NetSpider.Service
{
    // OperationLogEntity服务对象
    internal class OperationLogImpl : BaseService<OperationLogEntity>, IOperationLog
    {
        INetsDao NetsDao = null;
        ISmsesDao SmsesDao = null;
        IBasicDao BasicDao = null;
        ITransactionsDao TransactionsDao = null;
        ICallsDao CallsDao = null;
        ISummaryDao SummaryDao = null;
        /// <summary>
        /// 更新OperationLog及其对应子表信息
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public void UpdateWithSub(OperationLogEntity Entity)
        {
            base.Update(Entity);
            if (Entity != null)
            {
                if (Entity.Basic.Reg_time.ToTrim().IsEmpty())
                {
                    var startcall = Entity.Calls.Where(o => !o.Start_time.IsEmpty()).OrderBy(o => o.Start_time).FirstOrDefault();
                    if (startcall != null)
                    {
                        Entity.Basic.Reg_time = startcall.Start_time;
                    }
                }
                Entity.Basic.Oper_id = Entity.Id;
                BasicDao.Save(Entity.Basic);
            }
            //Smses保存
            if (Entity.Smses != null)
            {
                foreach (var detail in Entity.Smses)
                {
                    if (detail.Init_type != null && detail.Init_type.Length > 100)
                    {
                        detail.Init_type = "";
                    }
                    detail.Oper_id = Entity.Id;
                    SmsesDao.Save(detail);
                }
            }
            //Nets保存
            if (Entity.Nets != null)
            {
                foreach (var detail in Entity.Nets)
                {
                    if (detail.Net_type != null && detail.Net_type.Length > 100)
                    {
                        detail.Net_type = "";
                    }
                    detail.Oper_id = Entity.Id;
                    NetsDao.Save(detail);
                }
            }
            //Transactions保存
            if (Entity.Transactions != null)
            {
                foreach (var detail in Entity.Transactions)
                {
                    detail.Oper_id = Entity.Id;
                    TransactionsDao.Save(detail);
                }
            }
            //Calls保存
            if (Entity.Calls != null)
            {
                foreach (var detail in Entity.Calls)
                {
                    if (detail.Call_type != null && detail.Call_type.Length > 100)
                    {
                        detail.Call_type = "";
                    }
                    detail.Oper_id = Entity.Id;
                    CallsDao.Save(detail);
                }
            }
            int IsRealNameAuth = 0;

            //手机账单汇总
            SummaryEntity summaryEntity = new SummaryEntity();
            //summaryEntity.Bid = Entity.Bid;
            summaryEntity.BusId = Entity.BusId;
            summaryEntity.BusType = Entity.BusType;
            summaryEntity.City = Entity.City;
            summaryEntity.Mobile = Entity.Mobile;
            summaryEntity.Oper_id = Entity.Id;
            if (Entity.Basic != null)
            {
                //根据身份证判断是否实名认证
                if (Entity.Basic.Idcard != null && Entity.Basic.Idcard.Replace("*", "").Trim() != "")
                {
                    string strReg = Entity.Basic.Idcard.Replace("*", @"\d");
                    Regex reg = new Regex(strReg, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    if (reg.IsMatch(Entity.IdentityNo))
                    {
                        IsRealNameAuth = 1;
                    }
                }
                if (IsRealNameAuth == 0)
                {
                    //根据姓名判断是否实名认证
                    if (!Entity.Basic.Real_name.IsEmpty())
                    {
                        string strReg = Entity.Basic.Real_name.Replace("*", "[\u4e00-\u9fa5]");
                        Regex reg = new Regex(strReg, RegexOptions.IgnoreCase | RegexOptions.Multiline);
                        if (reg.IsMatch(Entity.Name))
                        {
                            IsRealNameAuth = 1;
                        }
                    }
                }
            }
            summaryEntity.IsRealNameAuth = IsRealNameAuth;

            DateTime nowDate = DateTime.Now;
            nowDate = ((DateTime)(nowDate.Year + "-" + nowDate.Month + "-1").ToDateTime(Consts.DateFormatString)).AddMonths(1).AddDays(-1);
            //统计通话次数
            if (Entity.Calls != null)
            {
                summaryEntity.OneMonthCallRecordCount = Entity.Calls.Where(o => !o.Start_time.IsEmpty() && DateTime.Parse(o.Start_time) > nowDate.AddMonths(-2)).Count();
                summaryEntity.ThreeMonthCallRecordCount = Entity.Calls.Where(o => !o.Start_time.IsEmpty() && DateTime.Parse(o.Start_time) > nowDate.AddMonths(-4)).Count();
                summaryEntity.SixMonthCallRecordCount = Entity.Calls.Where(o => !o.Start_time.IsEmpty() && DateTime.Parse(o.Start_time) > nowDate.AddMonths(-7)).Count();
            }
            //统计金额
            if (Entity.Transactions != null)
            {
                summaryEntity.OneMonthCallRecordAmount = Entity.Transactions.Where(o => !o.Bill_cycle.IsEmpty() && DateTime.Parse(o.Bill_cycle) > nowDate.AddMonths(-2) && o.Total_amt != null).Select(o => (decimal)o.Total_amt).Sum();
                summaryEntity.ThreeMonthCallRecordAmount = Entity.Transactions.Where(o => !o.Bill_cycle.IsEmpty() && DateTime.Parse(o.Bill_cycle) > nowDate.AddMonths(-4) && o.Total_amt != null).Select(o => (decimal)o.Total_amt).Sum();
                summaryEntity.SixMonthCallRecordAmount = Entity.Transactions.Where(o => !o.Bill_cycle.IsEmpty() && DateTime.Parse(o.Bill_cycle) > nowDate.AddMonths(-7) && o.Total_amt != null).Select(o => (decimal)o.Total_amt).Sum();
            }

            SummaryDao.Save(summaryEntity);
        }

        public override OperationLogEntity Get(object id)
        {
            var ls = base.Find("from OperationLogEntity where Id=? order by SendTime desc", new object[] { id });
            return ls.FirstOrDefault();
        }

        /// <summary>
        /// 根据Bid,查询操作日志
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        public OperationLogEntity GetByBid(int bid)
        {
            var ls = base.Find("from OperationLogEntity where Bid=? order by SendTime desc", new object[] { bid });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 获取前200条等待接收的数据
        /// </summary>
        /// <returns></returns>
        public IList<OperationLogEntity> GetListFromWaitReceive()
        {
            IList<OperationLogEntity> entityList = new List<OperationLogEntity>();
            try
            {
                object[] objs = new object[0];
                entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) ORDER BY SendTime", objs, 1, 200);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
            }
            catch (Exception)
            { }
            return entityList;
        }


        /// <summary>
        /// 获取前200条等待接收的数据
        /// </summary>
        /// <returns></returns>
        public IList<OperationLogEntity> GetListFromWaitReceiveBySource(string source)
        {
            IList<OperationLogEntity> entityList = new List<OperationLogEntity>();
            try
            {
                object[] objs = new object[0];
                entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND Source ='" + source + "' AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) ORDER BY SendTime", objs, 1, 200);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
            }
            catch (Exception)
            { }
            return entityList;
        }


        /// <summary>
        ///获取路径为空的推送报告
        /// </summary>
        /// <returns></returns>
        public IList<OperationLogEntity> GetListFilePathIsNull()
        {
            IList<OperationLogEntity> entityList = new List<OperationLogEntity>();
            try
            {
                object[] objs = new object[0];
                entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 2 AND (ReceiveFilePath IS NULL) AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) ORDER BY SendTime", objs, 1, 1000);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
                //entityList = base.FindListByHql("from OperationLogEntity WHERE   Status = 1 AND (ReceiveFailCount IS NULL OR ReceiveFailCount < 10) AND DATEDIFF(n, SendTime, GETDATE()) > 10 ORDER BY SendTime", objs, 1, 200);
            }
            catch (Exception)
            { }
            return entityList;
        }

        public int UpdateReceiveByBid(int Bid, string ReceiveFilePath)
        {
            int ExecResult = 0;
            try
            {
                string hql = string.Format("update OperationLogEntity set SET ReceiveTime ='{0}' ,Status = 2 , ReceiveFilePath ='{1}' WHERE   Bid = {2}", DateTime.Now, ReceiveFilePath, Bid);
                ExecResult = base.ExecuteUpdateHQL(hql);
            }
            catch (Exception e)
            {

            }
            return ExecResult;
        }

        /// <summary>
        /// 根据业务信息,查询操作日志
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <returns></returns>
        public OperationLogEntity GetByBusiness(string BusId, string BusType)
        {
            var ls = base.Find("from OperationLogEntity where BusId=? and BusType=? order by SendTime desc", new object[] { BusId, BusType });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 根据身份证号,查询操作日志
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <returns></returns>
        public OperationLogEntity GetByIdentityNoAndMobile(string IdentityNo, string Mobile)
        {
            var ls = base.Find("from OperationLogEntity where IdentityNo=? and Mobile=? order by SendTime desc", new object[] { IdentityNo, Mobile });

            return ls.FirstOrDefault();
        }
        /// <summary>
        /// 根据身份证号,查询操作日志
        /// </summary>
        /// <param name="IdentityNo">身份证号</param>
        /// <param name="Name">姓名</param>
        /// <param name="Mobile">手机号</param>
        /// <returns></returns>
        public OperationLogEntity GetByIdentityNoAndNameAndMobile(string IdentityNo, string Name, string Mobile)
        {
            var ls = base.Find("from OperationLogEntity where IdentityNo=? and Name=? and Mobile=? and  Status=2 order by SendTime desc", new object[] { IdentityNo, Name, Mobile });

            return ls.FirstOrDefault();
        }
        public IList<OperationLogEntity> GetListTodayFailBySource(string source)
        {
            IList<OperationLogEntity> entityList = new List<OperationLogEntity>();
            try
            {
                object[] objs = new object[0];
                entityList = base.Find("from OperationLogEntity WHERE   Status = 1 AND [Source] ='" + source + "' AND ReceiveFailCount=10 AND SendTime>='" + DateTime.Now.ToShortDateString() + "' ORDER BY SendTime");
            }
            catch (Exception)
            { }
            return entityList;
        }

        public IList<OperationLogEntity> GetListTodayFail()
        {
            IList<OperationLogEntity> entityList = new List<OperationLogEntity>();
            try
            {
                object[] objs = new object[0];
                entityList = base.Find("from OperationLogEntity WHERE   Status = 1 AND ReceiveFailCount=10 AND SendTime>='" + DateTime.Now.ToShortDateString() + "' ORDER BY SendTime");
            }
            catch (Exception)
            { }
            return entityList;
        }
    }
}

