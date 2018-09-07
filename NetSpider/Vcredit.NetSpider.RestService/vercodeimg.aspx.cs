using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Vcredit.NetSpider.RestService
{
    public partial class vercodeimg : System.Web.UI.Page
    {
        public string src = string.Empty;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["vercode"] != null)
            {
                src = Request["vercode"];
            }
        }
    }
}