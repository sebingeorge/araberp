﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArabErp.Domain
{
    public class PendingWorkShopRequest
    {
        public int WorkShopRequestId { get; set; }
        public string WorkShopRequestRefNo { get; set; }
        public DateTime WorkShopRequestDate { get; set; }
        public string SaleOrderRefNo { get; set; }
        public DateTime SaleOrderDate { get; set; }
        public int CustomerId { get; set; }
        public string CustomerOrderRef { get; set; }
        public string CustomerName { get; set; }
        public DateTime RequiredDate { get; set; }
        public int Ageing { get; set; }
        public int DaysLeft { get; set; }

        public decimal Quantity { get; set; }
        public decimal PendingQuantity { get; set; }
    }
}
