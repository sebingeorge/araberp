﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SupplyOrderPreviousList
    {
        public int SupplyOrderId { get; set; }
        public int SupplyOrderItemId { get; set; }
        public string SupplyOrderNo { get; set; }
        public int SupplierId { get; set; }
        public string SupplierName { get; set; }
        public string QuotationNoAndDate { get; set; }
        public DateTime SupplyOrderDate { get; set; }
        public int RequestedQuantity { get; set; }
        public int SuppliedQuantity { get; set; }
        public int BalanceQuantity { get; set; }
        public decimal Amount { get; set; }
        public string ItemName { get; set; }
        public string PartNo { get; set; }
        public int GRNQty { get; set; }
        public string SettledReason { get; set; }
        public DateTime SettledDate { get; set; }
        public string SettledBy { get; set; }
        public string PurchaseRequestNo { get; set; }
    }
}
