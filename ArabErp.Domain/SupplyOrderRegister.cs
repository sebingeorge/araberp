﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class SupplyOrderRegister
    {
        public int SaleOrderId { get; set; }
        public string SupplyOrderNo { get; set; }
        public DateTime SupplyOrderDate { get; set; }
        public string SupplierName { get; set; }
        public string ItemName { get; set; }
        public int OrderedQty { get; set; }
        public string UnitName { get; set; }
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public decimal TotalAmount { get; set; }
        public int SettledQty { get; set; }
        public int ReceviedQty { get; set; }
        public int BalanceQty { get; set; }
        public string STATUS { get; set; }
        

    }
}

