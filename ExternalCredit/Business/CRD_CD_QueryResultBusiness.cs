using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExtTrade.BusinessLayer
{
    public  class CRD_CD_QueryResultBusiness
    {
        BaseDao dao = new BaseDao();
        OperatorLog opeLog = new OperatorLog(FileType.TxtFile);
        public CRD_CD_QueryResultBusiness() { }
        public CRD_CD_QueryResultBusiness(OperatorLog opeLog)
        {
            this.opeLog = opeLog;
        }
        /// <summary>
        ///批量插入数据
        /// </summary>
        /// <param name="dicList"></param>
        /// <returns>错误信息，如果成功就是空值</returns>
        public string InsertDicList(Dictionary<string, List<CRD_CD_QueryResultEntity>> dicList)
        {
            StringBuilder sb = new StringBuilder();
            NolmalBusinesshelper helper = new NolmalBusinesshelper();
            foreach(var  item in dicList)
            {
               try
               {
                   dao.InsertAll(item.Value);
                   opeLog.AddSuccessNum(1);
                   helper.MoveCommitExcel(item.Key);
               }
               catch(Exception ex)
               {
                   sb.AppendLine("fileName:" + item.Key + ",插入失败。" + ex.Message);
                   opeLog.AddFailNum(1).AddfailReson(new FailReason(item.Key,"","",ex.Message));
               }
               
            }
            return sb.ToString();

        }
    }
}
