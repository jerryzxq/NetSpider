using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade;
using Vcredit.ExternalCredit.Dto;
using Vcredit.ExternalCredit.Dto.Assure;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer.Nolmal;
using Vcredit.ExtTrade.ModelLayer.Nolmal.NewForeignTradeModel;

namespace TestProgramLayer
{

    class ImportCreditQuery
    {
        static HttpResult PostDataToUrl(string url, string data)
        {
            HttpItem httpItem = new HttpItem()
            {
                URL = url,
                Method = "POST",
                Postdata = data,
                // Encoding =Encoding.UTF8,
                PostEncoding = Encoding.UTF8,
                ContentType = "application/json"

            };
           return new HttpHelper().GetHtml(httpItem);

        }
      
        public void Import()
        {
            string path = ConfigurationHelper.GetAppSetting("CreditExcelPath");
            string busType=ConfigurationHelper.GetAppSetting("BusType");
            DirectoryInfo dirInof = new DirectoryInfo(path);

            foreach (var file in dirInof.GetFiles())
            {
                if (file.Name.StartsWith("WaiMao"))
                {
                    ImportWMData(busType, file);

                }
                else if (file.Name.StartsWith("DanBao"))
                {
                    ImportDBData(busType, file);
                }
            }

        }

        private static void ImportDBData(string busType, FileInfo file)
        {
            var url = ConfigurationHelper.GetAppSetting("DBServicePath");
            var dataTable = NPOIHelper.GetDataTable(file.FullName, true);
            foreach (DataRow dr in dataTable.Select())
            {
                var entity = new AssureQueryUserInfoParamDto
                {
                    ApplyID = 0,
                    BusType = busType,
                    CertNo = dr["CertNo"].ToString(),
                    Name = dr["Name"].ToString(),
                    CertType = "0",
                    QueryReason = "01"
                };
                var result = PostDataToUrl(url, JsonConvert.SerializeObject(entity));
                if(result.StatusCode==System.Net.HttpStatusCode.OK)
                {
                    var reEntity = JsonConvert.DeserializeObject<ApiResultDto<AddQueryInfoResultDto>>(result.Html);
                    if (!reEntity.IsSuccess)
                    {
                        var resDes = string.Format(@"CertNo:{0},Name:{1},StatusDescription:{2}.StatusCode:{3}
                            ", entity.CertNo, entity.Name, reEntity.StatusDescription, reEntity.StatusCode.ToString());
                        Log4netAdapter.WriteInfo(resDes);
                        Console.WriteLine(resDes);
                    }
                }
                else
                {
                    Log4netAdapter.WriteInfo(entity.CertNo+"担保请求失败"+result.StatusDescription);
                    Console.WriteLine(entity.CertNo+"担保请求失败"+result.StatusDescription);
                }
            }
        }
        private static void ImportWMData(string busType, FileInfo file)
        {
            var url = ConfigurationHelper.GetAppSetting("WMServicePath");
            var dataTable = NPOIHelper.GetDataTable(file.FullName, true);
            foreach (DataRow dr in dataTable.Select())
            {
                var entity = new SingleCreditReq
                {
                    ApplyID = 0,
                    BusType = busType,
                    idNo = dr["CertNo"].ToString(),
                    cifName = dr["Name"].ToString(),
                    idType = "0",
                    crpReason = "01",
                    czAuth = "1",
                    czId = "1"

                };
                var result = PostDataToUrl(url, JsonConvert.SerializeObject(entity));
                if(result.StatusCode==System.Net.HttpStatusCode.OK)
                {
                    var reEntity = JsonConvert.DeserializeObject<BaseRes>(result.Html);
                    if (reEntity.StatusCode != (int)StatusCode.Success)
                    {
                        var resDes = string.Format(@"CertNo:{0},Name:{1},StatusDescription:{2}.StatusCode:{3}
                            ", entity.idNo, entity.cifName, reEntity.StatusDescription, reEntity.StatusCode.ToString());
                        Log4netAdapter.WriteInfo(resDes);
                        Console.WriteLine(resDes);
                    }
                }
                else
                {
                    Log4netAdapter.WriteInfo(entity.idNo+"外贸请求失败"+result.StatusDescription);
                    Console.WriteLine(entity.idNo+"外贸请求失败"+result.StatusDescription);
                }
            }
        }
      
    }
}
