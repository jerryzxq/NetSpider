using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.BusinessLayer
{
    public class CRD_QR_RECORDDTLBusiness
    {
        BaseDao dao = new BaseDao();
        public CRD_QR_RECORDDTLEntity Get(decimal reportid)
        {
            return dao.Single<CRD_QR_RECORDDTLEntity>(x => x.Report_Id == reportid);
        }

        public void RefreshData()
        {
            List<CRD_HD_REPORTEntity> reportList = dao.Select<CRD_HD_REPORTEntity>("SELECT Report_Id,Report_Create_Time,Cert_No FROM credit.CRD_HD_REPORT WHERE SourceType=10 AND Time_Stamp>'2016-10-1' AND Report_Id NOT IN(SELECT Report_Id FROM credit.CRD_QR_RECORDDTL)");

            int i = 1;
            foreach (var item in reportList)
            {
                List<CRD_QR_RECORDDTLINFOEntity> recordList = dao.Select<CRD_QR_RECORDDTLINFOEntity>(x => x.Report_Id == item.Report_Id);
                try
                {
                    if (recordList != null && recordList.Count != 0)
                    {
                        try
                        {
                            CRD_QR_RECORDDTLEntity entity = new CRD_QR_RECORDDTLEntity() { Report_Id = item.Report_Id };
                            DateTime lastthreeMonth = item.Report_Create_Time.Value.AddMonths(-3);
                            DateTime lastoneMonth = item.Report_Create_Time.Value.AddMonths(-1);

                            var drs = recordList.Where(x => x.Query_Reason == "贷款审批" && DateTime.Compare(x.Query_Date.Value, lastthreeMonth) > 0 && DateTime.Compare(x.Query_Date.Value, item.Report_Create_Time.Value) <= 0);
                            entity.COUNT_loan_IN3M = GetTimeNum(drs);
                            drs = recordList.Where(x => x.Query_Reason == "信用卡审批" && DateTime.Compare(x.Query_Date.Value, lastthreeMonth) > 0 && DateTime.Compare(x.Query_Date.Value, item.Report_Create_Time.Value) <= 0);
                            entity.COUNT_CARD_IN3M = GetTimeNum(drs);
                            drs = recordList.Where(x => x.Query_Reason == "贷款审批" && DateTime.Compare(x.Query_Date.Value, lastoneMonth) > 0 && DateTime.Compare(x.Query_Date.Value, item.Report_Create_Time.Value) <= 0);
                            entity.COUNT_loan_IN1M = GetTimeNum(drs);
                            drs = recordList.Where(x => x.Query_Reason == "信用卡审批" && DateTime.Compare(x.Query_Date.Value, lastoneMonth) > 0 && DateTime.Compare(x.Query_Date.Value, item.Report_Create_Time.Value) <= 0);
                            entity.COUNT_CARD_IN1M = GetTimeNum(drs);
                            dao.Insert<CRD_QR_RECORDDTLEntity>(entity);
                            Console.WriteLine("报告ID:" + item.Report_Id + ",记录数：" + i);
                            i++;
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }
                catch (Exception)
                {
                }
            }

        }

        private int GetTimeNum(IEnumerable<CRD_QR_RECORDDTLINFOEntity> recordList)
        {
            if (recordList == null || recordList.Count() == 0)
                return 0;
            else
                return recordList.Count();

        }


    }
}
