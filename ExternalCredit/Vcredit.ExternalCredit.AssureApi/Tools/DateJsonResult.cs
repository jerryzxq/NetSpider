using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Web;
using System.Web.Mvc;

namespace Vcredit.ExternalCredit.AssureApi.Tools
{
    public class DateJsonResult : JsonResult
    {
        private readonly string _dateFormat;

        public DateJsonResult(string format = "yyyy-MM-dd") : base()
        {
            _dateFormat = format;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            HttpResponseBase response = context.HttpContext.Response;

            if (!String.IsNullOrEmpty(ContentType))
            {
                response.ContentType = ContentType;
            }
            else
            {
                response.ContentType = "application/json";
            }
            if (ContentEncoding != null)
            {
                response.ContentEncoding = ContentEncoding;
            }
            if (Data != null)
            {
                // Using Json.NET serializer
                var isoConvert = new IsoDateTimeConverter();
                isoConvert.DateTimeFormat = _dateFormat;
                response.Write(JsonConvert.SerializeObject(Data, isoConvert));
            }
        }
    }
}