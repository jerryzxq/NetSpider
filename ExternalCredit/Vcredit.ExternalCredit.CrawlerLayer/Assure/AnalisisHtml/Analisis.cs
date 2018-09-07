using Newtonsoft.Json;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Vcredit.Common.Utility;
using Vcredit.ExternalCredit.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.BusinessLayer.CommonBusiness;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;
namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    public class Analisis
    {
        #region 根据标题归类 Element
        Dictionary<string, Element[]> SplitElements(Document doc)
        {
            Dictionary<string, Element[]> trdic = new Dictionary<string, Element[]>();//存储表和数据的字典
            List<Element> elelist = new List<Element>();
            List<Element> reportbodyeles = null;
            Element reportHead = null;
            Element firstele = null;
            Element secondele = null;
            string itemtext = string.Empty;
            string key = string.Empty;
            //过滤掉没有用的信息
            GetRightContenttr(doc, out reportHead, out reportbodyeles);
            //第一个report表格
            trdic.Add(CommonData.creditReportTable, new Element[] { reportHead, reportbodyeles.First() });

            for (int i = 1; i < reportbodyeles.Count; i++)
            {
                firstele = reportbodyeles[i];
                secondele = reportbodyeles[i + 1];
                itemtext = firstele.Text().Trim();
                if (itemtext == CommonData.reportDescription)//报告说明以下的信息不需要
                {
                    trdic.Add(DeleteSpecialChar(key), copyEleList(elelist));
                    elelist.Clear();
                    break;
                }
                var table = firstele.GetElementsByTag(CommonData.table);
                var table1 = secondele.GetElementsByTag(CommonData.table);
                //判断是否是标题
                if ((table == null || table.Count == 0) &&//当前行没有table
                    itemtext.Length < 50 && //且文本小于50
                      ((table1 != null && table1.Count != 0) || //且下一行有table或者没有table且文本大等于50 就是标题
                      ((table1 == null || table1.Count == 0) && secondele.Text().Trim().Length >= 50)))
                {
                    if (elelist.Count > 0)
                    {
                        trdic.Add(DeleteSpecialChar(key), copyEleList(elelist));
                        elelist.Clear();
                    }
                    key = itemtext;
                }
                else if ((table != null && table.Count != 0) || //如果有table 
                    ((table == null || table.Count == 0) &&  //或者 没有table且文本长度大等于50 就是内容
                    itemtext.Length >= 50))
                {
                    elelist.Add(firstele);
                }
            }
            return trdic;
        }
        Element[] copyEleList(List<Element> list)
        {
            Element[] elearray = new Element[list.Count];
            list.CopyTo(elearray);
            return elearray;

        }
        string DeleteSpecialChar(string title)
        {
            if (title.StartsWith("（"))
            {
                title = title.Substring(title.IndexOf("）") + 1).Trim();
            }
            return title;
        }
        void GetRightContenttr(Document doc, out Element reportele, out List<Element> bodyeles)
        {

            reportele = doc.Body.Children.Where(x=>x.Tag.Name=="table").FirstOrDefault();
            bodyeles = doc.Body.Children.Last.Children[0].Children.Where(x => !string.IsNullOrWhiteSpace(x.Text())).ToList();

        }

        #endregion

        #region  归类好的element转化成DataTable

        Dictionary<string, DataTable> ConvertToDataTable(Dictionary<string, Element[]> dicele)
        {
            HaSpecialWork haspecialWork = new HaSpecialWork();
            Dictionary<string, DataTable> dic = new Dictionary<string, DataTable>();
            foreach (var item in haspecialWork.Firstlist)
            {
                foreach (var spitem in item.DealingSpecialWork(dicele))
                {
                    dic.Add(spitem.Key, spitem.Value);
                }
            }
            foreach (var item in haspecialWork.Finallist)
            {
                item.DealingSpecialWork(dic);
            }
            return dic;
        }

        #endregion
        public bool SaveData(string identityCard, string html)
        {
            Dictionary<string, Dictionary<string, DataTable>> dicList = new Dictionary<string, Dictionary<string, DataTable>>();
            Dictionary<string, Document> dicDoc = new Dictionary<string, Document>();
            dicDoc.Add(identityCard, NSoupClient.Parse(html));
            foreach (var item in dicDoc)
            {
                try
                {
                  
                    dicList.Add(item.Key,
                                  ConvertToDataTable(//2.转为Datatable
                                  SplitElements(item.Value)));//1.分表
                }
                catch (Exception ex)
                {
                   // failFileNameList.Add(item.Key); 
                    Log4netAdapter.WriteError("文件：" + item.Key + "，解析时出现问题", ex);
                    throw ex;
                }
            }

            bool result = true;
            if (dicList.Count != 0)
            {
                Log4netAdapter.WriteInfo("html开始入库操作");
                var json = JsonConvert.SerializeObject(dicList.First().Value);
             
                var report= new BridgingBusiness().DanBaoSaveDataToDb(dicList);
                Dictionary<string,long> dic =new Dictionary<string,long> ();
                dic.Add(report.Report_Sn,(long)report.Report_Id);
                Func<ExtTradeJson> fun = () => { return new ExtTradeJson { ExtJson =json, ReportIdList = dic }; };
                SengData.SendAcction(fun,"担保身份证号"+identityCard);
               // CreditInfoPush.current.PushCredit(MapperHelper.Map<CRD_HD_REPORTEntity, Vcredit.ExtTrade.ModelLayer.Nolmal.CRD_HD_REPORTEntity>(report), SysEnums.SourceType.Assure,(int)RequestState.SuccessCome);//查詢失敗情況處理
                Log4netAdapter.WriteInfo("html结束入库");
            }
            else
            {
                result = false;
            }
            return result;
        }
    
    }
}
