using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Vcredit.WeiXin.RestService
{
    public class Utility
    {
        public static bool WriteAllBytes(string id, byte[] ResultByte)
        {
            try
            {
                System.IO.File.WriteAllBytes(HttpContext.Current.Server.MapPath(@"/files/" + id + ".jpg"), ResultByte);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}