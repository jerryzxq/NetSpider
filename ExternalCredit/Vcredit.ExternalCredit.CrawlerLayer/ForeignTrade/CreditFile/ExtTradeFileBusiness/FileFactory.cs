using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ExternalCredit.CrawlerLayer.ForeignTrade
{
    public sealed class FileFactory
    {
        public static AbastractFileBase CreateFile(string suffix)
        {
            AbastractFileBase fb = null;
            switch (suffix)
            {
                case "txt":
                    fb = new TxtFile();
                    break;
                default:
                    fb = new ExcelFile();
                    break;
            }
            return fb;
        }

        
    }
}
