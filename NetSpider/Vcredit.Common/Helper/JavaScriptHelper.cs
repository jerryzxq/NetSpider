using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jint;
using Microsoft.JScript.Vsa;
using Noesis.Javascript;

namespace Vcredit.Common.Helper
{
    public class JavaScriptHelper
    {
        public static object JavaScriptEval(string strjs, string method)
        {
            object result = null;
            try
            {
                VsaEngine Engine = VsaEngine.CreateEngine();
                result = Microsoft.JScript.Eval.JScriptEvaluate(strjs+method, Engine);
            }
            catch (Exception e)
            { 
            }
            return result;
        }

        public static object JavaScriptExecute(string strjs, string method, object[] arrObject)
        {
            object result = null;
            try
            {
                var engine = new Engine().Execute(strjs);
                result = engine.Invoke(method, arrObject).ToObject();
            }
            catch (Exception)
            {
            }
            return result;
        }

        public static object NoesisJavaScriptExecute(string strjs, string getParameter)
        {
            object result = null;
            try
            {
                using (JavascriptContext context = new JavascriptContext())
                {
                    context.Run(strjs);

                    result = context.GetParameter(getParameter);
                }
            }
            catch (Exception)
            {
            }
            return result;
        }
    }
}
