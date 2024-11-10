using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace BaseLineProject.Models
{
    public class WelcomeRequest
    {
        public string ToEmail { get; set; }
        public string UserName { get; set; }
        public List<IFormFile> Attachments { get; set; }
        public decimal? custdisc { get; set; }
        public decimal? creditnoteval { get; set; }
        public string fileinvoice { get; set; }
        public string message { get; set; }
        public string callbackurl { get; set; }

    }
}
