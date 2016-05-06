﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
   public class WorkShopRequestItem
    {
        public int WorkShopRequestItemId { get; set; }
        public int WorkShopRequestId { get; set; }
        public int WorkShopRequestItemSlNo { get; set; }
        public int ItemId { get; set; }
        public string ItemDescription { get; set; }
        public string PartNo { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
    }
}
