using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vcredit.ActivexLogin.FrameWork;

namespace Vcredit.ActivexLogin.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var thread1 = new Thread(() =>
            {
                string requestStr = null;
                while (true)
                {
                    requestStr = RedisHelper.Dequeue<string>("72102385742474575266205500057137");

                    //DoRequest("{'SRand':'MTczNjQ3MDkyMg==','LoginType':'1','Name':'张**'}", WebSiteType.GuangZhouGjj);

                    //DoRequest("{'Randnum':'72102385742474575266205500057137'}", WebSiteType.TianJinGjj);

                    //DoRequest("", WebSiteType.ShenZhenGjj);
                }
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();

            Console.Read();
        }
    }
}
