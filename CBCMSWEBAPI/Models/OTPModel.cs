using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CBCMSWEBAPI.Models
{
    public class OTPModel
    {
        [Required]
        public string AppNo { get; set; }

        [Required]
        public string MobileNo { get; set; }
        
        [Required]
        public string EmailID { get; set; }

        [Required]
        public string Flag { get; set; }

        public string OTPType { get; set; }
    }
}