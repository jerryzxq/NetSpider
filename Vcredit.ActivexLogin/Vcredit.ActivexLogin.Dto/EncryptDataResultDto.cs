using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vcredit.ActivexLogin.Dto
{
    public enum EncryptStatus
    {
        NotFoundEncrypt = -1,

        Faild = 0,

        Success = 1,

    }
    public class EncryptDataResultDto
    {
        public EncryptStatus Reason { get; set; }

        public string ReasonDescription { get; set; }

        public object Data { get; set; }
    }
}
