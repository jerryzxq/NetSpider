using System;
using System.Collections.Generic;
using System.Linq;
using Vcredit.Framework.Server.Service;
using Vcredit.Framework.Common.Utility;
using Vcredit.NetSpider.Entity.DB;
using Vcredit.NetSpider.Dao;
using Vcredit.Common.Utility;


namespace Vcredit.NetSpider.Service
{
    // CRD_HD_REPORTEntity服务对象
    internal class CRD_HD_REPORTImpl : BaseService<CRD_HD_REPORTEntity>, ICRD_HD_REPORT
    {
        ICRD_CD_GUARANTEEDao GUARANTEEDao = null;
        ICRD_CD_LNDao LNDao = null;
        ICRD_CD_LNDDao LNDDao = null;
        ICRD_CD_STNCARDDao STNCARDDao = null;
        ICRD_HD_REPORT_HTMLDao htmlDao = null;
        ICRD_QR_RECORDDTLDao RECORDDTLDao = null;
        ICRD_IS_CREDITCUEDao CREDITCUEDao = null;
        ICRD_STAT_LNDao STATLNDao = null;
        ICRD_STAT_LNDDao STATLNDDao = null;
        ICRD_STAT_QRDao STATQRDao = null;
        ICRD_CD_ASRREPAYDao ASRREPAYDao = null;
        ICRD_PI_FORCEEXCTNDao FORCEEXCTNDao = null;
        ICRD_PI_TAXARREARDao TAXARREARDao = null;
        ICRD_PI_TELPNTDao TELPNTDao = null;
        ICRD_PI_CIVILJDGMDao CIVILJDGMDao = null;
        ICRD_PI_ADMINPNSHMDao ADMINPNSHMDao = null;

        /// <summary>
        /// 重写Save方法
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override object Save(CRD_HD_REPORTEntity entity)
        {
            base.Save(entity);

            foreach (var detail in entity.CRD_CD_GUARANTEEList)
            {
                detail.ReportId = entity.Id;
                GUARANTEEDao.Save(detail);
            }

            foreach (var detail in entity.CRD_CD_LNDList)
            {
                detail.ReportId = entity.Id;
                LNDDao.Save(detail);
            }

            foreach (var detail in entity.CRD_CD_LNList)
            {
                detail.ReportId = entity.Id;
                LNDao.Save(detail);
            }

            foreach (var detail in entity.CRD_CD_STNCARDList)
            {
                detail.ReportId = entity.Id;
                STNCARDDao.Save(detail);
            }

            foreach (var detail in entity.CRD_QR_RECORDDTLList)
            {
                detail.ReportId = entity.Id;
                RECORDDTLDao.Save(detail);
            }
            if (entity.CRD_IS_CREDITCUE != null)
            {
                entity.CRD_IS_CREDITCUE.Id = entity.Id;
                CREDITCUEDao.Save(entity.CRD_IS_CREDITCUE);
            }
            if (entity.CRD_HD_REPORT_HTML != null)
            {
                entity.CRD_HD_REPORT_HTML.Id = entity.Id;
                htmlDao.SaveOrUpdate(entity.CRD_HD_REPORT_HTML);
            }

            if (entity.CRD_STAT_LN != null)
            {
                entity.CRD_STAT_LN.ReportId = entity.Id;
                STATLNDao.Save(entity.CRD_STAT_LN);//贷款统计表
            }
            if (entity.CRD_STAT_LND != null)
            {
                entity.CRD_STAT_LND.ReportId = entity.Id;
                STATLNDDao.Save(entity.CRD_STAT_LND);//信用卡统计表
            }
            if (entity.CRD_STAT_QR != null)
            {
                entity.CRD_STAT_QR.ReportId = entity.Id;
                STATQRDao.Save(entity.CRD_STAT_QR);//征信查询统计表
            }

            foreach (var detail in entity.CRD_CD_ASRREPAYList)
            {
                detail.ReportId = entity.Id;
                ASRREPAYDao.Save(detail);
            }
            foreach (var detail in entity.CRD_PI_FORCEEXCTNEList)
            {
                detail.ReportId = entity.Id;
                FORCEEXCTNDao.Save(detail);
            }
            foreach (var detail in entity.CRD_PI_TAXARREARList)
            {
                detail.ReportId = entity.Id;
                TAXARREARDao.Save(detail);
            }
            foreach (var detail in entity.CRD_PI_TELPNTEntityList)
            {
                detail.ReportId = entity.Id;
                TELPNTDao.Save(detail);
            }

            foreach (var detail in entity.CRD_PI_CIVILJDGMEntityList)
            {
                detail.ReportId = entity.Id;
                CIVILJDGMDao.Save(detail);
            }
            foreach (var detail in entity.CRD_PI_ADMINPNSHMEntityList)
            {
                detail.ReportId = entity.Id;
                ADMINPNSHMDao.Save(detail);
            }
            return entity.Id;
        }

