﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkShopRequest
    {
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestNo { get; set; }
        public DateTime WorkShopRequestDate { get; set; }
        public int SaleOrderId { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderRef { get; set; }
        public string SpecialRemarks { get; set; }
        public DateTime? RequiredDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}