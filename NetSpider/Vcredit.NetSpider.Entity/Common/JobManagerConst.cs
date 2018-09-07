using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity
{
    public class JobManagerConsts
    {
        #region 返回数据类型常量
        public const string GatherData_Type_add = "add";
        public const string GatherData_Type_new = "new";
        #endregion

        #region 参数类型常量
        public const string parameter_Type_fromreturn = "fromreturn";
        public const string parameter_Type_identity = "identity";
        public const string parameter_Type_webservice = "webservice";
        #endregion

        #region 数据转换类型常量
        public const string dataconvert_Type_contain = "contain";
        public const string dataconvert_Type_replace = "replace";
        #endregion
    }
}
