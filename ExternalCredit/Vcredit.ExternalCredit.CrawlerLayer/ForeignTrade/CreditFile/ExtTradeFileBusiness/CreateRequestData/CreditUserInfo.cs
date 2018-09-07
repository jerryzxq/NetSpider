using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ExtTrade.ModelLayer;
using Vcredit.ExtTrade.CommonLayer;
using Vcredit.ExtTrade.BusinessLayer;
using Vcredit.ExtTrade.ModelLayer.Common;
namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{

    public class CreditUserInfo
    {

        FileOperator fileOpe = new FileOperator();
        string directoryName;
        public CreditUserInfo ()
        {
            directoryName = fileOpe.GetDirectoryName("",true);
        }
        /// <summary>
        ///上传用户信息
        /// </summary>
        /// <param name="userInfoDic"></param>
        public void UploadCreditUserInfo(List<CRD_CD_CreditUserInfoEntity> userInfoList)
        {
            CRD_CD_CreditUserInfoBusiness creditBus = new CRD_CD_CreditUserInfoBusiness();
            FileUp fileUp = new FileUp();
            string saveDirectory = ConfigData.requestFileSavePath + directoryName;
            fileOpe.CreateNotExistDirectory(saveDirectory);
            string[] filepaths = new string[2]; 
            foreach (var userInfo in userInfoList)
            {
                userInfo.LocalDirectoryName = directoryName;
                fileUp.SaveFile(Convert.FromBase64String(userInfo.AuthorizationFileBase64String), 
                    saveDirectory + "\\" + userInfo.NameFirstLetter + "_" + userInfo.Cert_No + ".zip");

            }
            creditBus.InsertList(userInfoList);
        }
        private string CreatejpgName(CRD_CD_CreditUserInfoEntity userInfo ,string fileType,string saveDirectory)
        {
            return saveDirectory + "\\" + fileType + "_" + ChineseToSpell.ConvertToFirstSpell(userInfo.Name)+ "_" + userInfo.Cert_No + "_" + directoryName + ".jpg";
        }
        private string CreateZipName(CRD_CD_CreditUserInfoEntity userInfo,string saveDirectory)
        {
            return saveDirectory + "\\" + ChineseToSpell.ConvertToFirstSpell(userInfo.Name) + "_" + userInfo.Cert_No + ".zip";
        }
        
         
    }
}
