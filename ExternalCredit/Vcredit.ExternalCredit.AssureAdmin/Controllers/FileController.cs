using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vcredit.Common.Ext;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.AssureAdmin.Tools;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;

namespace Vcredit.ExternalCredit.AssureAdmin.Controllers
{
    public class FileController : BaseController
    {
        private List<UploadFilesResult> statuses = new List<UploadFilesResult>();

        private int dataType = -1;

        private BaseDao _thisDao = new BaseDao();

        /// <summary>
        /// 数据导入
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ImportAssureData()
        {
            return View();
        }

        [HttpGet]
        public void DownloadFile(long id)
        {
            var context = HttpContext;
            var entity = _thisDao.Select<CRD_CD_AssureFileEntity>(x => x.Id == id).FirstOrDefault();
            if (entity == null)
            {
                context.Response.StatusCode = 404;
                return;
            }
            var filePath = entity.FilePath;
            if (System.IO.File.Exists(filePath))
            {
                context.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + entity.FileName + "\"");
                context.Response.ContentType = "application/octet-stream";
                context.Response.ClearContent();
                context.Response.WriteFile(filePath);
            }
            else
                context.Response.StatusCode = 404;
        }

        /// <summary>
        /// 文件上传
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadAssureFile()
        {
            dataType = Convert.ToInt32(Request.Form["dataType"]);
            if (dataType <= -1)
            {
                for (int i = 0; i < Request.Files.Count; i++)
                {
                    var file = Request.Files[i];
                    var oldName = file.FileName;
                    this.AddResultList(0, oldName, "", file, "上传失败，请选择数据类型！");
                }
                return Json(new { files = statuses });
            }

            foreach (string file in Request.Files)
            {
                var fileName = Request.Headers["X-File-Name"];

                if (string.IsNullOrEmpty(fileName))
                    UploadWholeFile();
                else
                {
                    UploadPartialFile(fileName);
                }
            }
            return Json(new { files = statuses });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="oldName"></param>
        /// <param name="request"></param>
        private void UploadPartialFile(string oldName)
        {
            if (Request.Files.Count != 1)
                throw new HttpRequestValidationException("Attempt to upload chunked file containing more than one fragment per request");

            var file = Request.Files[0];
            var inputStream = file.InputStream;

            string guid = this.GetGuid();
            var fullPath = this.GetFullPath(guid, oldName);

            using (var fs = new FileStream(fullPath, FileMode.Append, FileAccess.Write))
            {
                var buffer = new byte[1024];

                var l = inputStream.Read(buffer, 0, 1024);
                while (l > 0)
                {
                    fs.Write(buffer, 0, l);
                    l = inputStream.Read(buffer, 0, 1024);
                }
                fs.Flush();
                fs.Close();
            }
            this.SaveDataToDb(file, guid, fullPath, oldName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        private void UploadWholeFile()
        {
            for (int i = 0; i < Request.Files.Count; i++)
            {
                var file = Request.Files[i];
                var oldName = file.FileName;
                string guid = this.GetGuid();
                var fullPath = this.GetFullPath(guid, oldName);
                file.SaveAs(fullPath);

                this.SaveDataToDb(file, guid, fullPath, oldName);
            }
        }

        private void SaveDataToDb(HttpPostedFileBase file, string guid, string fullPath, string oldName)
        {
            var fileEntity = new CRD_CD_AssureFileEntity
            {
                Guid = guid,
                FileName = oldName,
                FilePath = fullPath,
                FileType = file.ContentType,
                FileSize = file.ContentLength,

                DataType = dataType,
                State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.保存成功,
                StateDescription = Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.保存成功.ToString(),
            };
            _thisDao.Save(fileEntity);

            var fileId = fileEntity.Id;
            if (fileId <= 0)
                throw new ArgumentException("文件保存失败");

            var analisisResult = this.AnalisisData(fullPath, fileId);

            this.AddResultList(fileId, oldName, fullPath, file, !analisisResult ? "数据解析出现错误！" : string.Empty);
        }

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="fileId"></param>
        /// <returns></returns>
        private bool AnalisisData(string fullPath, long fileId)
        {
            try
            {
                var dt = NPOIHelper.GetDataTable(fullPath, true);

                if (dataType == (int)FileDataType.清贷非清贷)
                    this.AnalisisSquared(dt, fileId);
                else if (dataType == (int)FileDataType.代偿)
                    this.AnalisisBalanceTransfer(dt, fileId);

                var fileEntity = _thisDao.SingleById<CRD_CD_AssureFileEntity>(fileId);
                fileEntity.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.解析成功;
                fileEntity.StateDescription = Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.解析成功.ToString();
                _thisDao.Update(fileEntity);

                return true;
            }
            catch (Exception ex)
            {
                var fileEntity = _thisDao.SingleById<CRD_CD_AssureFileEntity>(fileId);
                fileEntity.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.解析失败;
                fileEntity.StateDescription = Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.解析失败.ToString();
                _thisDao.Update(fileEntity);

                Log4netAdapter.WriteError("数据解析出现异常！", ex);
                return false;
            }
        }

        /// <summary>
        /// 代偿
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileId"></param>
        private void AnalisisBalanceTransfer(DataTable dt, long fileId)
        {
            var dataList = new List<CRD_CD_AssureMaintainBalanceTransferEntity>();
            foreach (DataRow dr in dt.Rows)
            {
                var data = new CRD_CD_AssureMaintainBalanceTransferEntity();
                data.GuaranteeLetterCode = dr["业务号"].ToString();
                data.Warranteename = dr["姓名"].ToString();
                data.InkeepBalance = Convert.ToInt32(dr["剩余本金"]);
                data.Balancechangedate = Convert.ToDateTime(dr["收款时间"]);
                data.IsEnd = Convert.ToInt32(dr["是否清贷"]) == 1;
                data.BalanceTransferDate = Convert.ToDateTime(dr["转担保日期"]);
                data.BalanceTransferSum = Convert.ToInt32(dr["转担保汇总金额"]);
                data.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureReportState.NeedCheck;
                data.StateDescription = "导入入库（需要确认数据无误）";
                data.FileId = fileId;

                if (data.GuaranteeLetterCode.IsEmpty() ||
                    data.Warranteename.IsEmpty())
                {
                    throw new ArgumentException("导入数据“业务号”或“姓名”字段不能为空");
                }
                dataList.Add(data);
            }
            this.InsertByPage(dataList);
        }

        /// <summary>
        /// 清贷非清贷
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="fileId"></param>
        private void AnalisisSquared(DataTable dt, long fileId)
        {
            var dataList = new List<CRD_CD_AssureMaintainEntity>();
            foreach (DataRow dr in dt.Rows)
            {
                var data = new CRD_CD_AssureMaintainEntity();
                data.GuaranteeLetterCode = dr["业务号"].ToString();
                data.Warranteename = dr["姓名"].ToString();
                data.InkeepBalance = Convert.ToInt32(dr["剩余本金"]);
                data.Balancechangedate = Convert.ToDateTime(dr["收款时间"]);
                data.IsEnd = Convert.ToInt32(dr["是否清贷"]) == 1;
                data.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureReportState.NeedCheck;
                data.StateDescription = "导入入库（需要确认数据无误）";
                data.FileId = fileId;
                if (data.GuaranteeLetterCode.IsEmpty() ||
                    data.Warranteename.IsEmpty())
                {
                    throw new ArgumentException("导入数据“业务号”或“姓名”字段不能为空");
                }
                dataList.Add(data);
            }
            this.InsertByPage(dataList);
        }

        /// <summary>
        /// 分页插入
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataList"></param>
        private void InsertByPage<T>(List<T> dataList)
        {
            int pageIndex = 1;
            int pageSize = 100;
            var totalPage = Math.Ceiling((double)dataList.Count / (double)pageSize);
            while (pageIndex <= totalPage)
            {
                var insertlist = dataList.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                _thisDao.InsertAll(insertlist);
                pageIndex++;
            }
        }

        private string GetFullPath(string guid, string oldFileName)
        {
            var storageRoot = Path.Combine(Server.MapPath("~/UploadFiles/Assure"), DateTime.Now.ToString("yyyyMMdd"));
            if (!Directory.Exists(storageRoot))
                Directory.CreateDirectory(storageRoot);

            return Path.Combine(storageRoot, guid + Path.GetExtension(oldFileName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="oldName"></param>
        /// <param name="fullPath"></param>
        /// <param name="file"></param>
        /// <param name="error"></param>
        private void AddResultList(long id, string oldName, string fullPath, HttpPostedFileBase file, string error = "")
        {
            statuses.Add(new UploadFilesResult()
            {
                name = oldName,
                size = file.ContentLength,
                type = file.ContentType,
                url = "DownloadFile?id=" + id,
                //deleteUrl = "DeleteFile?id=" + id,
                //thumbnailUrl = this.GetThumbnailUrl(fullPath),
                deleteType = "GET",
                error = error,
            });
        }

        /// <summary>
        /// guid
        /// </summary>
        /// <returns></returns>
        private string GetGuid()
        {
            var guid = Guid.NewGuid().ToString("N");
            return guid;
        }

        #region 已导入数据
        /// <summary>
        /// 已导入数据
        /// </summary>
        /// <returns></returns>
        public ActionResult ImportedFile()
        {
            return View();
        }

        /// <summary>
        /// 获取上传文件列表
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public JsonResult Get_FileList(AssureFileQueryDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureFileEntity>>();

            BaseDao dao = new BaseDao();
            SqlExpression<CRD_CD_AssureFileEntity> sqlexp = dao.SqlExpression<CRD_CD_AssureFileEntity>();
            if (!param.FileName.IsEmpty())
            {
                sqlexp.Where(x => x.FileName == param.FileName);
            }
            if (param.State != null)
            {
                sqlexp.Where(x => x.State == param.State.Value);
            }
            if (param.CreateTimeBegin != null)
            {
                sqlexp.Where(x => x.Createtime >= param.CreateTimeBegin.Value);
            }
            if (param.CreateTimeEnd != null)
            {
                param.CreateTimeEnd = (param.CreateTimeEnd.Value.ToString("yyyy-MM-dd") + " 23:59:59").ToDateTime();
                sqlexp.Where(x => x.Createtime <= param.CreateTimeEnd.Value);
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
        /// 获取数据列表
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public JsonResult Get_FileDataList(long fileId, DataRequestDto param)
        {
            var entity = _thisDao.Select<CRD_CD_AssureFileEntity>(x => x.Id == fileId).FirstOrDefault();
            dynamic data = null;
            if (entity.DataType == (int)FileDataType.清贷非清贷)
            {
                data = this.GetAssureMaintainData(fileId, param);
            }
            else if (entity.DataType == (int)FileDataType.代偿)
            {
                data = this.GetAssureMaintainDataTransferInfo(fileId, param);
            }

            return new DateJsonResult("yyyy-MM-dd HH:mm") { Data = data };
        }
        private dynamic GetAssureMaintainData(long fileId, DataRequestDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureMaintainEntity>>();
            SqlExpression<CRD_CD_AssureMaintainEntity> sqlexp = _thisDao.SqlExpression<CRD_CD_AssureMaintainEntity>();
            sqlexp.Where(x => x.FileId == fileId);

            data.TotalCount = _thisDao.Count(sqlexp);   // 总数

            sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            data.Result = _thisDao.Select(sqlexp); // 分页数据

            data.PageIndex = param.PageIndex;
            data.PageSize = param.PageSize;
            data.EndTime = DateTime.Now.ToString();
            data.StatusCode = StatusCode.Success;
            data.StatusDescription = "数据查询成功";

            return data;
        }
        private dynamic GetAssureMaintainDataTransferInfo(long fileId, DataRequestDto param)
        {
            var data = new DataResponseDto<List<CRD_CD_AssureMaintainBalanceTransferEntity>>();
            SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity> sqlexp = _thisDao.SqlExpression<CRD_CD_AssureMaintainBalanceTransferEntity>();
            sqlexp.Where(x => x.FileId == fileId);

            data.TotalCount = _thisDao.Count(sqlexp);   // 总数

            sqlexp.OrderByDescending(x => x.Id).Skip((param.PageIndex - 1) * param.PageSize);
            sqlexp.Take(param.PageSize);
            data.Result = _thisDao.Select(sqlexp); // 分页数据

            data.PageIndex = param.PageIndex;
            data.PageSize = param.PageSize;
            data.EndTime = DateTime.Now.ToString();
            data.StatusCode = StatusCode.Success;
            data.StatusDescription = "数据查询成功";

            return data;
        }
        #endregion

        /// <summary>
        /// 文件删除
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult DeleteFileData(long id)
        {
            var entity = _thisDao.Select<CRD_CD_AssureFileEntity>(x => x.Id == id).FirstOrDefault();
            if (entity.State == (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.确认无误)
                return Json(new ApiResultDto<string> { StatusCode = StatusCode.Fail, StatusDescription = "已确认的数据无法删除！" });


            if (entity.DataType == (int)FileDataType.清贷非清贷)
            {
                var data = _thisDao.Select<CRD_CD_AssureMaintainEntity>(x => x.FileId == entity.Id).ToList();
                foreach (var item in data)
                {
                    _thisDao.Delete(item);
                }
            }
            else if (entity.DataType == (int)FileDataType.代偿)
            {
                var data = _thisDao.Select<CRD_CD_AssureMaintainBalanceTransferEntity>(x => x.FileId == entity.Id);
                foreach (var item in data)
                {
                    _thisDao.Delete(item);
                }
            }

            entity.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.数据已删除;
            entity.StateDescription = Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.数据已删除.ToString();
            _thisDao.Update(entity);

            return Json(new ApiResultDto<string> { StatusCode = StatusCode.Success, StatusDescription = "操作成功！" });
        }

        /// <summary>
        /// 确认数据无误
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public JsonResult ConfirmFileData(long id)
        {
            var entity = _thisDao.Select<CRD_CD_AssureFileEntity>(x => x.Id == id).FirstOrDefault();
            if (entity.State != (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.解析成功)
                return Json(new ApiResultDto<string> { StatusCode = StatusCode.Fail, StatusDescription = "只有解析成功的数据才能确认提交！" });


            if (entity.DataType == (int)FileDataType.清贷非清贷)
            {
                var data = _thisDao.Select<CRD_CD_AssureMaintainEntity>(x => x.FileId == entity.Id).ToList();
                foreach (var item in data)
                {
                    item.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureReportState.Default;
                    item.StateDescription = "数据导入，已确认无误，等待上报...";
                    _thisDao.Update(item);
                }
            }
            else if (entity.DataType == (int)FileDataType.代偿)
            {
                var data = _thisDao.Select<CRD_CD_AssureMaintainBalanceTransferEntity>(x => x.FileId == entity.Id);
                foreach (var item in data)
                {
                    item.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureReportState.Default;
                    item.StateDescription = "数据导入，已确认无误，等待上报...";
                    _thisDao.Update(item);
                }
            }

            entity.State = (int)Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.确认无误;
            entity.StateDescription = Vcredit.ExternalCredit.CommonLayer.SysEnums.AssureFileState.确认无误.ToString();
            _thisDao.Update(entity);

            return Json(new ApiResultDto<string> { StatusCode = StatusCode.Success, StatusDescription = "操作成功！" });
        }

    }

    /// <summary>
    /// 数据类型
    /// </summary>
    public enum FileDataType
    {
        清贷非清贷 = 0,
        代偿 = 1,
    }

    /// <summary>
    /// 
    /// </summary>
    public class UploadFilesResult
    {
        public string url { get; set; }
        public string thumbnailUrl { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public string deleteUrl { get; set; }
        public string deleteType { get; set; }
        public string error { get; set; }
    }
}