using NSoup.Nodes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{
    sealed class  HaSpecialWork
    {

        /// <summary>
        /// 最后特殊处理工作
        /// </summary>
       internal List<ICreditSpecialWork<DataTable>> Finallist = new List<ICreditSpecialWork<DataTable>>();
        /// <summary>
        /// 一开始处理的工作
        /// </summary>
       internal List<ICreditSpecialWork<Element[]>> Firstlist = new List<ICreditSpecialWork<Element[]>>();
       internal HaSpecialWork()
       {
           Dictionary<string,Type> columnDatas = new Dictionary<string,Type> ()
           { {"Count_Dw",typeof(decimal)},{ "Months",typeof(decimal)}, {"Highest_Oa_Per_Mon",typeof(decimal)},
           {"Max_Duration",typeof(decimal)}, {"Type_Dw",typeof(string)} };
           Dictionary<string, Type> columnREORDSMRDatas =new Dictionary<string,Type> ()
           { { "Sum_Dw",typeof(decimal) }, { "Type_Id" ,typeof(string)} };
           Finallist.Add(new HaCombineTable(CommonData.CRD_PI_IDENTITY, CommonData.CRD_PI_IDENTITYMate));
           Finallist.Add(new HaCombineTable(CommonData.CRD_IS_CREDITCUE, CommonData.CRD_IS_CREDITCUEScore));
           Finallist.Add(new HaMixinType("逾期（透支）信息汇总", CommonData.OveDueType, columnDatas));
          // Finallist.Add(new HaMixinType("逾期（透支）信息汇总", CommonData.OveDueType, columnDatas));
           Finallist.Add(new HaMixinType(CommonData.CRD_QR_REORDSMR, CommonData.Type_Ids.Keys.ToArray(), columnREORDSMRDatas));

           Firstlist.Add(new TeleCombineTr());
           Firstlist.Add(new MainReport());
           Firstlist.Add(new HaCombineField());
           Firstlist.Add(new NomalDealing());
      
       }
   
       internal HaSpecialWork FinalAdd(ICreditSpecialWork<DataTable> impSpeialWork)
       {
           if (impSpeialWork != null)
           {
               Finallist.Add(impSpeialWork);
           }
           return this;
       }

       internal HaSpecialWork FirstAdd(ICreditSpecialWork<Element[]> impSpeialWork)
       {
           if (impSpeialWork != null)
           {
               Firstlist.Add(impSpeialWork);
           }
           return this;
       } 
 
        
     
    }
}
