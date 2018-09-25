using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.Attributes;
using Vcredit.ActivexLogin.Common;

namespace Vcredit.ActivexLogin.App.Business
{
    public class BusinessFactory
    {
        //public static WebSiteBizTemplate GenerateSiteBiz(ProjectEnums.WebSiteType siteType)
        //{
        //    WebSiteBizTemplate exuc = null;
        //    switch (siteType)
        //    {
        //        case ProjectEnums.WebSiteType.GuangZhouGjj:
        //            exuc = new GuangZhouGjjBizImpl();
        //            break;
        //        case ProjectEnums.WebSiteType.TianJinGjj:
        //            exuc = new TianJinGjjBizImpl();
        //            break;
        //        case ProjectEnums.WebSiteType.ShenZhenGjj:
        //            exuc = new ShenZhenGjjBizImpl();
        //            break;
        //        case ProjectEnums.WebSiteType.WuHanGjj:
        //            exuc = new WuHanGjjBizImpl();
        //            break;
        //        default:
        //            break;
        //    }

        //    if (exuc == null)
        //        throw new ArgumentException("没有找到合适的 WebSiteBiz 请检查 AppSetting ShowSites 参数是否正确");

        //    return exuc;
        //}

        public static WebSiteBizTemplate GenerateSiteBizV2(ProjectEnums.WebSiteType siteType)
        {
            try
            {
                WebSiteBizTemplate exuc = null;

                var assembly = Assembly.Load(typeof(BusinessFactory).Assembly.GetName().Name);
                var types = assembly.GetTypes().Where(x => x.BaseType == typeof(WebSiteBizTemplate)
                            && x.GetCustomAttribute(typeof(RestraintSiteAttribute)) != null
                            && (x.GetCustomAttribute(typeof(RestraintSiteAttribute)) as RestraintSiteAttribute).TargetWebSite == siteType);

                if (types == null || !types.Any())
                    throw new ArgumentException("没有找到合适的 WebSiteBiz，请检查 WebSiteBizTemplate 的实现类是否配置了 RestraintSiteAttribute 特性，或者检查 AppSetting ShowSites 参数是否正确");

                if (types.Count() > 1)
                    throw new ArgumentException("RestraintSiteAttribute 特性配置错误，当前 WebSiteType 配置了多个，请确保没有重复配置");

                exuc = (WebSiteBizTemplate)Activator.CreateInstance(types.FirstOrDefault(), null);
                return exuc;
            }
            catch (Exception ex)
            {
                throw new Exception("WebSiteBizTemplate 实力化失败", ex);
            }
        }
    }
}
