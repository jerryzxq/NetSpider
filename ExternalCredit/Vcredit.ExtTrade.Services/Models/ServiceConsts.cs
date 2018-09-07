using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vcredit.ExtTrade.Services
{
    public class ServiceConsts
    {
  
        public const int StatusCode_success = 0;
        public const int StatusCode_fail = 1;
        public const int StatusCode_needvercode = 5;
        public const int StatusCode_error = 110;
        public const int StatusCode_httpfail = 500;
        public const int StatusCode_OutMaxRequestNum = 2;
        public const int StatusCode_HaveGet = 3;
        public const int StatusCode_NotExist = 4;
        public const int StatusCode_ReqError = 6;
    }
}