using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity
{
    public static class GlobalEnums
    {
        public enum EnumLimitSign
        {
            NoLimit = 2001,          //不做任意格式的限制
            NoWebSign = 2002,        //匹配时去掉网页符号
            OnlyCN = 2003,           //只匹配中文
            OnlyDoubleByte = 2004,     //只匹配双字节字符
            OnlyNumber = 2005,         //只匹配数字
            OnlyChar = 2006,           //只匹配字母数字及常用字符
            CustomMatchChar = 2007,//自定义正则匹配表达式 
            Custom = 2008
        }
        public enum EnumFlowType
        {
           QueueExec = 1001,
           ChainedExec = 1002
        }
    }
    public enum EnumMobileCompany
    {
        ChinaMobile = 3001,
        ChinaNet = 3002,
        ChinaUnicom = 3003,
        JuXinLi = 3004,
    }

    public enum EnumMobileDeatilType
    {
        SMS,
        Call,
        Net,
        Other
    }
}
