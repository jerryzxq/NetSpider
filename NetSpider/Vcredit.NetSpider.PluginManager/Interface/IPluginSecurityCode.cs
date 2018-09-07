/*********************************************  
 * * 功能描述： 解析验证码的相关接口
 * * 创 建 人:  张志博
 * * 日    期:  2014/9/19
 * * 修 改 人:  
 * * 修改日期: 
 * * 修改描述:  
 * * 版    本:  1.0
 * *******************************************/ 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.PluginManager
{
    public enum CharSort
    {
        All=1001,
        Number=1002,
        Character = 1003,
        NumberAndUpper = 1004,
        NumberAndLower = 1005,
        CharacterUpper = 1006,
        CharacterLower = 1007
    }
    public interface IPluginSecurityCode
    {
        /// <summary>
        /// 对传入的验证码图片二进制流进行解析
        /// </summary>
        /// <param name="VerBytes">图片二进制</param>
        /// <returns></returns>
        string GetVerCode(byte[] VerBytes);
        /// <summary>
        /// 对传入的验证码图片二进制流进行解析
        /// </summary>
        /// <param name="VerBytes">图片二进制</param>
        /// <param name="CharSort">解析后的字符格式（枚举）</param>
        /// <returns></returns>
        string GetVerCodeByCharSort(byte[] VerBytes, CharSort CharSort);
        /// <summary>
        /// 通过悠悠云对传入的验证码图片二进制流进行解析
        /// </summary>
        /// <param name="VerBytes">图片二进制</param>
        /// <param name="CodeType">图片解析类型</param>
        /// <returns></returns>
        string GetVerCodeFromUUwise(byte[] VerBytes, int CodeType=0);

        
    }
}
