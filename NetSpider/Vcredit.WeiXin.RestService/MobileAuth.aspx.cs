using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Vcredit.Common.Constants;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Service;

namespace Vcredit.WeiXin.RestService
{
    public partial class MobileAuth : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btn_update_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.tb_mobile.Text != "")
                {
                    ISummary service = NetSpiderFactoryManager.GetSummaryService();
                    if (service.UpdateRealNameAuth(this.tb_identityNo.Text,this.tb_mobile.Text))
                    {
                        HttpHelper httpHelper = new HttpHelper();
                        HttpItem httpItem = new HttpItem() 
                        {
                            URL =AppSettings.kkdWeChatService+ "facade/customer/selectMobile",
                            Method="post",
                            ContentType = "application/json",
                            Postdata = "{\"identityNo\":\"" + this.tb_identityNo.Text + "\",\"mobile\":\"" + this.tb_mobile.Text + "\"}"
                        };
                        var result = httpHelper.GetHtml(httpItem);
                        this.lbl_sm.Text ="已修改为认证状态";
                    }
                }
               
            }
            catch(Exception ex)
            {
                this.lbl_sm.Text=ex.Message;
            }
           
        }
    }
}