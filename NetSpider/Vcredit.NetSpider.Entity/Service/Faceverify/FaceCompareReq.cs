using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vcredit.NetSpider.Entity.Service.Faceverify
{
    public class FaceCompareReq
    {
        public string Name { get; set; }
        public string IdentityCard { get; set; }
        public string PersonPhotoBase64 { get; set; }
        public string IdentityPhotoBase64 { get; set; }
    }
}
