﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace ArabErp.Domain
{
    public class Bay
    {

        public int BayId { get; set; }
        public string BayRefNo { get; set; }
        [Required]
        public string BayName { get; set; }
        [Required]
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}