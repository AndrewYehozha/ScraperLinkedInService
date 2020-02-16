using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScraperLinkedInService.Models.Response
{
    public class BaseResponse
    {
        public string ErrorMessage { get; set; }

        public int StatusCode { get; set; }
    }
}
