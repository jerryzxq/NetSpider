﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.CommonLayer;

namespace Vcredit.ExternalCredit.CrawlerLayer.Assure.AnalisisHtml
{

    class HaTypeCredit :  ICreditSpecialWork<NSoup.Nodes.Element[]>
    {

        string[] businessTypeArray;
        public HaTypeCredit(string[] businessTypeArray)
        {
            this.businessTypeArray = businessTypeArray;
        }
        void DealOtherTable(Dictionary<string, DataTable> dic, string title)
        {
            if (!dic.Keys.Contains(title))
                return;
            DataTable Specialdt = dic[title];
            DataRow[] typeTable = null;
            foreach (string item in businessTypeArray)
            {
               typeTable = Specialdt.Select(CommonData.businessTypeStr + "='" + item + "'");
               if(typeTable!=null &&typeTable.Length>0)
               {
                   dic.Add(item + title, typeTable.CopyToDataTable());
               }
            }
            dic.Remove(title);
        }
        public Dictionary<string, System.Data.DataTable> DealingSpecialWork(Dictionary<string, System.Data.DataTable> dic)
        {
            
            DealOtherTable(dic, CommonData.specialTradeStr);
            DealOtherTable(dic, CommonData.overDueStr);
            return dic;
        }

        public Dictionary<string, DataTable> DealingSpecialWork(Dictionary<string, NSoup.Nodes.Element[]> dic)
        {
            throw new NotImplementedException();
        }
    }
}