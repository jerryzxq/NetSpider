using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Vcredit.ExternalCredit.CommonLayer
{
    public class UniversalFilePathResolver
    {
        public static string ResolvePath(string relativePath)
        {
            if (relativePath == null || !relativePath.StartsWith("~"))
            {
                throw new ArgumentException("The path '" + relativePath +
                    "' should be relative path and should start with '~'");
            }

            HttpContext httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                string fullPath = httpContext.Server.MapPath(relativePath);
                return fullPath;
            }
            else
            {
                // We are in a console / Windows desktop application -->
                // use currently executing assembly directory to find the full path
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assemblyDir = assembly.CodeBase;
                assemblyDir = assemblyDir.Replace("file:///", "");
                assemblyDir = Path.GetDirectoryName(assemblyDir);

                // Remove "bin\debug" and "bin\release" directories from the path
                string applicationDir = RemoveStringAtEnd(@"\bin\debug", assemblyDir);
                applicationDir = RemoveStringAtEnd(@"\bin\release", applicationDir);

                string fullPath = relativePath.Replace("~", applicationDir);
                return fullPath;
            }
        }

        private static string RemoveStringAtEnd(string searchStr, string targetStr)
        {
            if (targetStr.ToLower().EndsWith(searchStr.ToLower()))
            {
                string resultStr = targetStr.Substring(0, targetStr.Length - searchStr.Length);
                return resultStr;
            }
            return targetStr;
        }
    }
}
