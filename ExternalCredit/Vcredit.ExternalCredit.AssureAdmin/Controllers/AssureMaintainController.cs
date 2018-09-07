using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Ext;
using Vcredit.ExternalCredit.AssureAdmin.Tools;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;


namespace Vcredit.ExternalCredit.AssureAdmin.Controllers
{
    public class AssureMaintainController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MaintainList()
        {
            return View();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_DataList(AssureMaintainQueryDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureMaintainEntity>>();

            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureMaintainEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureMaintainEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }

            data.TotalCount = dao.Count(sqlexp);   // 总数

            sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            data.Result = dao.Select(sqlexp); // 分页数据

            data.PageIndex = param.PageIndex;
            data.PageSize = param.PageSize;
            data.EndTime = DateTime.Now.ToString();
            data.StatusCode = StatusCode.Success;
            data.StatusDescription = "数据查询成功";

            return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileResult Export(AssureReportedQueryDto param)
        {
            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureMaintainEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureMaintainEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }
            // 最多取1000条数据
            sqlexp.OrderByDescending(x => x.Id).Take(200000);
            var data = dao.Select(sqlexp); // 分页数据


            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                                        new DataColumn { ColumnName = "担保业务编号", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "姓名", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "在保余额", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "余额变化日期", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "是否清贷", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "状态", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "描述", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "上传日期", DataType = typeof(string) }
                                    }
                                );
            foreach (var item in data)
            {
                dt.Rows.Add(new object[] {
                    item.GuaranteeLetterCode ,
                    item.Warranteename,
                    item.InkeepBalance,
                    item.Balancechangedate.ToString("yyyy-MM-dd"),
                    item.IsEnd,
                    item.State,
                    item.StateDescription,
                    item.UpdateTime,
                });
            }

            //第一种:使用FileContentResult
            byte[] fileContents = Vcredit.ExtTrade.CommonLayer.NPOIHelper.x2007.TableToExcelBytes(dt, "数据导出");
            return File(fileContents, "application/ms-excel", string.Format("正常在保余额维护_数据导出_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHss")));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult MaintainBalanceTransferList()
        {
            return View();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Get_DataBalanceTransferList(AssureMaintainBalanceTransferQueryDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureMaintainBalanceTransferEntity>>();

            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }

            data.TotalCount = dao.Count(sqlexp);   // 总数

            sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            data.Result = dao.Select(sqlexp); // 分页数据

            data.PageIndex = param.PageIndex;
            data.PageSize = param.PageSize;
            data.EndTime = DateTime.Now.ToString();
            data.StatusCode = StatusCode.Success;
            data.StatusDescription = "数据查询成功";

            return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
        }

        /// <summary>
        /// 导出Excel
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public FileResult ExportBalanceTransfer(AssureMaintainBalanceTransferQueryDto param)
        {
            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity>();
            if (!param.GuaranteeLetterCode.IsEmpty())
            {
                sqlexp.Where(x => x.GuaranteeLetterCode == param.GuaranteeLetterCode);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.UpdateTimeBegin != null)
            {
                sqlexp.Where(x => x.UpdateTime >= param.UpdateTimeBegin.Value);
            }
            if (param.UpdateTimeEnd != null)
            {
                sqlexp.Where(x => x.UpdateTime < param.UpdateTimeEnd.Value);
            }
            // 最多取1000条数据
            sqlexp.OrderByDescending(x => x.Id).Take(200000);
            var data = dao.Select(sqlexp); // 分页数据


            var dt = new DataTable();
            dt.Columns.AddRange(new[] {
                                        new DataColumn { ColumnName = "担保业务编号", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "姓名", DataType = typeof(string) },
                                        //new DataColumn { ColumnName = "在保余额/剩余本金", DataType = typeof(string) },
                                        //new DataColumn { ColumnName = "收款时间", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "是否清贷", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "代偿日期", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "代偿金额", DataType = typeof(string) },

                                        new DataColumn { ColumnName = "状态", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "描述", DataType = typeof(string) },
                                        new DataColumn { ColumnName = "上传日期", DataType = typeof(string) }
                                    }
                                );
            foreach (var item in data)
            {
                dt.Rows.Add(new object[] {
                    item.GuaranteeLetterCode ,
                    item.Warranteename,
                    //item.InkeepBalance,
                    //item.Balancechangedate.ToString("yyyy-MM-dd"),
                    item.IsEnd,

                    item.BalanceTransferDate.ToString("yyyy-MM-dd"),
                    item.BalanceTransferSum,

                    item.State,
                    item.StateDescription,
                    item.UpdateTime,
                });
            }

            //第一种:使用FileContentResult
            byte[] fileContents = Vcredit.ExtTrade.CommonLayer.NPOIHelper.x2007.TableToExcelBytes(dt, "数据导出");
            return File(fileContents, "application/ms-excel", string.Format("担保赔付代偿维护_数据导出_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHss")));
        }

    }

}