        /// <summary>
        /// 根据Bid,查询征信报告数据
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        public CRD_HD_REPORTEntity GetByBid(int bid, bool isAll = false)
        {
            CRD_HD_REPORTEntity entity = base.Find("from CRD_HD_REPORTEntity where Bid=? order by ReportCreateTime desc", new object[] { bid }).FirstOrDefault();

            if (entity != null && isAll)
            {
                CRD_STAT_LNEntity ln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                CRD_STAT_LNDEntity lnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                CRD_STAT_QREntity qr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();

                entity.CRD_STAT_LN = ln;
                entity.CRD_STAT_LND = lnd;
                entity.CRD_STAT_QR = qr;
            }
            return entity;
        }
        /// <summary>
        /// 根据Bid,查询征信报告数据, 同时查询关联子表
        /// </summary>
        /// <param name="bid">VBS系统业务号</param>
        /// <returns></returns>
        public CRD_HD_REPORTEntity GetWithSubByBid(int bid)
        {
            CRD_HD_REPORTEntity entity = base.Find("from CRD_HD_REPORTEntity where Bid=? order by ReportCreateTime desc", new object[] { bid }).FirstOrDefault();

            if (entity != null)
            {
                var lnds = LNDDao.Find("from CRD_CD_LNDEntity where ReportId=?", new object[] { entity.Id });
                var lns = LNDao.Find("from CRD_CD_LNEntity where ReportId=?", new object[] { entity.Id });


                CRD_STAT_LNEntity ln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                CRD_STAT_LNDEntity lnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                CRD_STAT_QREntity qr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();

                entity.CRD_CD_LNDList = lnds;
                entity.CRD_CD_LNList = lns;
                entity.CRD_STAT_LN = ln;
                entity.CRD_STAT_LND = lnd;
                entity.CRD_STAT_QR = qr;
            }
            return entity;
        }
        /// <summary>
        /// 根据报告编号,获取对象
        /// </summary>
        /// <param name="ReportSn">报告编号</param>
        /// <returns></returns>
        public CRD_HD_REPORTEntity GetByReportSn(string ReportSn, bool isAall = false)
        {
            CRD_HD_REPORTEntity entity = base.Find("from CRD_HD_REPORTEntity where ReportSn=? order by ReportCreateTime desc", new object[] { ReportSn }).FirstOrDefault();
            if (entity != null && isAall)
            {
                entity = GetReportStat(entity);
            }

            return entity;
        }

