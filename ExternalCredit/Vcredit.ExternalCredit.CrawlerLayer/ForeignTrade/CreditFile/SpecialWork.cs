using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.ModelLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    sealed class  SpecialWork
    {


       internal List<ICreditExcelMap> list = new List<ICreditExcelMap>();
       internal SpecialWork()
       {
           Dictionary<string,Type> columnDatas = new Dictionary<string,Type> ()
           { {"Count_Dw",typeof(decimal)},{ "Months",typeof(decimal)}, {"Highest_Oa_Per_Mon",typeof(decimal)},
           {"Max_Duration",typeof(decimal)}, {"Type_Dw",typeof(string)} };
           Dictionary<string, Type> columnREORDSMRDatas =new Dictionary<string,Type> ()
           { { "Sum_Dw",typeof(decimal) }, { "Type_Id" ,typeof(string)} };

           list.Add(new CombineField());

           list.Add(new CombineTable(CommonData.CRD_PI_IDENTITY, CommonData.CRD_PI_IDENTITYMate));
           list.Add(new CombineTable(CommonData.CRD_IS_CREDITCUE,CommonData.CRD_IS_CREDITCUEScore));

           list.Add(new TypeCredit(CommonData.BusinessTypeArray));

           list.Add(new MixinType(CommonData.CRD_IS_OVDSUMMARY, CommonData.OveDueType, columnDatas));
           list.Add(new MixinType(CommonData.CRD_QR_REORDSMR, CommonData.Type_Ids.Keys.ToArray(), columnREORDSMRDatas));

       }
       internal SpecialWork Add(ICreditExcelMap impSpeialWork)
       {
           if (impSpeialWork != null)
           {
               list.Add(impSpeialWork);
           }
           return this;
       } 

 
        
     
    }
}
