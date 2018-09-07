using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Vcredit.NetSpider.Emall.Crawler.JingDong;
using Vcredit.Common.Helper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Vcredit.Common.Utility;
using System.Threading;
using System.Drawing;
using Vcredit.Framework.Queue.Redis;
using Vcredit.NetSpider.Emall.Dto.TaoBao;
using Vcredit.Framework.Server.Dto;
using Vcredit.NetSpider.Emall.Dto;
using System.Text.RegularExpressions;
using NSoup.Nodes;
using NSoup;
using Vcredit.NetSpider.Emall.Entity;
using Vcredit.NetSpider.Emall.Framework;
using Vcredit.NetSpider.Emall.Processor.Impl;
using Vcredit.NetSpider.Emall.Framework.Utility;
using Vcredit.Common.Ext;
using Vcredit.NetSpider.Emall.Crawler.JingDong.Mobile;
namespace ConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Program1.testjd1();
            Console.Read();
        }
    }

}
