﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ArabErp.Domain
{
    public class CustomerCategory
    {
        public int CusCategoryId { get; set; }
        public string CusCategoryRefNo { get; set; }
        [Required]
        public string CusCategoryName { get; set; }
        [Required]
        public string CusCategoryShortName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
