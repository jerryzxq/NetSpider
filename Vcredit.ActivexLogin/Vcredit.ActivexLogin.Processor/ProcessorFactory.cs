using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.Processor
{
    public class ProcessorFactory
    {
        //public static IActivexLoginExecutor GenerateExecutor(ProjectEnums.WebSiteType siteType)
        //{
        //    IActivexLoginExecutor exuc = null;
        //    switch (siteType)
        //    {
        //        case ProjectEnums.WebSiteType.GuangZhouGjj:
        //            exuc = GuangZhouGjjExecutorImpl.Instance;
        //            break;
        //        case ProjectEnums.WebSiteType.TianJinGjj:
        //            exuc = TianJinGjjExecutorImpl.Instance;
        //            break;
        //        case ProjectEnums.WebSiteType.ShenZhenGjj:
        //            exuc = ShenZhenGjjExecutorImpl.Instance;
        //            break;
        //        case ProjectEnums.WebSiteType.WuHanGjj:
        //            exuc = WuHanGjjExecutorImpl.Instance;
        //            break;
        //        default:
        //            break;
        //    }

        //    if (exuc == null)
        //        throw new ArgumentException("没有找到合适的 ActivexLoginExecutor 请检查 siteType 参数是否正确");

        //    return exuc;
        //}

        public static IActivexLoginExecutor GenerateExecutorV2(ProjectEnums.WebSiteType siteType)
        {
            try
            {
                IActivexLoginExecutor exuc = null;

                var assembly = Assembly.Load(typeof(ProcessorFactory).Assembly.GetName().Name);
                var types = assembly.GetTypes().Where(x => x.GetInterface(typeof(IActivexLoginExecutor).Name) != null
                            && x.GetCustomAttribute(typeof(RestraintSiteAttribute)) != null
                            && (x.GetCustomAttribute(typeof(RestraintSiteAttribute)) as RestraintSiteAttribute).TargetWebSite == siteType);

                if (types == null || !types.Any())
                    throw new ArgumentException("没有找到合适的 WebSiteBiz，请检查 WebSiteBizTemplate 的实现类是否配置了 RestraintSiteAttribute 特性，或者检查 AppSetting ShowSites 参数是否正确");

                if (types.Count() > 1)
                    throw new ArgumentException("RestraintSiteAttribute 特性配置错误，当前 WebSiteType 配置了多个，请确保没有重复配置");

                exuc = (IActivexLoginExecutor)types.FirstOrDefault().GetField("Instance").GetValue(null);
                return exuc;
            }
            catch (Exception ex)
            {
                throw new Exception("IActivexLoginExecutor 实例化失败", ex);
            }
        }

    }
}
