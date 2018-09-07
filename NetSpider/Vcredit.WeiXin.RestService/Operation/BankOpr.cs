using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using Vcredit.WeiXin.RestService.Models;
using Vcredit.Common;

namespace Vcredit.WeiXin.RestService.Operation
{
    public class BankOpr
    {
        //银行卡列表文件路径
        protected string _bankFilePath = ConfigurationManager.AppSettings["bankfilepath"];
        /// <summary>
        /// 根据路径读取银行卡列表
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        protected List<BankCardInfo> LoadDataFromExcel(string filePath = "")
        {
            List<BankCardInfo> list = null;
            try
            {
                filePath = filePath.Length > 0 ? filePath : _bankFilePath;
                string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Extended Properties='Excel 8.0;HDR=False;IMEX=1'";
                OleDbConnection OleConn = new OleDbConnection(strConn);
                OleConn.Open();
                String sql = "SELECT 发卡行名称及机构代码 as BankName,长度 as Length,取值 as [Value],卡种 as CardType FROM  [Sheet1$]";
                OleDbDataAdapter OleDaExcel = new OleDbDataAdapter(sql, OleConn);
                DataSet OleDsExcle = new DataSet();
                OleDaExcel.Fill(OleDsExcle, "Sheet1");
                OleConn.Close();

                if (OleDsExcle.Tables.Count > 0)
                {
                    list = DataTableEntityBuilder<BankCardInfo>.ConvertToList(OleDsExcle.Tables[0]);
                }
            }
            catch (Exception ex)
            {

            }
            return list;
        }
        /// <summary>
        /// 获取银行卡列表
        /// </summary>
        /// <returns></returns>
        protected List<BankCardInfo> GetBankCardList()
        {
            List<BankCardInfo> list = null;
            object obj = CacheHelper.GetCache("BankCardList");
            if (obj == null)
            {
                list = LoadDataFromExcel();
                CacheHelper.SetCacheByFileDependency("BankCardList", list, _bankFilePath);
            }
            else
            {
                list = obj as List<BankCardInfo>;
            }

            return list;
        }
        /// <summary>
        /// 匹配银行卡信息
        /// </summary>
        /// <param name="bankName">银行名称</param>
        /// <param name="cardNo">卡号</param>
        /// <param name="cardType">卡种</param>
        /// <returns></returns>
        public bool MatchBankCard(string bankName, string cardNo, string cardType)
        {
            bool result = true;
            try
            {
                List<BankCardInfo> list = GetBankCardList();

                BankCardInfo info = list.Where(o => o.BankName.Contains(bankName)
                                                    && o.CardType == cardType
                                                       && (o.Length == "2" && o.Value == cardNo.Substring(0, 2)
                                                        || o.Length == "3" && o.Value == cardNo.Substring(0, 3)
                                                        || o.Length == "4" && o.Value == cardNo.Substring(0, 4)
                                                        || o.Length == "5" && o.Value == cardNo.Substring(0, 5)
                                                        || o.Length == "6" && o.Value == cardNo.Substring(0, 6)
                                                        || o.Length == "7" && o.Value == cardNo.Substring(0, 7)
                                                        || o.Length == "8" && o.Value == cardNo.Substring(0, 8)
                                                        || o.Length == "9" && o.Value == cardNo.Substring(0, 9)
                                                        || o.Length == "10" && o.Value == cardNo.Substring(0, 10))
                                                    ).FirstOrDefault();

                result = info != null;
            }
            catch (Exception ex)
            {
                result = false;
            }
            return result;
        }
    }
}