﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SupplierCategory
    {
        public int SupCategoryId { get; set; }
        public string SupCategoryRefNo { get; set; }
        public string SupCategoryName { get; set; }
        public string SupCategoryShortName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
