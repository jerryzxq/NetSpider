using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vcredit.Common.Helper;
using Vcredit.NetSpider.Processor;
//using Winista.Text.HtmlParser;
//using Winista.Text.HtmlParser.Lex;
//using Winista.Text.HtmlParser.Util;
using Vcredit.NetSpider.Parser;
using Vcredit.NetSpider.Entity.JobManager;
//using Quartz;
//using Quartz.Impl;
using Vcredit.NetSpider.PluginManager;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Utility;
using System.IO;
using System.Management;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Xml.Serialization;
using Quartz;
using Quartz.Impl;
using Vcredit.Common.Ext;

namespace TestConsoleApp
{
    class Program
    {

        static void Main(string[] args)
        {
            SpiderTool.GetMobile();
            Console.WriteLine("1");
        }

    }
}
