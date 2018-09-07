using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Vcredit.ExternalCredit.CrawlerLayer.NewForeignTrade
{
     [XmlRoot("Root")]
    public class UpLoadXmlFile
    {
        public Work Task { get; set; }

    }
    public class Work
    {
      
        public string SequenceId{ get; set; }
        public string  BranchCode { get; set; }
        public string  Source { get; set; }
        public string  OtherParam { get; set; }
        public List<Document> Documents { get; set; }


    }
    public  class Document
    {
      
        public string  Busstype{ get; set; }
        public string  Subtype { get; set; }
        public string  BussNo { get; set; }
        public int FilesCount { get; set; }
        public List<File> Files { get; set; } 
    }
    
    public class File
    {
        public string  Name { get; set; }
        public int FileOrder { get; set; }

    }

}
