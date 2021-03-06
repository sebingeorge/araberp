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
        public string Remarks { get; set; }
        public string PartNo { get; set; }
        public decimal Quantity { get; set; }
        public string UnitName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int OrganizationId { get; set; }
        public int SlNo { get; set; }
        public bool isIssueUsed { get; set; }
        //public string WorkDescr { get; set; }
        public string ItemName { get; set; }
        public decimal? ActualQuantity { get; set; }
        public int isAddtionalMaterialRequest { get; set; }
      
    }
}