        public bool ReportIsExist(string Loginname, string ReportCreateTime)
        {
            try
            {
                var entitys = base.Find("from CRD_HD_REPORTEntity where Loginname=? and ReportCreateTime=? order by ReportCreateTime desc", new object[] { Loginname, ReportCreateTime });
                if (entitys.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 根据业务信息,查询征信报告数据
        /// </summary>
        /// <param name="BusId">业务号</param>
        /// <param name="BusType">业务类别</param>
        /// <param name="isAll">是否包含三张统计表信息</param>
        /// <returns></returns>
        public CRD_HD_REPORTEntity GetByBusiness(string BusId, string BusType, bool isAll)
        {
            CRD_HD_REPORTEntity entity = base.FindListByHql("from CRD_HD_REPORTEntity where BusId=? and BusType=? order by ReportCreateTime desc", new object[] { BusId, BusType }, 1, 1).FirstOrDefault();

            //if (entity != null && isAll)
            //{
            //    CRD_STAT_LNEntity ln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
            //    CRD_STAT_LNDEntity lnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
            //    CRD_STAT_QREntity qr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();

            //    entity.CRD_STAT_LN = ln;
            //    entity.CRD_STAT_LND = lnd;
            //    entity.CRD_STAT_QR = qr;
            //}
            return entity;
        }

        public CRD_HD_REPORTEntity GetByIdentityCard(string IdentityCard, bool isAll)
        {
            CRD_HD_REPORTEntity entity = base.FindListByHql("from CRD_HD_REPORTEntity where BusIdentityCard=? order by ReportCreateTime desc", new object[] { IdentityCard }, 1, 1).FirstOrDefault();

            if (entity != null && isAll)
            {
                entity = GetReportStat(entity);
                //CRD_STAT_LNEntity ln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                //CRD_STAT_LNDEntity lnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                //CRD_STAT_QREntity qr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();

                //entity.CRD_STAT_LN = ln;
                //entity.CRD_STAT_LND = lnd;
                //entity.CRD_STAT_QR = qr;
            }
            return entity;
        }

        public CRD_HD_REPORTEntity GetByIdentityCardAndBusType(string IdentityCard, string BusType, bool isAll)
        {

            string hql = "from CRD_HD_REPORTEntity where BusIdentityCard=? ";
            if (!string.IsNullOrEmpty(BusType))
            {
                hql += "and BusType='" + BusType + "' ";
            }
            hql += "order by ReportCreateTime desc";

            CRD_HD_REPORTEntity entity = base.FindListByHql(hql, new object[] { IdentityCard }, 1, 1).FirstOrDefault();
            if (entity != null && isAll)
            {
                entity = GetReportStat(entity);
            }
            return entity;
        }
        public IList<object[]> GetVbsSycnData(int reportid)
        {
            var query = base.FindObjsByHql("select Id,Name,BusIdentityCard,ReportSn,ReportCreateTime,CreateTime from CRD_HD_REPORTEntity where Id>? and BusIdentityCard is not null", new object[] { reportid });

            return query;
        }

        public CRD_HD_REPORTEntity GetByReportId(string reportid)
        {
            CRD_HD_REPORTEntity entity = base.Find("from CRD_HD_REPORTEntity where Report_Id=? order by ReportCreateTime desc", new object[] { reportid }).FirstOrDefault();
            if (entity != null)
            {
                entity = GetReportStat(entity);
                //CRD_STAT_LNEntity ln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                //CRD_STAT_LNDEntity lnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                //CRD_STAT_QREntity qr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();

                //entity.CRD_STAT_LN = ln;
                //entity.CRD_STAT_LND = lnd;
                //entity.CRD_STAT_QR = qr;
            }
            return entity;
        }

        public CRD_HD_REPORTEntity GetDataAllByIdentityCardAndBusType(string IdentityCard, string reportsn, string BusType, bool isAll)
        {

            string hql = "from CRD_HD_REPORTEntity where  ";
            var parsm = new List<object>();
            if (!string.IsNullOrEmpty(IdentityCard))
            {
                hql += " BusIdentityCard=? ";
                parsm.Add(IdentityCard);

            }
            if (!string.IsNullOrEmpty(reportsn))
            {
                hql += !string.IsNullOrEmpty(IdentityCard) ? " and ReportSn=?" : "ReportSn=? ";
                parsm.Add(reportsn);

            }
            if (!string.IsNullOrEmpty(BusType))
            {
                hql += "and BusType='" + BusType + "' ";
            }
            hql += "order by ReportCreateTime desc";

            CRD_HD_REPORTEntity entity = base.FindListByHql(hql, parsm.ToArray(), 1, 1).FirstOrDefault();
            if (entity != null && isAll)
            {
                entity = GetDataFromRepots(entity);
            }
            return entity;
        }
        private CRD_HD_REPORTEntity GetDataFromRepots(CRD_HD_REPORTEntity entity)
        {
            try
            {
                var lndList = LNDDao.Find("from CRD_CD_LNDEntity where ReportId=?", new object[] { entity.Id });//贷记卡信息
                var stncardList = STNCARDDao.Find("from CRD_CD_STNCARDEntity where ReportId=?", new object[] { entity.Id });//准贷记卡信息
                var recordList = RECORDDTLDao.Find("from CRD_QR_RECORDDTLEntity where ReportId=?", new object[] { entity.Id });//信贷审批查询记录明细
                var lnList = LNDao.Find("from CRD_CD_LNEntity where ReportId=?", new object[] { entity.Id });///贷款信息
                var guarnlist = GUARANTEEDao.Find("from CRD_CD_GUARANTEEEntity where ReportId=?", new object[] { entity.Id });//担保信息

                entity.CRD_CD_LNDList = lndList;
                entity.CRD_CD_LNList = lnList;
                entity.CRD_CD_GUARANTEEList = guarnlist;
                entity.CRD_CD_STNCARDList = stncardList;
                entity.CRD_QR_RECORDDTLList = recordList;

            }
            catch (Exception ex)
            {

            }

            return entity;
        }

        /// <summary>
        /// 计算征信变量信息
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private CRD_HD_REPORTEntity GetReportStat(CRD_HD_REPORTEntity entity)
        {
            try
            {
                #region 贷款统计信息
                int ALL_LOAN_DELAY_MONTH = 0;//贷款5年逾期的月数
                decimal ln_housing_fund_amount = 0;//个人住房公积金贷款额
                decimal ln_shopfront_amount = 0;//个人住房商铺贷款额
                decimal ln_housing_mortgage_amount = 0;//个人住房按揭贷款额
                int ln_normal_count = 0;//状态为正常的贷款笔数
                var statln = STATLNDao.Find("from CRD_STAT_LNEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                var lnList = LNDao.Find("from CRD_CD_LNEntity where ReportId=?", new object[] { entity.Id });

                if (statln != null)
                {
                    ALL_LOAN_DELAY_MONTH = lnList.Where(o => o.OverdueCyc > 0).Select(o => (int)o.OverdueCyc).Sum();
                    ln_housing_fund_amount = lnList.Where(o => o.TypeDw == "个人住房公积金贷款" && o.State == "正常").Select(o => (decimal)o.CreditLimitAmount).Sum();
                    ln_shopfront_amount = lnList.Where(o => o.TypeDw == "个人住房商铺贷款" && o.State == "正常").Select(o => (decimal)o.CreditLimitAmount).Sum();
                    ln_housing_mortgage_amount = lnList.Where(o => o.TypeDw == "住房贷款" && o.State == "正常").Select(o => (decimal)o.CreditLimitAmount).Sum();
                    ln_normal_count = lnList.Where(o => o.State == "正常").Count();

                    statln.ALL_LOAN_DELAY_MONTH = ALL_LOAN_DELAY_MONTH;
                    statln.ln_housing_fund_amount = ln_housing_fund_amount;
                    statln.ln_shopfront_amount = ln_shopfront_amount;
                    statln.ln_housing_mortgage_amount = ln_housing_mortgage_amount;
                    statln.ln_normal_count = ln_normal_count;
                }
                else
                {
                    statln = new CRD_STAT_LNEntity();
                }
                #endregion

                #region 贷记卡统计信息

                int ALL_CREDIT_DELAY_MONTH = 0;//贷记卡5年逾期的月数
                int loand_Badrecord = 0;//贷记卡呆账数
                int stncard_Badrecord = 0;//准贷记卡呆账数
                decimal StnCard_UseCreditLimit = 0;//准贷记卡透支余额
                decimal lnd_max_overdue_percent = 0;//贷记卡最大逾期次数占比
                int lnd_max_normal_Age = 0;//正常状态最大授信额度卡的卡龄
                var statlnd = STATLNDDao.Find("from CRD_STAT_LNDEntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                var lndList = LNDDao.Find("from CRD_CD_LNDEntity where ReportId=?", new object[] { entity.Id });
                var stncardList = STNCARDDao.Find("from CRD_CD_STNCARDEntity where ReportId=?", new object[] { entity.Id });
                var recordList = RECORDDTLDao.Find("from CRD_QR_RECORDDTLEntity where ReportId=?", new object[] { entity.Id });

                if (statlnd != null)
                {
                    ALL_CREDIT_DELAY_MONTH = (int)lndList.Where(o => o.OverdueCyc > 0).Select(o => o.OverdueCyc).Sum();
                    loand_Badrecord = (int)lndList.Where(o => o.State == "呆账").Count();
                    stncard_Badrecord = (int)stncardList.Where(o => o.State == "呆账").Count();
                    StnCard_UseCreditLimit = (int)stncardList.Where(o => o.UsedCreditLimitAmount > 0).Select(o => o.UsedCreditLimitAmount).Sum();

                    int usemonth = 0;
                    DateTime nowdate = DateTime.Now;
                    decimal tempoverdue = 0;
                    foreach (var item in lndList)
                    {
                        if (item.OverdueCyc > 0 && item.OpenDate != null)
                        {
                            usemonth = CommonFun.GetIntervalOf2DateTime(nowdate, (DateTime)item.OpenDate, "M");
                            if (usemonth > 60)
                            {
                                usemonth = 60;
                            }
                            if (usemonth == 0)
                            {
                                continue;
                            }
                            tempoverdue = (decimal)item.OverdueCyc / usemonth;
                            if (lnd_max_overdue_percent < tempoverdue)
                            {
                                lnd_max_overdue_percent = tempoverdue;
                            }
                        }
                    }
                    //正常状态最大授信额度卡的卡龄
                    if (statlnd.CREDITLIMITAMOUNTNORMMAX > 0)
                    {
                        var templnd = lndList.Where(o => o.CreditLimitAmount == statlnd.CREDITLIMITAMOUNTNORMMAX).FirstOrDefault();
                        if (templnd != null)
                        {
                            lnd_max_normal_Age = CommonFun.GetIntervalOf2DateTime((DateTime)entity.ReportCreateTime, (DateTime)templnd.OpenDate, "M");
                        }
                    }

                    statlnd.ALL_CREDIT_DELAY_MONTH = ALL_CREDIT_DELAY_MONTH;
                    statlnd.loand_Badrecord = loand_Badrecord;
                    statlnd.stncard_Badrecord = stncard_Badrecord;
                    statlnd.StnCard_UseCreditLimit = StnCard_UseCreditLimit;
                    statlnd.lnd_max_overdue_percent = lnd_max_overdue_percent;
                    statlnd.lnd_max_normal_Age = lnd_max_normal_Age;
                }
                else
                {
                    statlnd = new CRD_STAT_LNDEntity();
                }

                #endregion

                #region 征信空白(2016-4-12)
                bool iscreditblank = true;
                //判断逻辑
                //1）信贷记录-->贷记卡“已使用额度”+准贷记卡“透支余额”<=0
                //2）信贷记录-->状态为“正常”，最大“信用额度”的贷记卡、准贷记卡，其“信用额度”<500
                //3) 有贷款
                /*博哥制造
                if (statln.LOANAGEMONTH <= 0)//有贷款
                {
                    if (statlnd.NORMALUSEDMAX != null && statlnd.NORMALUSEDMAX <= 0 && StnCard_UseCreditLimit <= 0)
                    {
                        iscreditblank = true;
                    }
                    else
                    {
                        var temp_stncard = stncardList.Where(o => o.State == "正常").OrderByDescending(o => o.CreditLimitAmount).FirstOrDefault();

                        if (statlnd.CREDITLIMITAMOUNTNORMMAX != null && statlnd.CREDITLIMITAMOUNTNORMMAX < 500)
                        {
                            iscreditblank = true;
                        }
                    }
                }*/
                if (lnList.Count > 0)
                {
                    iscreditblank = false;
                }
                else
                {
                    var non_overdue_lnd = lndList.Where(e => !e.Cue.Contains("逾期") && e.State != "未激活");
                    if (non_overdue_lnd.Count() > 0)
                    {
                        if (non_overdue_lnd.Max(e => e.UsedCreditLimitAmount) > 0)
                        {
                            iscreditblank = false;
                        }
                        else if (non_overdue_lnd.Max(e => e.CreditLimitAmount) >= 500)
                        {
                            iscreditblank = false;
                        }
                    }
                    else
                    {
                        var non_overdraft_stn = stncardList.Where(e => !e.Cue.Contains("透支超过"));
                        if (non_overdraft_stn.Max(e => e.UsedCreditLimitAmount) > 0)
                        {
                            iscreditblank = false;
                        }
                        else if (non_overdraft_stn.Max(e => e.CreditLimitAmount) >= 500)
                        {
                            iscreditblank = false;
                        }
                    }
                }

                //增加征信空白逻辑
                //4)贷记卡有逾期
                //5)准贷记卡有透支
                if (iscreditblank)
                {
                    var overdue_lnd = lndList.Where(e => e.Cue.Contains("逾期"));
                    if (overdue_lnd.Count() > 0)
                    {
                        iscreditblank = false;
                    }
                    else
                    {
                        var overdraft_stn = stncardList.Where(e => e.Cue.Contains("透支超过"));
                        if (overdraft_stn.Count() > 0)
                        {
                            iscreditblank = false;
                        }
                    }
                }

                entity.IsCreditBlank = iscreditblank;
                #endregion


                //查询次数统计
                var statqr = STATQRDao.Find("from CRD_STAT_QREntity where ReportId=?", new object[] { entity.Id }).FirstOrDefault();
                if (statqr == null)
                {
                    statqr = new CRD_STAT_QREntity();
                }
                #region 卡卡贷网络版征信评分字段决策配置(hering)

                //银行信贷涉及的账户数
                statln.ACCT_NUM = lndList.Select(e => CommonFun.GetMidStr(e.FinanceOrg, "日", "发放的贷记卡") + e.OpenDate).Distinct().Count() + lnList.Count;
                //五年内最大逾期次数
                var DLQ_5YR_CNT_MAX = lndList.OrderByDescending(o => o.OverdueCyc).FirstOrDefault();
                statlnd.DLQ_5YR_CNT_MAX = DLQ_5YR_CNT_MAX != null ? DLQ_5YR_CNT_MAX.OverdueCyc : 0;
                //过去贷款结清数
                statln.LST_LOAN_CLS_CNT = lnList.Count(o => o.State == "结清");
                //过去贷款开户数
                statln.LST_LOAN_OPE_CNT = lnList.Count;
                //非银行机构的贷款审批查询次数
                statqr.NON_BNK_LN_QRY_CNT = recordList.Count(o => o.QueryReason == "贷款审批" && (o.Querier.IndexOf("银行") == -1 && o.Querier.IndexOf("花旗") == -1));
                //最近正常贷记卡授信额度
                var CreditLimitRencent = lndList.Where(o => o.State == "正常").OrderByDescending(t => t.OpenDate).ThenByDescending(d => d.CreditLimitAmount).ToList();
                statlnd.RCNT_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].CreditLimitAmount : decimal.Zero;
                //最近未销户贷记卡额度
                CreditLimitRencent = lndList.Where(o => o.State != "销户").OrderByDescending(t => t.OpenDate).ThenByDescending(d => d.CreditLimitAmount).ToList();
                statlnd.UNCLS_RCNT_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].CreditLimitAmount : decimal.Zero;
                //最早未销户贷记卡额度
                CreditLimitRencent = lndList.Where(o => o.State != "销户").OrderBy(f => f.OpenDate).ThenByDescending(d => d.CreditLimitAmount).ToList();
                statlnd.UNCLS_OLD_CDT_LMT = CreditLimitRencent.Count > 0 ? CreditLimitRencent[0].CreditLimitAmount : decimal.Zero;

                #endregion

                #region 新增需求(Kerry)
                if (lnList.Count == 0 && lndList.Count == 0 && stncardList.Count == 0 && recordList.Count == 0)
                {
                    statlnd.L12M_LOANACT_USED_MAX_R = -9999999;
                    statln.L24M_OPE_NORM_ACCT_PCT = -9999999;
                    statlnd.NORM_CDT_BAL_USED_PCT_AVG = -9999999;
                    statlnd.CRD_AGE_UNCLS_OLDEST = -9999999;
                    statqr.L3M_ACCT_QRY_NUM = -9999999;
                    statqr.L3M_LN_QRY_NUM = -9999999;
                }
                else
                {

                    DateTime query_time = new DateTime();
                    query_time = entity.QueryTime == null ? (DateTime)entity.ReportCreateTime : (DateTime)entity.QueryTime;

                    var L3M_ACCT_QRY_NUM = 0;
                    var L3M_LN_QRY_NUM = 0;
                    foreach (var record in recordList)
                    {
                        if (record.QueryDate != null)
                        {
                            var time = query_time.Year * 12 + query_time.Month - ((DateTime)record.QueryDate).Year * 12 - ((DateTime)record.QueryDate).Month;
                            //var time = query_time.Year * 1200 + query_time.Month * 100 + query_time.Day - ((DateTime)record.QueryDate).Year * 1200 - ((DateTime)record.QueryDate).Month * 100 - ((DateTime)record.QueryDate).Day;
                            //if (time < 400)
                            if (time < 3)
                            {
                                if (record.QueryReason == "信用卡审批")
                                    L3M_ACCT_QRY_NUM++;
                                if (record.QueryReason == "贷款审批")
                                    L3M_LN_QRY_NUM++;
                            }
                        }
                    }
                    statqr.L3M_ACCT_QRY_NUM = L3M_ACCT_QRY_NUM;//过去3个月信用卡审批查询次数
                    statqr.L3M_LN_QRY_NUM = L3M_LN_QRY_NUM;//过去3个月贷款审批查询次数
                    //entity.L3M_ACCT_QRY_NUM = statqr.M3CREDITCNT == null ? 0 : (int)statqr.M3CREDITCNT;//过去3个月信用卡审批查询次数
                    //entity.L3M_LN_QRY_NUM = statqr.M3LOANCNT == null ? 0 : (int)statqr.M3LOANCNT;//过去3个月贷款审批查询次数

                    // L12M_LOANACT_USED_MAX_R
                    #region L12M_LOANACT_USED_MAX_R
                    if (lnList.Count == 0)
                    {
                        statlnd.L12M_LOANACT_USED_MAX_R = -9999998;
                    }
                    else if (entity.QueryTime == null && entity.ReportCreateTime == null)
                    {
                        statlnd.L12M_LOANACT_USED_MAX_R = -9999997;
                    }
                    else
                    {
                        decimal L12M_CNT = 0;
                        decimal L12M_LOANACT_USED_MAX = 0;
                        decimal L12M_LMT_ALL = 0;
                        foreach (var ln in lnList)
                        {
                            if (ln.OpenDate != null)
                            {
                                if ((int.Parse(query_time.ToString("yyyyMMdd")) - int.Parse(((DateTime)ln.OpenDate).ToString("yyyyMMdd"))) <= 10000)
                                {
                                    L12M_CNT++;
                                    if (ln.Balance != null)
                                        L12M_LOANACT_USED_MAX = L12M_LOANACT_USED_MAX > (decimal)ln.Balance ? L12M_LOANACT_USED_MAX : (decimal)ln.Balance;
                                    L12M_LMT_ALL += (decimal)ln.CreditLimitAmount;
                                }
                            }
                        }
                        if (L12M_CNT == 0)
                        {
                            statlnd.L12M_LOANACT_USED_MAX_R = -9999996;
                        }
                        else if (L12M_LMT_ALL == 0)
                        {
                            statlnd.L12M_LOANACT_USED_MAX_R = -9999995;
                        }
                        else
                        {
                            var L12M_LOANACT_USED_MAX_R = L12M_LOANACT_USED_MAX / (L12M_LMT_ALL / L12M_CNT);
                            statlnd.L12M_LOANACT_USED_MAX_R = decimal.Parse(L12M_LOANACT_USED_MAX_R.ToString("#0.00"));//过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                            //entity.L12M_LOANACT_USED_MAX_R = L12M_LOANACT_USED_MAX == 0 ? 0 : decimal.Parse(L12M_LOANACT_USED_MAX_R.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//过去12个月开户的贷款账户最大单笔贷款余额过去12个月开户平均额度之比
                        }
                    }
                    #endregion
                    if (lndList.Count == 0)
                    {
                        statln.L24M_OPE_NORM_ACCT_PCT = -8888887;
                        statlnd.NORM_CDT_BAL_USED_PCT_AVG = -8888887;
                        statlnd.CRD_AGE_UNCLS_OLDEST = -8888888;
                    }
                    else
                    {
                        // CRD_AGE_UNCLS_OLDEST
                        #region CRD_AGE_UNCLS_OLDEST
                        var unclslndlist = lndList.Where(lnd => !lnd.State.Contains("销户"));
                        int CRD_AGE_UNCLS_OLDEST = 0;
                        if (entity.QueryTime != null || entity.ReportCreateTime != null)
                        {
                            foreach (var lnd in unclslndlist)
                            {
                                if (lnd.OpenDate != null)
                                {
                                    var CRD_AGE = query_time.Year * 12 + query_time.Month - ((DateTime)lnd.OpenDate).Year * 12 - ((DateTime)lnd.OpenDate).Month;
                                    if (CRD_AGE_UNCLS_OLDEST < CRD_AGE)
                                        CRD_AGE_UNCLS_OLDEST = CRD_AGE;
                                }
                            }
                        }
                        statlnd.CRD_AGE_UNCLS_OLDEST = CRD_AGE_UNCLS_OLDEST;//最早未销户卡龄
                        #endregion
                        // L24M_OPE_NORM_ACCT_PCT & NORM_CDT_BAL_USED_PCT_AVG
                        #region L24M_OPE_NORM_ACCT_PCT & NORM_CDT_BAL_USED_PCT_AVG
                        var normallndlist = lndList.Where(lnd => lnd.State.Contains("正常"));
                        var filterlndlist = new List<CRD_CD_LNDEntity>();
                        foreach (var lnd in normallndlist)
                        {
                            var templnd = filterlndlist.Where(flnd => flnd.OpenDate == lnd.OpenDate).FirstOrDefault();
                            if (templnd != null)
                            {
                                if (lnd.CreditLimitAmount < templnd.CreditLimitAmount)
                                {
                                    lnd.CreditLimitAmount = templnd.CreditLimitAmount;
                                }
                                decimal NORM_CDT_USED = (decimal)lnd.UsedCreditLimitAmount;
                                NORM_CDT_USED += (decimal)templnd.UsedCreditLimitAmount;
                                lnd.UsedCreditLimitAmount = NORM_CDT_USED;
                                filterlndlist.Remove(templnd);
                                filterlndlist.Add(lnd);
                            }
                            else
                            {
                                filterlndlist.Add(lnd);
                            }
                        }
                        decimal NOW_NORM_UNI_ACCT_NUM = filterlndlist.Count();
                        decimal L24M_OPE_ACCT_NUM = 0;
                        List<decimal> LIST_NORM_CDT_BAL_USED_PCT = new List<decimal>();
                        var templndlist = new List<CRD_CD_LNDEntity>();
                        foreach (var lnd in lndList)
                        {
                            if (string.IsNullOrEmpty(lnd.Currency))
                                continue;
                            var templnd = templndlist.Where(flnd => flnd.OpenDate == lnd.OpenDate).FirstOrDefault();
                            if (templnd == null)
                            {
                                if ((entity.QueryTime != null || entity.ReportCreateTime != null) && lnd.OpenDate != null)
                                {
                                    var CRD_AGE = query_time.Year * 12 + query_time.Month - ((DateTime)lnd.OpenDate).Year * 12 - ((DateTime)lnd.OpenDate).Month;
                                    if (0 < CRD_AGE && CRD_AGE < 24)
                                    {
                                        L24M_OPE_ACCT_NUM++;
                                    }
                                }
                                templndlist.Add(lnd);
                            }
                        }
                        foreach (var lnd in filterlndlist)
                        {
                            decimal CDT_LMT_SUM = (decimal)lnd.CreditLimitAmount;
                            decimal NORM_CDT_USED = (decimal)lnd.UsedCreditLimitAmount;
                            decimal SUM_NORM_CDT_USED = CDT_LMT_SUM - NORM_CDT_USED;
                            decimal NORM_CDT_BAL_USED_PCT = 0;
                            if (SUM_NORM_CDT_USED != 0)
                            {
                                NORM_CDT_BAL_USED_PCT = NORM_CDT_USED / SUM_NORM_CDT_USED;
                                LIST_NORM_CDT_BAL_USED_PCT.Add(NORM_CDT_BAL_USED_PCT);
                            }
                        }
                        if (NOW_NORM_UNI_ACCT_NUM == 0)
                        {
                            statln.L24M_OPE_NORM_ACCT_PCT = -9999998;
                        }
                        else
                        {
                            var L24M_OPE_NORM_ACCT_PCT = L24M_OPE_ACCT_NUM / NOW_NORM_UNI_ACCT_NUM;
                            statln.L24M_OPE_NORM_ACCT_PCT = decimal.Parse(L24M_OPE_NORM_ACCT_PCT.ToString("#0.00"));//过去24个月内非共享信用卡账户开户数占当前正常账户比例
                            //entity.L24M_OPE_NORM_ACCT_PCT = L24M_OPE_NORM_ACCT_PCT == 0 ? 0 : decimal.Parse(L24M_OPE_NORM_ACCT_PCT.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//过去24个月内非共享信用卡账户开户数占当前正常账户比例
                        }
                        decimal NORM_CDT_BAL_USED_PCT_AVG = 0;
                        if (normallndlist.Count() == 0 || LIST_NORM_CDT_BAL_USED_PCT.Count == 0)
                        {
                            statlnd.NORM_CDT_BAL_USED_PCT_AVG = -9999998;
                        }
                        else
                        {
                            NORM_CDT_BAL_USED_PCT_AVG = LIST_NORM_CDT_BAL_USED_PCT.Sum() / LIST_NORM_CDT_BAL_USED_PCT.Count;

                            statlnd.NORM_CDT_BAL_USED_PCT_AVG = decimal.Parse(NORM_CDT_BAL_USED_PCT_AVG.ToString("#0.00"));//当前正常的信用卡账户最大负债额与透支余额之比的均值
                            //entity.NORM_CDT_BAL_USED_PCT_AVG = NORM_CDT_BAL_USED_PCT_AVG == 0 ? 0 : decimal.Parse(NORM_CDT_BAL_USED_PCT_AVG.ToString("#0.00").TrimEnd(new char[] { '0', '.' }));//当前正常的信用卡账户最大负债额与透支余额之比的均值
                        }

                        #endregion
                    }
                }
                #endregion

                #region statln新增变量6个（Kerry 20160524）
                //房屋贷款年化率为4%
                //住房贷款
                var ln_Mortgage = lnList.Where(o => o.TypeDw == "住房贷款" && o.State == "正常");
                decimal Monthly_Mortgage_Payment = 0;
                decimal Monthly_Mortgage_Payment_Total = 0;
                statln.Monthly_Mortgage_Payment_Max = 0;
                foreach (CRD_CD_LNEntity ln in ln_Mortgage)
                {
                    int Mortgage_Month = (((DateTime)ln.EndDate).Year - ((DateTime)ln.OpenDate).Year) * 12 + (((DateTime)ln.EndDate).Month - ((DateTime)ln.OpenDate).Month);

                    if (Mortgage_Month == 0)
                    {
                        Mortgage_Month++;
                    }
                    decimal month_rate = ((decimal)0.04) / 12;
                    Monthly_Mortgage_Payment = (decimal)(ln.CreditLimitAmount * month_rate * factorial((1 + month_rate), Mortgage_Month)) / (factorial((1 + month_rate), Mortgage_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                    if (statln.Monthly_Mortgage_Payment_Max < Monthly_Mortgage_Payment)
                    {
                        statln.Monthly_Mortgage_Payment_Max = Monthly_Mortgage_Payment;//最大月按揭还款额
                    }
                    Monthly_Mortgage_Payment_Total += Monthly_Mortgage_Payment;
                }
                statln.Monthly_Mortgage_Payment_Total = Monthly_Mortgage_Payment_Total;//月按揭还款总额

                //个人商用房
                var ln_Commercial = lnList.Where(o => o.TypeDw == "个人商用房" && o.State == "正常");
                decimal Monthly_Commercial_Mortgage_Payment = 0;
                decimal Monthly_Commercial_Mortgage_Payment_Total = 0;
                statln.Monthly_Commercial_Mortgage_Payment_Max = 0;
                foreach (CRD_CD_LNEntity ln in ln_Commercial)
                {
                    int Commercial_Month = (((DateTime)ln.EndDate).Year - ((DateTime)ln.OpenDate).Year) * 12 + (((DateTime)ln.EndDate).Month - ((DateTime)ln.OpenDate).Month);
                    if (Commercial_Month == 0)
                    {
                        Commercial_Month++;
                    }
                    decimal month_rate = ((decimal)0.04) / 12;
                    Monthly_Commercial_Mortgage_Payment = (decimal)(ln.CreditLimitAmount * month_rate * factorial((1 + month_rate), Commercial_Month)) / (factorial((1 + month_rate), Commercial_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                    if (statln.Monthly_Commercial_Mortgage_Payment_Max < Monthly_Commercial_Mortgage_Payment)
                    {
                        statln.Monthly_Commercial_Mortgage_Payment_Max = Monthly_Commercial_Mortgage_Payment;//最大商用房月按揭还款额
                    }
                    Monthly_Commercial_Mortgage_Payment_Total += Monthly_Commercial_Mortgage_Payment;
                }
                statln.Monthly_Commercial_Mortgage_Payment_Total = Monthly_Commercial_Mortgage_Payment_Total;//商用房月按揭还款总额

                //其他贷款
                decimal ln_other_amount = 0;
                decimal Monthly_Other_Mortgage_Payment = 0;
                decimal Monthly_Other_Mortgage_Payment_Total = 0;
                var ln_other = lnList.Where(o => o.TypeDw != "个人商用房" && o.TypeDw != "住房贷款" && o.TypeDw != "个人住房公积金贷款" && (o.State == "正常"));
                foreach (CRD_CD_LNEntity ln in ln_other)
                {
                    if ((decimal)ln.Balance == 0)//若余额为0（报告滞后，应算作结清，不予统计）
                    {
                        continue;
                    }
                    if (ln.Cue.Contains("截至"))//描述中是否存在“截至”，即是否有截至年月
                    {
                        try
                        {
                            string CalEndMonth_Str = CommonFun.GetMidStr(ln.Cue, "截至", "，");//截至年月字符串
                            if (!string.IsNullOrEmpty(CalEndMonth_Str))//截至年月是否为空
                            {
                                int CalEndMonth = int.Parse(CommonFun.GetMidStr(CalEndMonth_Str, "", "年")) * 100 + int.Parse(CommonFun.GetMidStr(CalEndMonth_Str, "年", "月"));//截至年月
                                if (CalEndMonth == int.Parse(((DateTime)ln.EndDate).ToString("yyyyMM")) && (decimal)ln.CreditLimitAmount == (decimal)ln.Balance)//到期月=截止月，余额等于本金，全额计入
                                {
                                    Monthly_Other_Mortgage_Payment_Total += (decimal)ln.CreditLimitAmount;//全额计入
                                    ln_other_amount += (decimal)ln.CreditLimitAmount;
                                    continue;
                                }
                                else if (CalEndMonth > int.Parse(((DateTime)ln.OpenDate).AddMonths(1).ToString("yyyyMM")) && (decimal)ln.CreditLimitAmount == (decimal)ln.Balance)//贷款到期月不等于截止月，且截止月>=2+贷款发放月，并且发放的贷款本金=余额，则此类贷款记0
                                {
                                    ln_other_amount += (decimal)ln.CreditLimitAmount;
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                        catch
                        {
                            continue;
                        }
                    }
                    int Mortgage_Month = (((DateTime)ln.EndDate).Year - ((DateTime)ln.OpenDate).Year) * 12 + (((DateTime)ln.EndDate).Month - ((DateTime)ln.OpenDate).Month);
                    if (Mortgage_Month == 0)
                    {
                        Mortgage_Month++;
                    }
                    decimal month_rate = ((decimal)0.06) / 12;
                    ln_other_amount += (decimal)ln.CreditLimitAmount;
                    //try
                    //{
                    Monthly_Other_Mortgage_Payment = (decimal)(ln.CreditLimitAmount * month_rate * factorial((1 + month_rate), Mortgage_Month)) / (factorial((1 + month_rate), Mortgage_Month) - 1);
                    //}
                    //catch
                    //{
                    //    Monthly_Other_Mortgage_Payment_Total = 9999999;
                    //    break;
                    //}
                    Monthly_Other_Mortgage_Payment_Total += Monthly_Other_Mortgage_Payment;
                }
                statln.ln_other_amount = ln_other_amount;//其他贷款额
                statln.Monthly_Other_Mortgage_Payment_Total = Monthly_Other_Mortgage_Payment_Total;

                #endregion

                #region 保证人代偿
                var asrrepayList = ASRREPAYDao.Find("from CRD_CD_ASRREPAYEntity where ReportId=?", new object[] { entity.Id });
                statln.assurerrepay_amount = (decimal)asrrepayList.Where(o => o.Money != null).Sum(o => o.Money);
                #endregion

                #region statln新增变量2个（Kerry 20160617）
                var ln_AccumFund = lnList.Where(o => o.TypeDw == "个人住房公积金贷款" && o.State != "结清");
                decimal AccumFund_Mon_Mort_Repay = 0;
                decimal Max_AccumFund_All_Mort_Repay = 0;
                statln.Max_AccumFund_Mon_Mort_Repay = 0;
                foreach (CRD_CD_LNEntity ln in ln_AccumFund)
                {
                    int AccumFund_Month = (((DateTime)ln.EndDate).Year - ((DateTime)ln.OpenDate).Year) * 12 + (((DateTime)ln.EndDate).Month - ((DateTime)ln.OpenDate).Month);

                    if (AccumFund_Month == 0)
                    {
                        AccumFund_Month++;
                    }
                    decimal month_rate = ((decimal)0.03) / 12;
                    AccumFund_Mon_Mort_Repay = (decimal)(ln.CreditLimitAmount * month_rate * factorial((1 + month_rate), AccumFund_Month)) / (factorial((1 + month_rate), AccumFund_Month) - 1);//等额本息月款额=[贷款本金×月利率×（1+月利率）^还款月数]÷[（1+月利率）^还款月数－1]，月利率=年利率/12
                    if (statln.Max_AccumFund_Mon_Mort_Repay < AccumFund_Mon_Mort_Repay)
                    {
                        statln.Max_AccumFund_Mon_Mort_Repay = AccumFund_Mon_Mort_Repay;//最大月按揭还款额
                    }
                    Max_AccumFund_All_Mort_Repay += AccumFund_Mon_Mort_Repay;
                }
                statln.Max_AccumFund_All_Mort_Repay = Max_AccumFund_All_Mort_Repay;//月按揭还款总额


                #endregion

                #region statlnd新增变量1个(hering 20160809)

                DateTime queryTime = new DateTime();
                queryTime = entity.QueryTime ?? (DateTime)entity.ReportCreateTime;

                //[征信]3个月内非银机构查询次数
                var listRECORDDTLEntity = recordList.Where(o => (!o.Querier.Contains("银行") && !o.Querier.Contains("花旗")) && (!o.QueryReason.Contains("本人") && !o.QueryReason.Contains("贷后") & o.QueryReason.Contains("贷款")));
                foreach (CRD_QR_RECORDDTLEntity item in listRECORDDTLEntity)
                {
                    if (item.QueryDate == null) continue;
                    if (queryTime.AddMonths(-3) < (DateTime)item.QueryDate)
                    {
                        statqr.COUNT_Nonbank_IN3M++;
                    }
                }

                #endregion

                entity.CRD_STAT_LN = statln;
                entity.CRD_STAT_LND = statlnd;
                entity.CRD_STAT_QR = statqr;

                entity.PubInfoSummary.AdminpnshmCount = ADMINPNSHMDao.Find("from CRD_PI_ADMINPNSHMEntity where ReportId=?", new object[] { entity.Id }).Count();
                entity.PubInfoSummary.CiviljdgmCount = CIVILJDGMDao.Find("from CRD_PI_CIVILJDGMEntity where ReportId=?", new object[] { entity.Id }).Count();
                entity.PubInfoSummary.ForceexctnCount = FORCEEXCTNDao.Find("from CRD_PI_FORCEEXCTNEntity where ReportId=?", new object[] { entity.Id }).Count();
                entity.PubInfoSummary.TaxarrearCount = TAXARREARDao.Find("from CRD_PI_TAXARREAREntity where ReportId=?", new object[] { entity.Id }).Count();
                entity.PubInfoSummary.TelpntCount = TELPNTDao.Find("from CRD_PI_TELPNTEntity where ReportId=?", new object[] { entity.Id }).Count();
                return entity;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private decimal factorial(decimal m, int n)
        {
            decimal ret = 1;
            for (int i = 0; i < n; i++)
            {
                ret = ret * m;
            }
            return ret;
        }
    }
}

